namespace Bridgette.Google.Dtos;

public class GoogleApiSettings
{
    /// <summary>
    /// The JSON content of the Google Cloud Service Account key.
    /// </summary>
    public required string ServiceAccountKeyJson { get; set; }
}