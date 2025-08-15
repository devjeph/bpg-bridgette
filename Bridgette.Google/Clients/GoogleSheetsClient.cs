using Bridgette.Google.Dtos;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Bridgette.Google.Clients;

public class GoogleSheetsClient : IGoogleSheetsClients
{
    private readonly SheetsService _sheetsService;

    public GoogleSheetsClient(IOptions<GoogleApiSettings> googleApiSettings)
    {
        var credential = GoogleCredential.FromJson(googleApiSettings.Value.ServiceAccountKeyJson)
            .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

        _sheetsService = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "BPG Bridgette"
        });
    }

    public async Task<bool> IsSheetPopulatedAsync(string spreadsheetId, string sheetName)
    {
        try
        {
            // 1. Determine the date range to check
            if (!TryGetDateRange(sheetName, out var firstDayOfMonth, out var lastSaturday))
            {
                return false; // Invalid sheet name or date logic.
            }

            // 2. Fetch and parse sheet data.
            var sheetData = await GetAndParseSheetData(spreadsheetId, sheetName);

            // 3. Iterate and validate each required day.
            for (var day = firstDayOfMonth; day.Date <= lastSaturday; day = day.AddDays(1))
            {
                if (day.DayOfWeek == DayOfWeek.Sunday) continue; // Skip Sundays

                if (day.DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday)
                {
                    if (!IsWeekdayEntryValid(day, sheetData))
                    {
                        return false; // Found an invalid or missing weekday entry
                    }
                }
                else if (day.DayOfWeek == DayOfWeek.Saturday)
                {
                    if (!IsWeekendEntryValid(day, sheetData)) return false;
                }
            }

            // 4. If all checks passed, the timesheet is valid.
            return true;
        }
        catch (GoogleApiException ex) when (ex.Error.Message.Contains("Unable to parse range"))
        {
            return false; // Sheet (tab) for the month doesn't exist.
        }
    }

    /// <summary>
    /// Fetches data from the specific sheet and parses it into a date-keyed dictionary.
    /// </summary>
    private async Task<Dictionary<DateTime, IList<object>>> GetAndParseSheetData(string spreadsheetId, string sheetName)
    {
        var sheetData = new Dictionary<DateTime, IList<object>>();
        var range = $"{sheetName}!A9:M39";
        var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();
        
        if (response.Values == null) return sheetData;

        foreach (var row in response.Values)
        {
            if (row.Count < 3 || !int.TryParse(row[0]?.ToString(), out int year) ||
                !int.TryParse(row[1]?.ToString(), out int month) ||
                !int.TryParse(row[2]?.ToString(), out int day))
            {
                continue;
            }

            try
            {
                var entryDate = new DateTime(year, month, day);
                sheetData[entryDate.Date] = row;
            }
            catch
            {
                /* Ignore invalid dates in the sheet */
            }
        }
        return sheetData;
    }

    /// <summary>
    /// Validates a single weekday's data against the business rules.
    /// </summary>
    private bool IsWeekdayEntryValid(DateTime day, IReadOnlyDictionary<DateTime, IList<object>> sheetData)
    {
        if (!sheetData.TryGetValue(day.Date, out var row)) return false; // Row is missing for a required weekday.

        bool timeInIsEmpty = row.Count <= 6 || string.IsNullOrWhiteSpace(row[6].ToString());
        bool timeOutIsEmpty = row.Count <= 7 || string.IsNullOrWhiteSpace(row[7].ToString());
        
        // If time is not logged, it must be a valid absence or holiday.
        if (timeInIsEmpty && timeOutIsEmpty)
        {
            if (row.Count <=4) return false; // Column E is missing.
            var flag = row[4].ToString();
            return flag is "休暇" or "休日"; // Must be absent or holiday.
        }
        
        // If time is logged, it must be complete and meet the 8-hour requirement.
        if (timeInIsEmpty || timeOutIsEmpty) return false; // Partial entries are invalid.
        if (row.Count <= 12 || !double.TryParse(row[12].ToString(), out var hours) || hours < 8) return false;
        
        return true;
    }

    private bool IsWeekendEntryValid(DateTime day, IReadOnlyDictionary<DateTime, IList<object>> sheetData)
    {
        if (!sheetData.TryGetValue(day.Date, out var row)) return true;
        
        bool timeInIsEmpty = row.Count <= 6 || string.IsNullOrWhiteSpace(row[6].ToString());
        bool timeOutIsEmpty = row.Count <= 7 || string.IsNullOrWhiteSpace(row[7].ToString());
        
        // If a row exists but time is empty, it must be a valid absence or holiday.
        if (timeInIsEmpty && timeOutIsEmpty)
        {
            if (row.Count <=4) return false;
            var flag = row[4].ToString();
            return flag == "休暇" || flag == "休日";
        }
        
        // If time is logged, both fields must be present.
        return !timeInIsEmpty && !timeOutIsEmpty;
    }

    /// <summary>
    /// Calculates the start and end dates for the timesheet check.
    /// </summary>
    private bool TryGetDateRange(string sheetName, out DateTime firstDayOfMonth, out DateTime lastSaturday)
    {
        firstDayOfMonth = default;
        lastSaturday = default;

        if (!DateTime.TryParseExact(sheetName, "yyyyMM", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var monthDate))
        {
            return false; // Invalid sheet name format.
        }

        var today = DateTime.UtcNow;
        int daysToSubtract = (today.DayOfWeek - DayOfWeek.Saturday + 7) % 7;
        
        firstDayOfMonth = new DateTime(monthDate.Year, monthDate.Month, 1);
        lastSaturday = today.AddDays(-daysToSubtract).Date;
        
        return true;
    } 
    
}