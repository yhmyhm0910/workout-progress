using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[ApiController]
[Route("api/[controller]")]
public class WebScrap_ExercisesController : ControllerBase
{
    private readonly ILogger<WebScrap_ExercisesController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public WebScrap_ExercisesController(ILogger<WebScrap_ExercisesController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("exercises")]
    public async Task<IActionResult> GetExercises()
    {
        var serverUrl = Environment.GetEnvironmentVariable("SERVER_URL");
        var scrap = new Scraper();

        var exercisesFound = scrap.ScrapExercisesNameandLifts();

        // Call exercise-specific APIs and store them
        List<List<List<string>>> allStandards = new List<List<List<string>>>();
        Random random = new Random();

        for (int i = 0; i < 287; i++)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var fetchMaleDataResponse = await httpClient.GetAsync($"{serverUrl}fetchMaleData/{exercisesFound.exercisesFound_URL.Name[i]}");

            if (!fetchMaleDataResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching male data: {StatusCode}", fetchMaleDataResponse.StatusCode);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            // Read the response content from /fetchMaleData
            string maleDataContent = await fetchMaleDataResponse.Content.ReadAsStringAsync();
            if (maleDataContent != null)
            {
                List<List<string>> maleDataContentJson = JsonConvert.DeserializeObject<List<List<string>>>(maleDataContent);
                allStandards.Add(maleDataContentJson);
            }

            // Log the data for debugging
            _logger.LogInformation($"[{i}] Exercise: {exercisesFound.exercisesFound_display.Name[i]}, Fetched Male Data: {maleDataContent}");
            await Task.Delay(random.Next(10000, 15001)); // Use Task.Delay for async
            if (i % 10 == 0 && i != 0)
            {
                await Task.Delay(random.Next(10000, 15001));
            }
        }

        // Return both /exercises data and /fetchMaleData data
        var responseData = new
        {
            ExercisesFound = exercisesFound.exercisesFound_display,
            MaleData = allStandards
        };

        string json = JsonConvert.SerializeObject(responseData, Formatting.Indented);
        await System.IO.File.WriteAllTextAsync("BaseExercises.json", json); // Use async file write

        // Return the table data as a JSON response
        return Ok(json);
    }

    [HttpGet("fetchMaleData/{exerciseName}")]
    public IActionResult FetchMaleData(string exerciseName)
    {
        var scraper = new Scraper();
        var result = scraper.ScrapExercises(exerciseName);
        return Ok(result);
    }
}
