using Npgsql;
using workout_progress.Models;
using Newtonsoft.Json;

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

    public async Task<List<Dictionary<string, object>>> SelectUserByUserIDAsync(string databaseUrl, string query, string userId)
    {
        // List to store the result
        var result = new List<Dictionary<string, object>>();

        // Get the connection to Heroku PostgreSQL
        using var connection = GetConnection(databaseUrl);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("UserId", userId);
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

    public async Task<List<Dictionary<string, object>>> SelectRecordByUserIDAsync(string databaseUrl, string query, string userId)
    {
        // List to store the result
        var result = new List<Dictionary<string, object>>();

        // Get the connection to Heroku PostgreSQL
        using var connection = GetConnection(databaseUrl);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("UserId", userId);
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

    public async Task<List<Dictionary<string, object>>> SelectStandardByNameAsync(string databaseUrl, string query, string name)
    {
        // List to store the result
        var result = new List<Dictionary<string, object>>();

        // Get the connection to Heroku PostgreSQL
        using var connection = GetConnection(databaseUrl);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("Name", name);
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

    public async Task<List<Dictionary<string, object>>> isRefreshTokenValidAsync(string databaseUrl, string query, string refresh_token)
    {
        var result = new List<Dictionary<string, object>>();
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);
            
            command.Parameters.AddWithValue("RefreshToken", refresh_token);
            
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user: {ex.Message}");
            return result; // Indicating failure
        }
    }
    
    public async Task<List<Dictionary<string, object>>> isUserExistAsync(string databaseUrl, string query, string userId)
    {
        var result = new List<Dictionary<string, object>>();
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);
            
            command.Parameters.AddWithValue("UserId", userId);
            
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user: {ex.Message}");
            return result; // Indicating failure
        }
    }

    public async Task<int> InsertUserAsync(string databaseUrl, string query, string userId, string name, string email, string refresh_token)
    {
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);
            
            command.Parameters.AddWithValue("UserId", userId);
            command.Parameters.AddWithValue("Name", name);
            command.Parameters.AddWithValue("Email", email);
            command.Parameters.AddWithValue("RefreshToken", refresh_token);
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

    public async Task<int> InsertRecordAsync(string databaseUrl, string query, string userId, string exerciseName, float oneRepMax, float bodyWeight, int experience_month, string unit)
    {
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("UserId", userId);
            command.Parameters.AddWithValue("ExerciseName", exerciseName);
            command.Parameters.AddWithValue("OneRepMax", oneRepMax);
            command.Parameters.AddWithValue("BodyWeight", bodyWeight);
            command.Parameters.AddWithValue("ExperienceMonth", experience_month);
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

    public async Task<int> InsertStandardAsync(string databaseUrl, string query)
    {
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);

            string jsonFilePath = "./BaseExercises.json";
            string jsonString = await File.ReadAllTextAsync(jsonFilePath);
            Exercises_NameLiftsAndStandards data = JsonConvert.DeserializeObject<Exercises_NameLiftsAndStandards>(jsonString);

            if (data != null && data.ExercisesFound != null && data.ExercisesFound.Name != null && data.MaleData != null)
            {
                for (int i=0; i<data.ExercisesFound.Name.Count; i++)
                {
                    Console.WriteLine(data.ExercisesFound.Name[i]);
                    string[,] lbs_per_BW = new string[5, 21];

                    int idx = 0;
                    foreach (var md_allBW in data.MaleData[i])
                    {
                        Console.WriteLine("BW: " + md_allBW[0]);
                        Console.WriteLine("Beginner: " + md_allBW[1]);
                        for (int j=0; j<lbs_per_BW.GetLength(0); j++) 
                        {
                            lbs_per_BW[j,idx] = md_allBW[j+1];
                        }
                        // Console.WriteLine("Novice: " + md_allBW[2]);
                        // Console.WriteLine("Intermediate: " + md_allBW[3]);
                        // Console.WriteLine("Advanced: " + md_allBW[4]);
                        // Console.WriteLine("Elite: " + md_allBW[5]);
                        // lbs_per_BW[idx,1] = md_allBW[1];
                        idx++;
                    }
                    Console.WriteLine("lbs_per_BW: " + string.Join(", ", lbs_per_BW));
                    for (int j=0; j<lbs_per_BW.GetLength(0); j++)
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("Name", data.ExercisesFound.Name[i]);
                        switch (j)
                        {
                            case 0:
                                command.Parameters.AddWithValue("Level", "Beginner");
                                break;
                            case 1:
                                command.Parameters.AddWithValue("Level", "Novice");
                                break;
                            case 2:
                                command.Parameters.AddWithValue("Level", "Intermediate");
                                break;
                            case 3:
                                command.Parameters.AddWithValue("Level", "Advanced");
                                break;
                            case 4:
                                command.Parameters.AddWithValue("Level", "Elite");
                                break;
                            default:
                                Console.WriteLine("Error detecting level of standard (Beginner, Novice, Intermediate, Advanced, or Elite)");
                                return 0;
                        }
                        for (int k=110; k<=310; k += 10) 
                        {
                            int idx_weight_by_lbs = (k-110)/10;
                            Console.WriteLine($"lbs_per_BW[idx_weight_by_lbs]: {lbs_per_BW[j,idx_weight_by_lbs]}");
                            command.Parameters.AddWithValue(k.ToString(), lbs_per_BW[j,idx_weight_by_lbs]);
                        };

                        // Execute the INSERT query and return the number of affected rows
                        int affectedRows = await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"affectedRows: {affectedRows}");
                    }

                }
            }
            
            return 2;


            // return affectedRows;  // Return the number of rows inserted (should typically be 1 for a single insert)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inserting record: {ex.Message}");
            return 0; // Indicating failure
        }
    }

    public async Task<int> UpdateRefreshTokenAsync(string databaseUrl, string query, string userId, string refresh_token)
    {
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("RefreshToken", (object)refresh_token ?? DBNull.Value);
            command.Parameters.AddWithValue("UserID", userId);

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

    public async Task<int> DeleteStandardAsync(string databaseUrl, string query)
    {
        try
        {
            using var connection = GetConnection(databaseUrl);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);

            // Execute the INSERT query and return the number of affected rows
            int affectedRows = await command.ExecuteNonQueryAsync();

            return affectedRows;  // Return the number of rows inserted (should typically be 1 for a single insert)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting standard: {ex.Message}");
            return 0; // Indicating failure
        }
    }

    public async Task<int> DeleteUserAsync(string databaseUrl, string query, string userId)
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
