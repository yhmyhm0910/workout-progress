using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web;


[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string serverUrl;
    private readonly HttpClient _httpClient;
    private readonly string _clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
    private readonly string _clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
    private readonly string _redirectUri;
    private readonly string _scope = "https://www.googleapis.com/auth/userinfo.profile";
    // private readonly string _responseType = "code";
    // private readonly string _accessType = "offline";
    // private readonly string _prompt = "consent";
    private readonly Login _login;

    private void SetTokenCookie(string cookieName, string tokenValue, int expiresInSeconds)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Set to true for HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds)
        };

        Response.Cookies.Append(cookieName, tokenValue, cookieOptions);
    }

    public class TokensDto
    {
        public string id_token { get; set; }
        public string refresh_token { get; set; }
    }

    public class JWTToken2ProfileDto
    {
        public string userId { get; set;}
        public string email { get; set; }
        public string name { get; set; }
        public long exp { get; set; }
    }

    public LoginController(IHttpClientFactory httpClientFactory, HttpClient httpClient)
    {
        _httpClientFactory = httpClientFactory;
        _httpClient = httpClient;
        _login = new Login();
        serverUrl = Environment.GetEnvironmentVariable("SERVER_URL");
        _redirectUri = $"{serverUrl}api/Login/oauth/callback";
    }

    private async Task<HttpResponseMessage> PostRequestToDBConnection(string url, object body)
    {   
        var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync(url, content);
        return response;
    }

    private async Task<HttpResponseMessage> PatchRequestToDBConnection(string url, object body)
    {   
        var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = content
        };
        var response = await httpClient.SendAsync(request);
        return response;
    }

    private JWTToken2ProfileDto JWTToken2Profile(string JWT_token)
    {
        // Decode the JWT
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(JWT_token);
        // Access claims directly
        if (jwtToken.Payload.TryGetValue("sub", out var UserIdObj) && 
        jwtToken.Payload.TryGetValue("email", out var EmailObj) && 
        jwtToken.Payload.TryGetValue("name", out var NameObj) &&
        jwtToken.Payload.TryGetValue("exp", out var ExpireTimeStamp)) 
        {
            var userId = UserIdObj as string;
            var email = EmailObj as string;
            var name = NameObj as string;
            var unixTimestampSeconds = (long)ExpireTimeStamp;
            DateTime expireDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestampSeconds).UtcDateTime;
            DateTime expireDateTime_local = expireDateTime.ToLocalTime();
            DateTime now = DateTime.Now;
            Console.WriteLine($"UserId: {userId}");
            Console.WriteLine($"Email: {email}");
            Console.WriteLine($"Name: {name}");
            Console.WriteLine($"Expire time: {expireDateTime_local}");
            Console.WriteLine($"Time now: {now}");

            return new JWTToken2ProfileDto{
                userId = userId, 
                email = email,
                name = name,
                exp = unixTimestampSeconds
            };
        }
        else
        {
            return null;    
        }
    }

    [HttpGet("Oauth/Callback")]
    public async Task<IActionResult> ExchangeOAuthCodeForTokens()
    {
        // Log the incoming request details
        Console.WriteLine("Redirect URI hit");

        // Example: Retrieve a value from the session
        var oAuthToken = HttpContext.Session.GetString("OAuthToken");
        Console.WriteLine($"Session string: {oAuthToken}");
        // Retrieve query parameters
        var code = HttpContext.Request.Query["code"];
        var state = HttpContext.Request.Query["state"];

        // Print the parameters (you can log them or process them as needed)
        Console.WriteLine($"Code: {code}");
        Console.WriteLine($"State: {state}");

        if (string.IsNullOrEmpty(code))
        {
            return BadRequest("Authorization code not provided.");
        }

        try
        {
            // Exchange the code for tokens
            var tokens = await _login.ExchangeCodeForTokensAsync(_clientId, _clientSecret, code, _redirectUri);
            string access_token = tokens["access_token"].ToString();
            string refresh_token = tokens["refresh_token"].ToString();
            string id_token = tokens["id_token"].ToString();
            //TODO: update refresh token in DB (different everytime login)
            JWTToken2ProfileDto profile = JWTToken2Profile(id_token);
            var isUserExist = await PostRequestToDBConnection($"{serverUrl}api/Database/isUserExist", new {profile.userId});
            if (isUserExist.IsSuccessStatusCode)
            {
                var isUserExist_resultString = await isUserExist.Content.ReadAsStringAsync();
                bool isUserExistExists = bool.Parse(isUserExist_resultString);
                if (!isUserExistExists)
                {
                    try
                    {
                        var body = new
                        {
                            id_token,    // for identifying user only
                            refresh_token
                        };
                              
                        var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            
                        var httpClient = _httpClientFactory.CreateClient();
                        var response = await httpClient.PostAsync($"{serverUrl}api/Login/InsertDB_UserProfile", content);
                        if (response.IsSuccessStatusCode)
                        {
                            var successfulInsert = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Inserted new user successfully. ID token: {id_token}, refresh_token: {refresh_token}");
                        }
                        else
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            return Ok(new { error = $"Error: {response.StatusCode}, Details: {responseContent}" });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Insert user to db failed: {ex.Message}");
                        return StatusCode(500, "Insert user to db failed.");
                    };
                }
                else
                {
                    // if user exists, update the corr. user db
                    var isSuccessUpdateRefreshToken = await PatchRequestToDBConnection($"{serverUrl}api/Database/UpdateRefreshToken", new {
                        userId = profile.userId,
                        refresh_token
                    });
                    if (isSuccessUpdateRefreshToken.IsSuccessStatusCode)
                    {
                        var isSuccessUpdateRefreshToken_resultString = await isSuccessUpdateRefreshToken.Content.ReadAsStringAsync();
                        bool isSuccessUpdateRefreshTokens = bool.Parse(isSuccessUpdateRefreshToken_resultString);
                        if (isSuccessUpdateRefreshTokens)
                        {
                            Console.WriteLine($"Successfully updated the refresh token with refresh_token={refresh_token}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to update the refresh token in the database.");
                    }
                }
            }
            HttpContext.Session.SetString("access_token", access_token);
            HttpContext.Session.SetString("id_token", id_token);
            int expires_in_one_hour = Convert.ToInt32(tokens["expires_in"]);
            int expires_in_one_year = expires_in_one_hour * 24 * 365;

            
            // Redirect the user to the frontend with the tokens now stored in cookies
            SetTokenCookie("refresh_token", refresh_token, expires_in_one_year);
            return Redirect("http://localhost:4200/#/dashboard");
            // return Ok(new { Message = "Tokens retrieved successfully", Tokens = tokens });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token exchange failed: {ex.Message}");
            return StatusCode(500, "Token exchange failed.");
        }
    }

    // Called when frontend init
    [HttpGet("GetAccessAndIDToken")]
    public async Task<IActionResult> GetAccessAndIDToken()
    {
        if (Request.Cookies.TryGetValue("refresh_token", out string refresh_token))
        {
            Console.WriteLine("refresh_token found in cookies: " + refresh_token);
            // Verifies if the refresh_token exists in DB or not
            var response = await PostRequestToDBConnection($"{serverUrl}api/Database/isRefreshTokenValid", new {refresh_token});
            if (response.IsSuccessStatusCode)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                bool isRefreshTokenExists = bool.Parse(resultString);
                if (!isRefreshTokenExists)
                {
                    Console.WriteLine($"Refresh Token does not exist. refresh_token={refresh_token}");
                    return Ok(new { Message = "Refresh token does not exist"});
                }
                else
                {
                    var access_token = HttpContext.Session.GetString("access_token");
                    var id_token = HttpContext.Session.GetString("id_token");
                    if (access_token != null && id_token != null)
                    {
                        Console.WriteLine($"Refresh token validated. Giving frontend access_token={access_token} and id_token={id_token}");
                        return Ok(new { message = "refresh token validated.", access_token, id_token });
                    }
                    else
                    {
                        Console.WriteLine("Refresh token validated but cannot fetch access_token and id_token. Error: access_token == null or id_token == null");
                        return Ok(new {message = "Refresh token validated but cannot fetch access_token and id_token."});
                    }
                }
            }
            else
            {
                Console.WriteLine("Communication with DB failed.");
            }
        }
        return Ok(new {message = "Cannot find refresh_token."});
    }

    [HttpPost("tryPost_oauth_refresh")]
    public Uri TryPost_oauth_refresh()
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["client_id"] = _clientId;
        query["redirect_uri"] = _redirectUri;
        query["scope"] = _scope; // Ensure scope covers the data you need
        query["response_type"] = "code";
        query["access_type"] = "offline"; // Requests a refresh token
        query["prompt"] = "consent"; // Ensures the consent screen is shown

        var uriBuilder = new UriBuilder("https://accounts.google.com/o/oauth2/v2/auth")
        {
            Query = query.ToString()
        };

        return uriBuilder.Uri; // User must navigate to this URL
    }


    [HttpPost("InsertDB_UserProfile")]
    public async Task<IActionResult> InsertDB_UserProfile([FromBody] TokensDto tokens)
    {
        string id_token = tokens.id_token;
        string refresh_token = tokens.refresh_token;
    
        // Decode the JWT
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(id_token);
        // Access claims directly
        if (jwtToken.Payload.TryGetValue("sub", out var UserIdObj) && 
        jwtToken.Payload.TryGetValue("email", out var EmailObj) && 
        jwtToken.Payload.TryGetValue("name", out var NameObj) &&
        jwtToken.Payload.TryGetValue("exp", out var ExpireTimeStamp)) 
        {
            var userId = UserIdObj as string;
            var email = EmailObj as string;
            var name = NameObj as string;
            var unixTimestampSeconds = (long)ExpireTimeStamp;
            DateTime expireDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestampSeconds).UtcDateTime;
            DateTime expireDateTime_local = expireDateTime.ToLocalTime();
            DateTime now = DateTime.Now;
            Console.WriteLine($"UserId: {userId}");
            Console.WriteLine($"Email: {email}");
            Console.WriteLine($"Name: {name}");
            Console.WriteLine($"Expire time: {expireDateTime_local}");
            Console.WriteLine($"Time now: {now}");

            if (userId != null && email != null && name != null && refresh_token != null) {
                var httpClient = _httpClientFactory.CreateClient();
                // is existing user?
                var isUserExist_body = new
                {userId};
                
                var isUserExist_content = new StringContent(JsonConvert.SerializeObject(isUserExist_body), Encoding.UTF8, "application/json");
                var isUserExist = await httpClient.PostAsync($"{serverUrl}api/Database/isUserExist", isUserExist_content);   
                if (isUserExist.IsSuccessStatusCode) 
                {
                    var resultString = await isUserExist.Content.ReadAsStringAsync();
                    bool isUserExists = bool.Parse(resultString);
                    if (isUserExists)
                    {
                        var returnJson = new {
                            message = "User exists",
                            name = name
                        };
                        return Ok(returnJson);
                    }
                    else
                    {
                        var insertNewUser_body = new
                        {
                            userId = userId,
                            email = email,
                            name = name,
                            refresh_token = refresh_token
                        };
                        
                        var insertNewUser_content = new StringContent(JsonConvert.SerializeObject(insertNewUser_body), Encoding.UTF8, "application/json");
            
                        var insertNewUser = await httpClient.PostAsync($"{serverUrl}api/Database/InsertUser", insertNewUser_content);
                        if (insertNewUser.IsSuccessStatusCode)
                        {
                            var returnJson = new {
                                message = "Successfully inserted new user.",
                                name = name
                            };
                            return Ok(returnJson);
                        }
                        else
                        {
                            var responseContent = await insertNewUser.Content.ReadAsStringAsync();
                            return Ok(new { error = $"Error: {insertNewUser.StatusCode}, Details: {responseContent}" });
                        }
                    }
                }
                else
                {
                    var responseContent = await isUserExist.Content.ReadAsStringAsync();
                    return Ok(new { error = $"Error: {isUserExist.StatusCode}, Details: {responseContent}" });
                }
            }
        }
        else
        {
            Console.WriteLine("Email or sub (UserId) or Name claim not found in the token.");
            return Ok("Failed inserting new user.");
        }

        // Print the whole payload for reference
        // var payload = JsonSerializer.Serialize(jwtToken.Payload, new JsonSerializerOptions { WriteIndented = true });
        // Console.WriteLine("Full Payload:");
        // Console.WriteLine(payload);
        return Ok("0");
    }
}