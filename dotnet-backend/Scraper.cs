using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using workout_progress.Models;

public class Scraper
{
    public (Exercises_NameandLifts exercisesFound_display, Exercises_NameandLifts exercisesFound_URL) ScrapExercisesNameandLifts()
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

        var exercisesFound = new Exercises_NameandLifts();

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
        var exercisesFound_stripped = new Exercises_NameandLifts();
        foreach (string exercise in exercisesFound.Name) 
        {
            string transformed = string.Concat(exercise.Select(c => c == ' ' ? '-' : char.ToLower(c)));
            exercisesFound_stripped.Name.Add(transformed);
        }

        //logger.LogInformation("Finish scrapping exercises:\n{ListString}", string.Join("\n", exercisesFound_stripped.Name));

        driver.Quit();

        return (exercisesFound, exercisesFound_stripped);
    }
}

    public IResult ScrapExercises(string exerciseName)
    {
        var options = new ChromeOptions();
        options.AddArgument("headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

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
    }
}
