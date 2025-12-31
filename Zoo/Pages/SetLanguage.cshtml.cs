using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Zoo.Pages;

/// <summary>
/// 語言切換頁面模型
/// </summary>
/// <remarks>
/// 處理使用者的語言切換請求，將選擇的語言儲存在 Cookie 中
/// </remarks>
public class SetLanguageModel : PageModel
{
    private readonly ILogger<SetLanguageModel> _logger;

    /// <summary>
    /// 初始化語言切換頁面模型
    /// </summary>
    /// <param name="logger">日誌記錄器</param>
    public SetLanguageModel(ILogger<SetLanguageModel> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 處理 POST 請求，設定使用者的語言偏好
    /// </summary>
    /// <param name="culture">要切換的語言文化代碼（如 zh-TW、en）</param>
    /// <param name="returnUrl">切換後要返回的 URL</param>
    /// <returns>重新導向至指定的 URL</returns>
    public IActionResult OnPost(string culture, string returnUrl)
    {
        _logger.LogInformation("使用者切換語言至: {Culture}", culture);

        // 驗證語言代碼
        if (string.IsNullOrEmpty(culture))
        {
            culture = "zh-TW";
        }

        // 設定語言偏好 Cookie
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            }
        );

        // 返回原頁面或首頁
        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
        {
            returnUrl = "/";
        }

        return LocalRedirect(returnUrl);
    }
}
