using System.Text.Json;


namespace PraticeOneSampleTests;

// PSEUDOCODE / PLAN (detailed):
// 1. Create an xUnit test class ProgramTest that uses WebApplicationFactory<Program> to host the minimal API in-memory.
// 2. Create a HttpClient from the factory.
// 3. Send GET request to "/weatherforecast".
// 4. Parse the JSON response with System.Text.Json (avoid DateOnly deserialization issues).
// 5. Assert the response status is success and that the JSON root is an array of length 5.
// 6. For each forecast object:
//    - Ensure properties "date", "temperatureC", and "summary" exist.
//    - Ensure temperatureC is within the expected Random.Shared.Next bounds (-20 .. 54).
//    - Ensure summary is not null or empty.
//    - If "temperatureF" is present, compute expected Fahrenheit using the same formula and assert equality.
// 7. Keep tests deterministic in shape (count), but allow range checks for randomized values.
// 8. Use minimal dependencies: xUnit, Microsoft.AspNetCore.Mvc.Testing, System.Text.Json.
// 9. Place this file at PracticeOneSample/ProgramTest.cs so it runs inside the same project/assembly as Program.

// FILE: PracticeOneSample/ProgramTest.cs

public class ProgramTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProgramTest(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsFiveForecasts_WithExpectedShape()
    {
        // Arrange
        // Act
        var response = await _client.GetAsync("/weatherforecast");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode, "Expected successful status code from /weatherforecast");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        Assert.Equal(5, root.GetArrayLength());

        foreach (var item in root.EnumerateArray())
        {
            // Date property should exist (string)
            Assert.True(item.TryGetProperty("date", out var dateProp), "Missing 'date' property");
            Assert.Equal(JsonValueKind.String, dateProp.ValueKind);

            // temperatureC should exist and be an integer
            Assert.True(item.TryGetProperty("temperatureC", out var tempCProp), "Missing 'temperatureC' property");
            Assert.Equal(JsonValueKind.Number, tempCProp.ValueKind);
            int tempC = tempCProp.GetInt32();
            // Random.Shared.Next(-20, 55) yields values in [-20,54]
            Assert.InRange(tempC, -20, 54);

            // summary should exist and be non-empty
            Assert.True(item.TryGetProperty("summary", out var summaryProp), "Missing 'summary' property");
            Assert.Equal(JsonValueKind.String, summaryProp.ValueKind);
            string? summary = summaryProp.GetString();
            Assert.False(string.IsNullOrEmpty(summary));

            // If temperatureF is present, verify computed value matches record's calculation
            if (item.TryGetProperty("temperatureF", out var tempFProp) && tempFProp.ValueKind == JsonValueKind.Number)
            {
                int returnedF = tempFProp.GetInt32();
                int expectedF = 32 + (int)(tempC / 0.5556);
                Assert.Equal(expectedF, returnedF);
            }
        }
    }
}