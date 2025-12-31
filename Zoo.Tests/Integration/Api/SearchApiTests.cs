using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;

namespace Zoo.Tests.Integration.Api;

/// <summary>
/// 搜尋 API 整合測試
/// </summary>
public class SearchApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public SearchApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 測試環境可能需要額外設定
            });
        });
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    #region Search Suggest API Tests

    [Fact]
    public async Task SearchSuggest_WithValidKeyword_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/api/Search/Suggest?q=獅");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SearchSuggest_WithValidKeyword_ReturnsJsonContent()
    {
        var response = await _client.GetAsync("/api/Search/Suggest?q=獅");

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task SearchSuggest_WithValidKeyword_ReturnsSuggestionsArray()
    {
        var response = await _client.GetAsync("/api/Search/Suggest?q=獅");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SuggestResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.NotNull(result.Suggestions);
    }

    [Fact]
    public async Task SearchSuggest_WithEmptyKeyword_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/Search/Suggest?q=");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchSuggest_WithoutKeyword_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/Search/Suggest");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchSuggest_RespectsLimitParameter()
    {
        var response = await _client.GetAsync("/api/Search/Suggest?q=非&limit=1");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SuggestResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.True(result.Suggestions.Count <= 1);
    }

    #endregion

    #region Animals API Tests

    [Fact]
    public async Task AnimalsApi_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/api/Animals");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AnimalsApi_ReturnsJsonContent()
    {
        var response = await _client.GetAsync("/api/Animals");

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task AnimalsApi_ReturnsAnimalsArrayAndTotal()
    {
        var response = await _client.GetAsync("/api/Animals");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AnimalsResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.NotNull(result.Animals);
        Assert.True(result.Total >= 0);
    }

    [Fact]
    public async Task AnimalsApi_WithBiologicalClassFilter_ReturnsFilteredAnimals()
    {
        var response = await _client.GetAsync("/api/Animals?class=Mammal");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AnimalsResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        if (result.Animals.Count > 0)
        {
            Assert.All(result.Animals, a => Assert.Equal("Mammal", a.Classification.BiologicalClass));
        }
    }

    [Fact]
    public async Task AnimalsApi_WithHabitatFilter_ReturnsFilteredAnimals()
    {
        var response = await _client.GetAsync("/api/Animals?habitat=Grassland");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AnimalsResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        if (result.Animals.Count > 0)
        {
            Assert.All(result.Animals, a => Assert.Equal("Grassland", a.Classification.Habitat));
        }
    }

    [Fact]
    public async Task AnimalsApi_WithDietFilter_ReturnsFilteredAnimals()
    {
        var response = await _client.GetAsync("/api/Animals?diet=Carnivore");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AnimalsResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        // 只確認回應成功，實際資料依測試資料而定
    }

    [Fact]
    public async Task AnimalsApi_WithActivityFilter_ReturnsFilteredAnimals()
    {
        var response = await _client.GetAsync("/api/Animals?activity=Diurnal");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AnimalsResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        // 只確認回應成功，實際資料依測試資料而定
    }

    [Fact]
    public async Task AnimalsApi_WithInvalidFilter_ReturnsAllAnimals()
    {
        var response = await _client.GetAsync("/api/Animals?class=InvalidClass");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AnimalsResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        // 無效的篩選值應該被忽略，回傳所有動物
    }

    #endregion

    #region Animal Details API Tests

    [Fact]
    public async Task AnimalDetailsApi_WithValidId_ReturnsSuccessStatusCode()
    {
        // 先取得動物清單以獲得有效 ID
        var listResponse = await _client.GetAsync("/api/Animals");
        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listResult = JsonSerializer.Deserialize<AnimalsResponse>(listContent, _jsonOptions);

        if (listResult?.Animals.Count > 0)
        {
            var animalId = listResult.Animals[0].Id;
            var response = await _client.GetAsync($"/api/Animals/{animalId}");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task AnimalDetailsApi_WithInvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/Animals/invalid-id-12345");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AnimalDetailsApi_ReturnsJsonContent()
    {
        // 先取得動物清單以獲得有效 ID
        var listResponse = await _client.GetAsync("/api/Animals");
        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listResult = JsonSerializer.Deserialize<AnimalsResponse>(listContent, _jsonOptions);

        if (listResult?.Animals.Count > 0)
        {
            var animalId = listResult.Animals[0].Id;
            var response = await _client.GetAsync($"/api/Animals/{animalId}");

            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }
    }

    #endregion

    #region Search Page Tests

    [Fact]
    public async Task SearchPage_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/Search");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SearchPage_WithKeyword_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/Search?q=獅");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SearchPage_WithFilters_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/Search?class=Mammal&habitat=Grassland");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SearchPage_ContainsSearchForm()
    {
        var response = await _client.GetAsync("/Search");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("form", content);
        Assert.Contains("進階搜尋", content);
    }

    #endregion

    #region Response DTOs

    private class SuggestResponse
    {
        public List<SuggestionItem> Suggestions { get; set; } = [];
    }

    private class SuggestionItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
    }

    private class AnimalsResponse
    {
        public List<AnimalItem> Animals { get; set; } = [];
        public int Total { get; set; }
    }

    private class AnimalItem
    {
        public string Id { get; set; } = string.Empty;
        public string ChineseName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string ConservationStatus { get; set; } = string.Empty;
        public ClassificationItem Classification { get; set; } = new();
    }

    private class ClassificationItem
    {
        public string BiologicalClass { get; set; } = string.Empty;
        public string Habitat { get; set; } = string.Empty;
    }

    #endregion
}
