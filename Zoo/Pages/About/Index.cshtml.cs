using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Zoo.Pages.About;

/// <summary>
/// 關於我們頁面模型
/// </summary>
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    /// <summary>
    /// 初始化關於我們頁面模型
    /// </summary>
    /// <param name="logger">日誌記錄器</param>
    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 處理 GET 請求
    /// </summary>
    public void OnGet()
    {
        _logger.LogInformation("使用者訪問關於我們頁面");
    }
}
