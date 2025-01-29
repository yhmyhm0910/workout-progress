using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;

public class Login
{
    public async Task<JObject> ExchangeCodeForTokensAsync(string client_id, string client_secret, string authorizationCode, string redirect_uri)
    {
        using (var httpClient = new HttpClient())
        {
            // Prepare the request body
            var requestBody = new Dictionary<string, string>
            {
                { "client_id", client_id },
                { "client_secret", client_secret },
                { "code", authorizationCode },
                { "redirect_uri", redirect_uri },
                { "grant_type", "authorization_code" }
            };

            var requestContent = new FormUrlEncodedContent(requestBody);

            // Send the request to Google's token endpoint
            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            JObject responseContent_json = JObject.Parse(responseContent);
            Console.WriteLine($"responseContent: {responseContent_json}");
            Console.WriteLine($"responseContent.expires_in: {responseContent_json["expires_in"]}");
            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                return responseContent_json;
            }
            else
            {
                throw new Exception("Token exchange failed: " + responseContent);
            }
        }
    }
}