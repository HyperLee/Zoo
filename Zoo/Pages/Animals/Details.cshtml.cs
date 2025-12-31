using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Models;
using Zoo.Services;

namespace Zoo.Pages.Animals;

/// <summary>
/// 動物詳情頁面模型
/// </summary>
public class DetailsModel : PageModel
{
    private readonly IAnimalService _animalService;
    private readonly ILogger<DetailsModel> _logger;

    /// <summary>
    /// 動物資料
    /// </summary>
    public Animal? Animal { get; private set; }

    /// <summary>
    /// 相關動物清單
    /// </summary>
    public IReadOnlyList<Animal> RelatedAnimals { get; private set; } = [];

    /// <summary>
    /// 初始化動物詳情頁面模型
    /// </summary>
    /// <param name="animalService">動物服務</param>
    /// <param name="logger">日誌記錄器</param>
    public DetailsModel(IAnimalService animalService, ILogger<DetailsModel> logger)
    {
        _animalService = animalService;
        _logger = logger;
    }

    /// <summary>
    /// 處理 GET 請求，載入動物詳情
    /// </summary>
    /// <param name="id">動物 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>頁面結果</returns>
    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("動物 ID 為空，重導向至動物清單頁面");
            return RedirectToPage("/Animals/Index");
        }

        _logger.LogInformation("載入動物詳情頁面: {Id}", id);

        Animal = await _animalService.GetByIdAsync(id, cancellationToken);

        if (Animal is null)
        {
            _logger.LogWarning("找不到 ID 為 {Id} 的動物，重導向至動物清單頁面", id);
            return NotFound();
        }

        RelatedAnimals = await _animalService.GetRelatedAsync(id, cancellationToken);

        _logger.LogInformation("成功載入動物 {ChineseName} 的詳情，相關動物數量: {Count}", 
            Animal.ChineseName, RelatedAnimals.Count);

        return Page();
    }
}
