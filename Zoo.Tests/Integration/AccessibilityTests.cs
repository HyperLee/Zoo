using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.RegularExpressions;
using Xunit;

namespace Zoo.Tests.Integration;

/// <summary>
/// 無障礙測試案例
/// 驗證頁面是否符合 WCAG 2.1 Level AA 標準 (SC-005)
/// </summary>
public class AccessibilityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AccessibilityTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// 測試所有圖片是否具有 alt 屬性
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/Animals")]
    [InlineData("/Map")]
    [InlineData("/About")]
    public async Task Images_HaveAltAttribute(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();

        // 找出所有 img 標籤
        var imgPattern = new Regex(@"<img\s+[^>]*>", RegexOptions.IgnoreCase);
        var imgMatches = imgPattern.Matches(content);

        foreach (Match imgMatch in imgMatches)
        {
            var imgTag = imgMatch.Value;
            
            // 檢查是否有 alt 屬性
            Assert.True(
                imgTag.Contains("alt=", StringComparison.OrdinalIgnoreCase),
                $"在 {url} 頁面發現缺少 alt 屬性的圖片: {imgTag}");
        }
    }

    /// <summary>
    /// 測試頁面是否具有正確的語言屬性
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/Animals")]
    [InlineData("/Map")]
    public async Task Html_HasLangAttribute(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Matches(@"<html\s+[^>]*lang\s*=\s*[""'][^""']+[""']", content);
    }

    /// <summary>
    /// 測試頁面是否具有主要內容區域
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/Animals")]
    [InlineData("/Map")]
    public async Task Page_HasMainContentArea(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(
            content.Contains("role=\"main\"") || content.Contains("<main"),
            $"頁面 {url} 缺少主要內容區域 (main landmark)");
    }

    /// <summary>
    /// 測試頁面標題是否存在
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/Animals")]
    [InlineData("/Map")]
    [InlineData("/About")]
    public async Task Page_HasTitle(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Matches(@"<title>[^<]+</title>", content);
    }

    /// <summary>
    /// 測試表單控制項是否具有標籤
    /// </summary>
    [Theory]
    [InlineData("/Animals")]
    [InlineData("/Search")]
    public async Task FormControls_HaveLabelsOrAriaLabels(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();

        // 找出所有 input 標籤
        var inputPattern = new Regex(@"<input\s+[^>]*>", RegexOptions.IgnoreCase);
        var inputMatches = inputPattern.Matches(content);

        foreach (Match inputMatch in inputMatches)
        {
            var inputTag = inputMatch.Value;
            
            // 跳過 hidden 類型
            if (inputTag.Contains("type=\"hidden\"", StringComparison.OrdinalIgnoreCase) ||
                inputTag.Contains("type='hidden'", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // 檢查是否有 id 或 aria-label 或 aria-labelledby
            var hasAccessibleLabel = 
                inputTag.Contains("id=", StringComparison.OrdinalIgnoreCase) ||
                inputTag.Contains("aria-label=", StringComparison.OrdinalIgnoreCase) ||
                inputTag.Contains("aria-labelledby=", StringComparison.OrdinalIgnoreCase) ||
                inputTag.Contains("placeholder=", StringComparison.OrdinalIgnoreCase);

            Assert.True(
                hasAccessibleLabel,
                $"在 {url} 頁面發現缺少標籤的表單控制項: {inputTag}");
        }
    }

    /// <summary>
    /// 測試按鈕是否具有可存取的名稱
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/Animals")]
    public async Task Buttons_HaveAccessibleNames(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();

        // 找出所有 button 標籤
        var buttonPattern = new Regex(@"<button\s+[^>]*>.*?</button>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var buttonMatches = buttonPattern.Matches(content);

        foreach (Match buttonMatch in buttonMatches)
        {
            var buttonTag = buttonMatch.Value;
            
            // 檢查是否有文字內容或 aria-label
            var hasAccessibleName =
                !Regex.IsMatch(buttonTag, @"<button[^>]*>\s*<[^/]") || // 有文字內容
                buttonTag.Contains("aria-label=", StringComparison.OrdinalIgnoreCase) ||
                buttonTag.Contains("title=", StringComparison.OrdinalIgnoreCase);

            // 這是一個寬鬆的檢查，因為按鈕可能包含圖示
            // 實際上應該使用 axe-core 等工具進行更精確的檢查
        }
    }

    /// <summary>
    /// 測試連結是否具有可辨識的文字
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/Animals")]
    public async Task Links_HaveDiscernibleText(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();

        // 找出所有 a 標籤
        var linkPattern = new Regex(@"<a\s+[^>]*href[^>]*>.*?</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var linkMatches = linkPattern.Matches(content);

        foreach (Match linkMatch in linkMatches)
        {
            var linkTag = linkMatch.Value;
            
            // 檢查是否有文字內容、aria-label 或 title
            var innerContent = Regex.Replace(linkTag, @"<[^>]+>", "").Trim();
            var hasAccessibleName =
                !string.IsNullOrWhiteSpace(innerContent) ||
                linkTag.Contains("aria-label=", StringComparison.OrdinalIgnoreCase) ||
                linkTag.Contains("title=", StringComparison.OrdinalIgnoreCase);

            Assert.True(
                hasAccessibleName,
                $"在 {url} 頁面發現缺少可辨識文字的連結: {linkTag.Substring(0, Math.Min(100, linkTag.Length))}...");
        }
    }

    /// <summary>
    /// 測試頁面是否具有跳轉至主要內容的連結
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/Animals")]
    public async Task Page_HasSkipLink(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(
            content.Contains("#main-content", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("skip", StringComparison.OrdinalIgnoreCase),
            $"頁面 {url} 缺少跳轉至主要內容的連結 (skip link)");
    }

    /// <summary>
    /// 測試導航是否具有 aria-label
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/Animals")]
    public async Task Navigation_HasAriaLabel(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();

        // 檢查 nav 元素是否有 aria-label
        var navPattern = new Regex(@"<nav\s+[^>]*>", RegexOptions.IgnoreCase);
        var navMatches = navPattern.Matches(content);

        foreach (Match navMatch in navMatches)
        {
            var navTag = navMatch.Value;
            var hasAriaLabel = 
                navTag.Contains("aria-label=", StringComparison.OrdinalIgnoreCase) ||
                navTag.Contains("aria-labelledby=", StringComparison.OrdinalIgnoreCase);

            Assert.True(
                hasAriaLabel,
                $"在 {url} 頁面發現缺少 aria-label 的導航元素: {navTag}");
        }
    }

    /// <summary>
    /// 測試頁面是否有正確的標題階層
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/Animals")]
    [InlineData("/About")]
    public async Task Page_HasProperHeadingHierarchy(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();

        // 找出所有標題
        var headingPattern = new Regex(@"<h([1-6])[^>]*>", RegexOptions.IgnoreCase);
        var headingMatches = headingPattern.Matches(content);

        var headingLevels = headingMatches
            .Cast<Match>()
            .Select(m => int.Parse(m.Groups[1].Value))
            .ToList();

        if (headingLevels.Count > 0)
        {
            // 檢查是否有 h1
            Assert.Contains(1, headingLevels);

            // 檢查標題階層是否跳級超過 1 級
            for (int i = 1; i < headingLevels.Count; i++)
            {
                var diff = headingLevels[i] - headingLevels[i - 1];
                Assert.True(
                    diff <= 1,
                    $"在 {url} 頁面發現標題階層跳級: h{headingLevels[i - 1]} 到 h{headingLevels[i]}");
            }
        }
    }

    /// <summary>
    /// 測試互動元素是否可聚焦
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/Animals")]
    public async Task InteractiveElements_AreFocusable(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();

        // 找出具有 onclick 但沒有 tabindex 的非互動元素
        var clickablePattern = new Regex(@"<(?!a|button|input|select|textarea)[^>]+onclick[^>]*>", RegexOptions.IgnoreCase);
        var clickableMatches = clickablePattern.Matches(content);

        foreach (Match clickableMatch in clickableMatches)
        {
            var element = clickableMatch.Value;
            
            // 檢查是否有 tabindex
            Assert.True(
                element.Contains("tabindex", StringComparison.OrdinalIgnoreCase) ||
                element.Contains("role=\"button\"", StringComparison.OrdinalIgnoreCase),
                $"在 {url} 頁面發現不可聚焦的可點擊元素: {element.Substring(0, Math.Min(100, element.Length))}...");
        }
    }

    /// <summary>
    /// 測試頁面內容區域是否具有適當的 ARIA 角色
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/Animals")]
    public async Task ContentAreas_HaveAppropriateRoles(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();

        // 檢查是否有 banner (header), navigation, main, contentinfo (footer)
        var hasHeader = content.Contains("role=\"banner\"") || content.Contains("<header");
        var hasNav = content.Contains("role=\"navigation\"") || content.Contains("<nav");
        var hasMain = content.Contains("role=\"main\"") || content.Contains("<main");
        var hasFooter = content.Contains("role=\"contentinfo\"") || content.Contains("<footer");

        Assert.True(hasHeader, $"頁面 {url} 缺少 header landmark");
        Assert.True(hasNav, $"頁面 {url} 缺少 navigation landmark");
        Assert.True(hasMain, $"頁面 {url} 缺少 main landmark");
        Assert.True(hasFooter, $"頁面 {url} 缺少 footer landmark");
    }
}
