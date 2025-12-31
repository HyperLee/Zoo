using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Services;

namespace Zoo.Pages.Favorites;

/// <summary>
/// 我的收藏頁面模型
/// 顯示使用者收藏的動物清單（從 localStorage 讀取，由前端處理）
/// </summary>
public class IndexModel : PageModel
{
    private readonly IAnimalService _animalService;
    private readonly ILogger<IndexModel> _logger;

    /// <summary>
    /// 初始化收藏頁面模型
    /// </summary>
    /// <param name="animalService">動物服務</param>
    /// <param name="logger">日誌記錄器</param>
    public IndexModel(IAnimalService animalService, ILogger<IndexModel> logger)
    {
        _animalService = animalService;
        _logger = logger;
    }

    /// <summary>
    /// 處理 GET 請求
    /// </summary>
    public void OnGet()
    {
        _logger.LogInformation("使用者瀏覽收藏頁面");
    }
}
