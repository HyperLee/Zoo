using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;
using Xunit;

namespace Zoo.Tests.Integration;

/// <summary>
/// 效能測試案例
/// 驗證頁面載入時間是否符合 SC-003 和 SC-004 的要求（< 3 秒）
/// </summary>
public class PerformanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    /// <summary>
    /// 效能目標：3 秒（3000 毫秒）
    /// </summary>
    private const int PerformanceThresholdMs = 3000;

    public PerformanceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// 測試首頁載入效能
    /// </summary>
    [Fact]
    public async Task HomePage_LoadsWithinThreshold()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(
            stopwatch.ElapsedMilliseconds < PerformanceThresholdMs,
            $"首頁載入時間 ({stopwatch.ElapsedMilliseconds}ms) 超過效能目標 ({PerformanceThresholdMs}ms)");
    }

    /// <summary>
    /// 測試動物清單頁面載入效能
    /// </summary>
    [Fact]
    public async Task AnimalsIndex_LoadsWithinThreshold()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/Animals");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(
            stopwatch.ElapsedMilliseconds < PerformanceThresholdMs,
            $"動物清單頁面載入時間 ({stopwatch.ElapsedMilliseconds}ms) 超過效能目標 ({PerformanceThresholdMs}ms)");
    }

    /// <summary>
    /// 測試動物詳情頁面載入效能
    /// </summary>
    [Theory]
    [InlineData("lion-001")]
    [InlineData("elephant-001")]
    [InlineData("penguin-001")]
    public async Task AnimalDetails_LoadsWithinThreshold(string animalId)
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/Animals/Details/{animalId}");
        stopwatch.Stop();

        // Assert
        // 允許 404（動物可能不存在），但仍需在時間限制內
        Assert.True(
            stopwatch.ElapsedMilliseconds < PerformanceThresholdMs,
            $"動物詳情頁面載入時間 ({stopwatch.ElapsedMilliseconds}ms) 超過效能目標 ({PerformanceThresholdMs}ms)");
    }

    /// <summary>
    /// 測試地圖頁面載入效能
    /// </summary>
    [Fact]
    public async Task MapPage_LoadsWithinThreshold()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/Map");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(
            stopwatch.ElapsedMilliseconds < PerformanceThresholdMs,
            $"地圖頁面載入時間 ({stopwatch.ElapsedMilliseconds}ms) 超過效能目標 ({PerformanceThresholdMs}ms)");
    }

    /// <summary>
    /// 測試搜尋頁面載入效能
    /// </summary>
    [Theory]
    [InlineData("獅子")]
    [InlineData("熊貓")]
    [InlineData("大象")]
    public async Task SearchPage_LoadsWithinThreshold(string query)
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/Search?q={Uri.EscapeDataString(query)}");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(
            stopwatch.ElapsedMilliseconds < PerformanceThresholdMs,
            $"搜尋頁面載入時間 ({stopwatch.ElapsedMilliseconds}ms) 超過效能目標 ({PerformanceThresholdMs}ms)");
    }

    /// <summary>
    /// 測試路線規劃頁面載入效能
    /// </summary>
    [Fact]
    public async Task RoutesPage_LoadsWithinThreshold()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/Routes");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(
            stopwatch.ElapsedMilliseconds < PerformanceThresholdMs,
            $"路線頁面載入時間 ({stopwatch.ElapsedMilliseconds}ms) 超過效能目標 ({PerformanceThresholdMs}ms)");
    }

    /// <summary>
    /// 測試收藏頁面載入效能
    /// </summary>
    [Fact]
    public async Task FavoritesPage_LoadsWithinThreshold()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/Favorites");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(
            stopwatch.ElapsedMilliseconds < PerformanceThresholdMs,
            $"收藏頁面載入時間 ({stopwatch.ElapsedMilliseconds}ms) 超過效能目標 ({PerformanceThresholdMs}ms)");
    }

    /// <summary>
    /// 測試測驗頁面載入效能
    /// </summary>
    [Fact]
    public async Task QuizPage_LoadsWithinThreshold()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/Quiz");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(
            stopwatch.ElapsedMilliseconds < PerformanceThresholdMs,
            $"測驗頁面載入時間 ({stopwatch.ElapsedMilliseconds}ms) 超過效能目標 ({PerformanceThresholdMs}ms)");
    }

    /// <summary>
    /// 測試關於我們頁面載入效能
    /// </summary>
    [Fact]
    public async Task AboutPage_LoadsWithinThreshold()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/About");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(
            stopwatch.ElapsedMilliseconds < PerformanceThresholdMs,
            $"關於我們頁面載入時間 ({stopwatch.ElapsedMilliseconds}ms) 超過效能目標 ({PerformanceThresholdMs}ms)");
    }

    /// <summary>
    /// 測試 API 端點效能
    /// </summary>
    [Theory]
    [InlineData("/api/animals")]
    [InlineData("/api/zones")]
    [InlineData("/api/routes")]
    public async Task ApiEndpoints_RespondWithinThreshold(string endpoint)
    {
        // Arrange
        var apiThreshold = 1000; // API 應該更快，設定為 1 秒
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync(endpoint);
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(
            stopwatch.ElapsedMilliseconds < apiThreshold,
            $"API {endpoint} 回應時間 ({stopwatch.ElapsedMilliseconds}ms) 超過效能目標 ({apiThreshold}ms)");
    }

    /// <summary>
    /// 批次測試多個頁面的平均效能
    /// 驗證 95% 的頁面載入時間符合要求
    /// </summary>
    [Fact]
    public async Task AllPages_95PercentWithinThreshold()
    {
        // Arrange
        var pages = new[]
        {
            "/",
            "/Animals",
            "/Map",
            "/Routes",
            "/Favorites",
            "/Quiz",
            "/About",
            "/Search?q=test"
        };

        var loadTimes = new List<long>();

        // Act
        foreach (var page in pages)
        {
            // 執行 3 次取平均
            for (int i = 0; i < 3; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await _client.GetAsync(page);
                stopwatch.Stop();
                
                if (response.IsSuccessStatusCode)
                {
                    loadTimes.Add(stopwatch.ElapsedMilliseconds);
                }
            }
        }

        // Assert
        loadTimes.Sort();
        var percentile95Index = (int)(loadTimes.Count * 0.95);
        var percentile95Value = loadTimes[Math.Min(percentile95Index, loadTimes.Count - 1)];

        Assert.True(
            percentile95Value < PerformanceThresholdMs,
            $"95% 頁面載入時間 ({percentile95Value}ms) 超過效能目標 ({PerformanceThresholdMs}ms)");
    }
}
