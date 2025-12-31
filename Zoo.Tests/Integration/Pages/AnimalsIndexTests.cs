using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Zoo.Tests.Integration.Pages;

/// <summary>
/// 動物清單頁面整合測試
/// </summary>
public class AnimalsIndexTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AnimalsIndexTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 測試環境可能需要額外設定
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task AnimalsIndex_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/Animals");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AnimalsIndex_ReturnsHtmlContent()
    {
        var response = await _client.GetAsync("/Animals");

        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task AnimalsIndex_ContainsPageTitle()
    {
        var response = await _client.GetAsync("/Animals");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("動物介紹", content);
    }

    [Fact]
    public async Task AnimalsIndex_ContainsAnimalCards()
    {
        var response = await _client.GetAsync("/Animals");
        var content = await response.Content.ReadAsStringAsync();

        // 驗證頁面包含動物卡片的 CSS 類別
        Assert.Contains("animal-card", content);
    }

    [Fact]
    public async Task AnimalsIndex_ContainsBreadcrumb()
    {
        var response = await _client.GetAsync("/Animals");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("breadcrumb", content);
        Assert.Contains("首頁", content);
    }
}
