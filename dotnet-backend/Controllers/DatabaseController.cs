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

    [HttpGet("Select")]
    public async Task<IActionResult> Select()
    {
        if (_databaseUrl != null)
        {
            string query = @"
                SELECT * FROM exercises_record
            ";
            
            // Execute the query and fetch the results
            var result = await _db.SelectAsync(_databaseUrl, query);

            // Return the result of the query
            return Ok(result);
        }

        return BadRequest("Database URL not found.");
    }

    [HttpPost("InsertUser")]
    public async Task<IActionResult> InsertUser(int userId, string name, string email)
    {
        if (_databaseUrl != null)
        {
            string query = await System.IO.File.ReadAllTextAsync("SQL/insert_user_template.sql");
            
            // Execute the query and fetch the results
            var result = await _db.InsertUserAsync(_databaseUrl, query, userId, name, email);

            // Return the result of the query
            return Ok($"Inserted {result} row of user.");
        }

        return BadRequest("Database URL not found.");
    }

    [HttpPost("InsertRecord")]
    public async Task<IActionResult> InsertRecord(int userId, string exerciseName, float oneRepMax, string unit)
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

    [HttpDelete("DeleteUser")]
    public async Task<IActionResult> DeleteUser(int userId)
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
