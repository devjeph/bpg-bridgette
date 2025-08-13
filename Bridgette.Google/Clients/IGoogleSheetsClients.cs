namespace Bridgette.Google.Clients;

public interface IGoogleSheetsClients
{
    /// <summary>
    /// Checks if a specific sheet (tab) within a Google Spreadsheet contains any data.
    /// </summary>
    /// <param name="spreadsheetId">The ID of the Google Spreadsheet.</param>
    /// <param name="sheetName">The name of the sheet (tab) to check (e.g., "202508").</param>
    /// <returns>True if the sheet has data, otherwise false.</returns>
    Task<bool> IsSheetPopulatedAsync(string spreadsheetId, string sheetName);
}