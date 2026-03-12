using System.Text.RegularExpressions;
using Microsoft.Playwright;
using System.Globalization;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();
var password = Environment.GetEnvironmentVariable("BH_PASSWORD")
    ?? throw new InvalidOperationException("BH_PASSWORD not found in .env file.");

var email = Environment.GetEnvironmentVariable("BH_EMAIL") ?? throw new InvalidOperationException("BH_EMAIL not found in .env file.");

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = false // Set to true once you know it works to run in the background
});

var context = await browser.NewContextAsync();
var page = await context.NewPageAsync();

// Days of the week mapped to their Bullhorn field IDs
var dow = new Dictionary<string, string>
{
    { "monday",    "txtmon0" },
    { "tuesday",   "txtmon1" },
    { "wednesday", "txtmon2" },
    { "thursday",  "txtmon3" },
    { "friday",    "txtmon4" }
};

var orderedDays = new List<string> { "monday", "tuesday", "wednesday", "thursday", "friday" };

Console.WriteLine("Enter the current week (x/x/xxxx):");
var current_week = PromptUntilValid("Invalid week format, try again! (x/x/xxxx)", IsValidCurrentWeek);

Console.WriteLine("Enter the starting day (monday/tuesday/etc.):");
var current_day = PromptUntilValid("Invalid day, try again! Use lowercase (e.g. monday)", IsValidCurrentDay);

// Navigate and log in once before the loop
await page.GotoAsync("https://sl2-www.bte.bullhornstaffing.com/LogonVerify/RuId/login?signin=3adda7e8725d532f148b2681bf305bf3");
await page.GetByPlaceholder("youremail@domain.com").FillAsync(email);
await page.FillAsync("#password", password);
await page.GetByRole(AriaRole.Button, new() { Name = "Log In" }).ClickAsync();

// Click the current week
await page.GetByText(current_week).ClickAsync();

bool keepGoing = true;

while (keepGoing)
{
    Console.WriteLine($"\n--- Entering time for {current_day} ---");

    // Collect time in
    Console.WriteLine("Enter time in (HH:mm):");
    var time_in = PromptUntilValid("Invalid time format, try again! (HH:mm or H:mm)", IsValidTimeFormat);

    // Collect time out
    Console.WriteLine("Enter time out (HH:mm):");
    var time_out = PromptUntilValid("Invalid time format, try again! (HH:mm or H:mm)", IsValidTimeFormat);

    // Lunch
    var lunch_in  = "";
    var lunch_out = "";

    Console.WriteLine("Did you take a lunch? (yes/no):");
    var lunch_check = Console.ReadLine()?.Trim().ToLower();
    if (lunch_check == "yes")
    {
        Console.WriteLine("Enter lunch out (HH:mm):");
        lunch_out = PromptUntilValid("Invalid time format, try again! (HH:mm or H:mm)", IsValidTimeFormat);

        Console.WriteLine("Enter lunch in (HH:mm):");
        lunch_in = PromptUntilValid("Invalid time format, try again! (HH:mm or H:mm)", IsValidTimeFormat);
    }

    // Enter time into Bullhorn
    await page.Locator("#" + dow[current_day]).ClickAsync();
    await Task.Delay(5000);

    await page.Keyboard.TypeAsync(time_in);
    await page.Keyboard.PressAsync("Tab");
    await page.Keyboard.TypeAsync(time_out);

    if (lunch_out != "" && lunch_in != "")
    {
        await page.Keyboard.PressAsync("Tab");
        await page.Keyboard.TypeAsync(lunch_out);
        await page.Keyboard.PressAsync("Tab");
        await page.Keyboard.TypeAsync(lunch_in);
        await page.Keyboard.PressAsync("Tab");
    }

    await Task.Delay(1000);
    await page.Locator("[data-automation-id='save']").ClickAsync();
    await Task.Delay(3000);

    Console.WriteLine($"Time for {current_day} saved successfully!");

    // Ask to continue with the next day
    int currentIndex = orderedDays.IndexOf(current_day);
    bool hasNextDay = currentIndex < orderedDays.Count - 1;

    if (hasNextDay)
    {
        string nextDay = orderedDays[currentIndex + 1];
        Console.WriteLine($"\nDo you want to add time for the next day ({nextDay})? (yes/no):");
        var continueInput = Console.ReadLine()?.Trim().ToLower();

        if (continueInput == "yes")
        {
            current_day = nextDay;
        }
        else
        {
            keepGoing = false;
        }
    }
    else
    {
        Console.WriteLine("\nYou've reached the end of the work week (Friday).");
        keepGoing = false;
    }
}

// Offer to submit timesheet once all days are entered
Console.WriteLine("\nDo you want to submit the timesheet for approval? (yes/no):");
var submit = Console.ReadLine()?.Trim().ToLower();

if (submit == "yes")
{
    await page.Locator("[data-automation-id='submit-all-for-approval']").ClickAsync();
    Console.WriteLine("Timesheet submitted!");
    await Task.Delay(5000);
}
else
{
    Console.WriteLine("Timesheet not submitted. You can submit it manually.");
}

// ── Helpers ────────────────────────────────────────────────────────────────

static string PromptUntilValid(string errorMessage, Func<string, bool> validator)
{
    var input = Console.ReadLine()?.Trim() ?? "";
    while (!validator(input))
    {
        Console.WriteLine(errorMessage);
        input = Console.ReadLine()?.Trim() ?? "";
    }
    return input;
}

static bool IsValidTimeFormat(string time)
{
    return DateTime.TryParseExact(
        time,
        new[] { "HH:mm", "H:mm", "h:mm tt", "h:mmtt" },
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out _);
}

static bool IsValidCurrentWeek(string currentWeek)
{
    return Regex.IsMatch(currentWeek, @"^\d{1,2}\/\d{1,2}\/\d{4}$");
}

static bool IsValidCurrentDay(string currentDay)
{
    var valid = new[] { "monday", "tuesday", "wednesday", "thursday", "friday" };
    return valid.Contains(currentDay.ToLower());
}