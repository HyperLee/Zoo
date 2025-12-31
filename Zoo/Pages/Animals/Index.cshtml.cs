using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Models;
using Zoo.Services;

namespace Zoo.Pages.Animals;

/// <summary>
/// 動物清單頁面模型
/// </summary>
public class IndexModel : PageModel
{
    private readonly IAnimalService _animalService;
    private readonly ILogger<IndexModel> _logger;

    /// <summary>
    /// 動物清單
    /// </summary>
    public IReadOnlyList<Animal> Animals { get; private set; } = [];

    /// <summary>
    /// 初始化動物清單頁面模型
    /// </summary>
    /// <param name="animalService">動物服務</param>
    /// <param name="logger">日誌記錄器</param>
    public IndexModel(IAnimalService animalService, ILogger<IndexModel> logger)
    {
        _animalService = animalService;
        _logger = logger;
    }

    /// <summary>
    /// 處理 GET 請求，載入動物清單
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("載入動物清單頁面");

        Animals = await _animalService.GetAllAsync(cancellationToken);

        _logger.LogInformation("成功載入 {Count} 隻動物", Animals.Count);
    }
}
