using Google.Apis.Sheets.v4;

namespace Bridgette.Google.Clients;

public class GoogleSheetsClient : IGoogleSheetsClients
{
    private readonly SheetsService _sheetsService;

    public GoogleSheetsClient(SheetsService sheetsService)
    {
        _sheetsService = sheetsService;
    }

    public async Task<bool> IsSheetPopulatedAsync(string spreadsheetId, string sheetName)
    {
        try
        {
            // Check a small but significant range
            var range = $"{sheetName}!A1:C5";
            var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = await request.ExecuteAsync();

            return response.Values != null && response.Values.Count > 0;
        }
        catch (Google.GoogleApiException ex) when (ex.Error.Message.Contains("Unable to parse range"))
        {
            return false;
        }
    }
}