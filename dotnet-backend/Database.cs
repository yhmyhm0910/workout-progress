using Npgsql;

public class Database
{
    // Method to handle SELECT queries and return the result
    public async Task<List<Dictionary<string, object>>> SelectAsync(string databaseUrl, string query)
    {
        // List to store the result
        var result = new List<Dictionary<string, object>>();

        // Get the connection to Heroku PostgreSQL
        using var connection = GetConnection(databaseUrl);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(query, connection);
        
        // Execute the SELECT query and get the result
        using var reader = await command.ExecuteReaderAsync();

        // Read each row
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();

            // Loop through each column in the row
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.GetValue(i);
            }

            result.Add(row);
        }

        return result;
    }

    public async Task<int> InsertUserAsync(string databaseUrl, string query, int userId, string exerciseName, string email)
    {
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("UserId", userId);
            command.Parameters.AddWithValue("Name", exerciseName);
            command.Parameters.AddWithValue("Email", email);
            DateTime now = DateTime.Now;
            command.Parameters.AddWithValue("Created_at", now);

            // Execute the INSERT query and return the number of affected rows
            int affectedRows = await command.ExecuteNonQueryAsync();

            return affectedRows;  // Return the number of rows inserted (should typically be 1 for a single insert)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inserting user: {ex.Message}");
            return 0; // Indicating failure
        }
    }

    public async Task<int> InsertRecordAsync(string databaseUrl, string query, int userId, string exerciseName, float oneRepMax, string unit)
    {
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("UserId", userId);
            command.Parameters.AddWithValue("ExerciseName", exerciseName);
            command.Parameters.AddWithValue("OneRepMax", oneRepMax);
            command.Parameters.AddWithValue("Unit", unit);
            DateTime now = DateTime.Now;
            command.Parameters.AddWithValue("DateOfExercise", now);

            // Execute the INSERT query and return the number of affected rows
            int affectedRows = await command.ExecuteNonQueryAsync();

            return affectedRows;  // Return the number of rows inserted (should typically be 1 for a single insert)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inserting record: {ex.Message}");
            return 0; // Indicating failure
        }
    }

    public async Task<int> DeleteUserAsync(string databaseUrl, string query, int userId)
    {
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("UserId", userId);

            // Execute the INSERT query and return the number of affected rows
            int affectedRows = await command.ExecuteNonQueryAsync();

            return affectedRows;  // Return the number of rows inserted (should typically be 1 for a single insert)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user: {ex.Message}");
            return 0; // Indicating failure
        }
    }

    public async Task<int> DeleteRecordAsync(string databaseUrl, string query, int recordId)
    {
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("RecordId", recordId);

            // Execute the INSERT query and return the number of affected rows
            int affectedRows = await command.ExecuteNonQueryAsync();

            return affectedRows;  // Return the number of rows inserted (should typically be 1 for a single insert)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting record: {ex.Message}");
            return 0; // Indicating failure
        }
    }

    public async Task<int> UpdateRecordAsync(string databaseUrl, string query, int recordId, string exerciseName, float oneRepMax, string unit, DateTime dateOfExercise)
    {
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("ExerciseName", (object)exerciseName ?? DBNull.Value);
            command.Parameters.AddWithValue("OneRepMax", (object)oneRepMax ?? DBNull.Value);
            command.Parameters.AddWithValue("Unit", (object)unit ?? DBNull.Value);
            command.Parameters.AddWithValue("DateOfExercise", (object)dateOfExercise ?? DBNull.Value);
            command.Parameters.AddWithValue("RecordID", recordId);

            // Execute the INSERT query and return the number of affected rows
            int affectedRows = await command.ExecuteNonQueryAsync();

            return affectedRows;  // Return the number of rows inserted (should typically be 1 for a single insert)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting record: {ex.Message}");
            return 0; // Indicating failure
        }
    }

    public async Task<bool> CreateTableAsync(string databaseUrl, string query)
    {
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);
            await command.ExecuteNonQueryAsync();

            return true; // Indicating success
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating tables: {ex.Message}");
            return false; // Indicating failure
        }
    }

    public async Task EditDataAsync(string databaseUrl, string query)
    {
        // Get the connection to Heroku PostgreSQL
        using var connection = GetConnection(databaseUrl);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(query, connection);

        // Execute the query (can be INSERT, UPDATE, DELETE)
        int affectedRows = await command.ExecuteNonQueryAsync();
        Console.WriteLine($"{affectedRows} rows affected.");
    }

    public static NpgsqlConnection GetConnection(string databaseUrl)
    {

        // Parse the database URL and convert it to a Npgsql connection string
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        var npgsqlConnectionString = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Username = userInfo[0],
            Password = userInfo[1],
            Database = uri.LocalPath.TrimStart('/'),
            SslMode = SslMode.Require,   // Heroku PostgreSQL requires SSL
            TrustServerCertificate = true // Accept the self-signed SSL certificate
        }.ToString();

        // Create a new connection using Npgsql
        return new NpgsqlConnection(npgsqlConnectionString);
    }
}
