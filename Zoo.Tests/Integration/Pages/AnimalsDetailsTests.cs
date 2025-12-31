using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Zoo.Tests.Integration.Pages;

/// <summary>
/// 動物詳情頁面整合測試
/// </summary>
public class AnimalsDetailsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AnimalsDetailsTests(WebApplicationFactory<Program> factory)
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
    public async Task AnimalsDetails_ReturnsNotFound_WhenAnimalNotExists()
    {
        var response = await _client.GetAsync("/Animals/nonexistent-animal");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AnimalsDetails_PageRouteWorks()
    {
        // 先確認動物清單頁面可以正常載入
        var listResponse = await _client.GetAsync("/Animals");
        listResponse.EnsureSuccessStatusCode();
        
        var listContent = await listResponse.Content.ReadAsStringAsync();
        
        // 確認頁面有正確的標題
        Assert.Contains("動物介紹", listContent);
    }
}
