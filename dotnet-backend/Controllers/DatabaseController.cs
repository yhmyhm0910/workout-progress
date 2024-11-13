using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly ILogger<DatabaseController> _logger;
    private readonly string _databaseUrl;
    private readonly Database _db;

    public DatabaseController(ILogger<DatabaseController> logger)
    {
        _logger = logger;
        _databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL") ?? throw new InvalidOperationException("DATABASE_URL environment variable is not set.");
        _db = new Database();
    }

    public class InsertUserDto
    {
        public string userId { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string refresh_token { get; set; }
    }
    public class UpdateRefreshTokenDto
    {
        public string userId { get; set; }
        public string refresh_token { get; set; }
    }
    public class isUserExistDto
    {
        public string userId { get; set; }
    }
    public class isRefreshTokenValidDto
    {
        public string refresh_token { get; set; }
    }

    [HttpGet("Select")]
    public async Task<IActionResult> Select()
    {
        if (_databaseUrl != null)
        {
            // if (Request.Cookies.TryGetValue("refresh_token", out string cookieValue))
            // {
            //     // Do something with the cookie value
            //     Console.WriteLine($"cookieValue: {cookieValue}");
            // }
            string query = @"
                SELECT * FROM Users
            ";
            // Execute the query and fetch the results
            var result = await _db.SelectAsync(_databaseUrl, query);

            // Return the result of the query
            return Ok(result);
        }

        return BadRequest("Database URL not found.");
    }

    [HttpPost("isRefreshTokenValid")]
    public async Task<object> isRefreshTokenValid([FromBody] isRefreshTokenValidDto userInfos)
    {
        if (_databaseUrl != null)
        {
            string refresh_token = userInfos.refresh_token; 
            string query = await System.IO.File.ReadAllTextAsync("SQL/isRefreshTokenValid_template.sql");
            // Execute the query and fetch the results
            var result = await _db.isRefreshTokenValidAsync(_databaseUrl, query, refresh_token);

            if (result.Count == 0) {
                Console.WriteLine($"No corresponding refresh_token found with refresh_token={refresh_token}");
                return false;
            }
            else
            {
                Console.WriteLine($"Found user with refresh_token={refresh_token}");
                return true;
            }
        }
        return BadRequest("Database URL not found.");
    }

    [HttpPost("InsertUser")]
    public async Task<IActionResult> InsertUser([FromBody] InsertUserDto userInfos)
    {
        if (_databaseUrl != null)
        {
            string userId = userInfos.userId; 
            string email = userInfos.email;
            string name = userInfos.name;
            string refresh_token = userInfos.refresh_token;
            string query = await System.IO.File.ReadAllTextAsync("SQL/insert_user_template.sql");
            
            // Execute the query and fetch the results
            var result = await _db.InsertUserAsync(_databaseUrl, query, userId, name, email, refresh_token);

            // Return the result of the query
            return Ok($"Inserted {result} row of user.");
        }

        return BadRequest("Database URL not found.");
    }

    [HttpPost("isUserExist")]
    public async Task<object> isUserExist([FromBody] isUserExistDto userInfos)
    {
        if (_databaseUrl != null)
        {
            string userId = userInfos.userId; 
            string query = await System.IO.File.ReadAllTextAsync("SQL/isUserExist_template.sql");
            // Execute the query and fetch the results
            var result = await _db.isUserExistAsync(_databaseUrl, query, userId);

            if (result.Count == 0) {
                Console.WriteLine($"No corresponding user found with userId={userId}");
                return false;
            } else
            {
                Console.WriteLine($"Found user with userId={userId}");
                return true;
            }
        }
        return BadRequest("Database URL not found.");
    }

    [HttpPost("InsertRecord")]
    public async Task<IActionResult> InsertRecord(string userId, string exerciseName, float oneRepMax, string unit)
    {
        if (_databaseUrl != null)
        {
            string query = await System.IO.File.ReadAllTextAsync("SQL/insert_record_template.sql");
            
            // Execute the query and fetch the results
            var result = await _db.InsertRecordAsync(_databaseUrl, query, userId, exerciseName, oneRepMax, unit);

            // Return the result of the query
            return Ok($"Inserted {result} row of record.");
        }

        return BadRequest("Database URL not found.");
    }

    [HttpPost("InsertStandard")]
    public async Task<IActionResult> InsertStandard()
    {
        if (_databaseUrl != null)
        {
            string query = await System.IO.File.ReadAllTextAsync("SQL/insert_standard_template.sql");
            
            // Execute the query and fetch the results
            var result = await _db.InsertStandardAsync(_databaseUrl, query);

            // Return the result of the query
            return Ok($"Inserted {result} row of record.");
        }

        return BadRequest("Database URL not found.");
    }

    [HttpDelete("DeleteStandard")]
    public async Task<IActionResult> DeleteStandard()
    {
        if (_databaseUrl != null)
        {
            string query = await System.IO.File.ReadAllTextAsync("SQL/delete_standard_template.sql");
            
            // Execute the query and fetch the results
            var result = await _db.DeleteStandardAsync(_databaseUrl, query);

            // Return the result of the query
            return Ok($"Deleted {result} row of standard");
        }

        return BadRequest("Database URL not found.");
    }

    [HttpPatch("UpdateRefreshToken")] //PATCH = partial update, PUT = whole update
    public async Task<object> UpdateUser([FromBody] UpdateRefreshTokenDto userInfos)
    {
        if (_databaseUrl != null)
        {   
            string query = await System.IO.File.ReadAllTextAsync("SQL/update_refresh_token_template.sql");
            string userId = userInfos.userId;
            string refresh_token = userInfos.refresh_token;
            // Execute the query and fetch the results
            var result = await _db.UpdateRefreshTokenAsync(_databaseUrl, query, userId, refresh_token);
            if (result != 0)
            {   
                // updated at least one user
                return true;
            }
            else
            {
                // did not update any user
                return false;
            }
        }

        return BadRequest("Database URL not found.");
    }

    [HttpDelete("DeleteUser")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (_databaseUrl != null)
        {
            string query = await System.IO.File.ReadAllTextAsync("SQL/delete_user_template.sql");
            
            // Execute the query and fetch the results
            var result = await _db.DeleteUserAsync(_databaseUrl, query, userId);

            // Return the result of the query
            return Ok($"Deleted {result} row of user");
        }

        return BadRequest("Database URL not found.");
    }

    [HttpDelete("DeleteRecord")]
    public async Task<IActionResult> DeleteRecord(int recordId)
    {
        if (_databaseUrl != null)
        {
            string query = await System.IO.File.ReadAllTextAsync("SQL/delete_record_template.sql");
            
            // Execute the query and fetch the results
            var result = await _db.DeleteRecordAsync(_databaseUrl, query, recordId);

            // Return the result of the query
            return Ok($"Deleted {result} row of record");
        }

        return BadRequest("Database URL not found.");
    }

    [HttpPatch("UpdateRecord")] //PATCH = partial update, PUT = whole update
    public async Task<IActionResult> UpdateRecord(int recordId, string exerciseName, float oneRepMax, string unit, DateTime dateOfExercise)
    {
        if (_databaseUrl != null)
        {
            string query = await System.IO.File.ReadAllTextAsync("SQL/update_record_template.sql");
            
            // Execute the query and fetch the results
            var result = await _db.UpdateRecordAsync(_databaseUrl, query, recordId, exerciseName, oneRepMax, unit, dateOfExercise);

            // Return the result of the query
            return Ok($"Updated {result} row of record");
        }

        return BadRequest("Database URL not found.");
    }

    [HttpPost("CreateTable")]
    public async Task<IActionResult> CreateTable()
    {
        if (_databaseUrl != null)
        {
            string query = await System.IO.File.ReadAllTextAsync("SQL/create_tables.sql");
            
            var result = await _db.CreateTableAsync(_databaseUrl, query);

            if (result)
            {
                return Ok("Tables created successfully.");
            }
            else
            {
                _logger.LogError("Error creating tables.");
                return StatusCode(500, "Error creating tables.");
            }
        }
        else
        {
            _logger.LogError("Database URL not found");
            return BadRequest("Database URL is missing");
        }
    }
}
