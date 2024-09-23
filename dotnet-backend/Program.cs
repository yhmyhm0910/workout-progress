using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using workout_progress.Models;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddConsole();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/exercises", async (ILogger<Program> logger, IHttpClientFactory httpClientFactory) =>
{
    // Configure ChromeOptions to run in headless mode
    var options = new ChromeOptions();
    options.AddArgument("headless");
    options.AddArgument("--no-sandbox");
    options.AddArgument("--disable-dev-shm-usage");

    // Initialize ChromeDriver
    using (IWebDriver driver = new ChromeDriver(options))
    {
        // Navigate to the target page
        driver.Navigate().GoToUrl("https://strengthlevel.com/strength-standards");
        IWebElement moreExerciseButton = driver.FindElement(By.XPath("//button[contains(text(), 'More Exercises...')]"));

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        IWebElement element = wait.Until(drv =>
            {
                IWebElement el = drv.FindElement(By.XPath("//button[contains(text(), 'More Exercises...')]"));
                return (el.Displayed && el.Enabled) ? el : null;
            });

        // Click button
        for (int i=0; i<23; i++)
        {
            element.Click();
            Thread.Sleep(500);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
        }


        IWebElement exercisesDiv = driver.FindElement(By.XPath("//div[contains(@class, 'columns')]"));

        //logger.LogInformation("exercisesDiv Text: {Text}", exercisesDiv.Text);

        var exercisesFound = new Exercises();

        int counter = 0;
        string tempStr = "";
        foreach (char c in exercisesDiv.Text)
        {
            if (c == '\n')
            {
                if (counter % 2 == 0)
                {
                    exercisesFound.Name.Add(tempStr);  
                }
                else
                {
                    exercisesFound.Lifts.Add(tempStr);  
                };
                tempStr = "";
                counter++;
            }
            else
            {
                tempStr += c;
            };
        };

        // make names to be URL-ready
        var exercisesFound_stripped = new Exercises();
        foreach (string exercise in exercisesFound.Name) 
        {
            string transformed = string.Concat(exercise.Select(c => c == ' ' ? '-' : char.ToLower(c)));
            exercisesFound_stripped.Name.Add(transformed);
        }

        //logger.LogInformation("List Contents:\n{ListString}", string.Join("\n", exercisesFound_stripped.Name));

        driver.Quit();

        // Call exercise-specific APIs and store them
        List<List<List<string>>> all_standards = [];
        // for (int i=0; i<exercisesFound_stripped.Name.Count; i++)
        Random random = new Random();
        for (int i=0; i<287; i++)
        {
            var httpClient = httpClientFactory.CreateClient();
            var fetchMaleDataResponse = await httpClient.GetAsync($"http://localhost:5166/fetchMaleData/{exercisesFound_stripped.Name[i]}");

            if (!fetchMaleDataResponse.IsSuccessStatusCode)
            {
                logger.LogError("Error fetching male data: {StatusCode}", fetchMaleDataResponse.StatusCode);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            // Read the response content from /fetchMaleData
            string maleDataContent = await fetchMaleDataResponse.Content.ReadAsStringAsync();
            if (maleDataContent != null)
            {
                List<List<string>> maleDataContent_json = JsonConvert.DeserializeObject<List<List<string>>>(maleDataContent);
                all_standards.Add(maleDataContent_json);
            }

            // Log the data for debugging
            logger.LogInformation($"[{i}] Exercise: {exercisesFound_stripped.Name[i]}, Fetched Male Data: {maleDataContent}");
            Thread.Sleep(random.Next(100000, 150001));
            if (i%10 == 0 && i != 0)
            {
                Thread.Sleep(random.Next(100000, 150001));
            }
        }

        // Return both /exercises data and /fetchMaleData data
        var responseData = new
        {
            ExercisesFound = exercisesFound,
            MaleData = all_standards
        };

        string json = JsonConvert.SerializeObject(responseData, Formatting.Indented);
        File.WriteAllText("BaseExercises.json", json);

        // Return the table data as a JSON response
        return Results.Json(json);
    }
})
.WithName("Exercises")
.WithOpenApi();

app.MapGet("/fetchMaleData/{exerciseName}", (ILogger<Program> logger, string exerciseName) =>
{
    var options = new ChromeOptions();
    options.AddArgument("headless"); // Run Chrome in headless mode (no UI)
    options.AddArgument("--no-sandbox");
    options.AddArgument("--disable-dev-shm-usage");

    // Initialize the ChromeDriver
    using (IWebDriver driver = new ChromeDriver(options))
    {
        // Navigate to the page
        driver.Navigate().GoToUrl($"https://strengthlevel.com/strength-standards/{exerciseName}");

        // Locate the table by XPath
        IWebElement table = driver.FindElement(By.XPath("//table[thead//th[contains(., 'BW')]]"));

        // Initialize a list to hold table data
        var tableData = new List<List<string>>();

        // Extract data from each row and column
        var rows = table.FindElements(By.TagName("tr"));
        foreach (var row in rows)
        {
            var columns = row.FindElements(By.TagName("td"));
            if (columns.Count > 0)
            {
                var rowData = columns.Select(column => column.Text).ToList();
                tableData.Add(rowData);
            }
        }

        // Close the browser
        driver.Quit();

        // Return the collected table data as a JSON response
        return Results.Json(tableData);
    }
})
.WithName("FetchMaleData")
.WithOpenApi();

app.Run();