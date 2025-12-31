using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Models;
using Zoo.Services;

namespace Zoo.Pages.Quiz;

/// <summary>
/// 知識測驗頁面模型
/// </summary>
public class IndexModel : PageModel
{
    private readonly IQuizService _quizService;
    private readonly IAnimalService _animalService;
    private readonly ILogger<IndexModel> _logger;

    /// <summary>
    /// 測驗題目總數
    /// </summary>
    public int TotalQuizCount { get; private set; }

    /// <summary>
    /// 有測驗題目的動物清單
    /// </summary>
    public IReadOnlyList<Animal> AnimalsWithQuizzes { get; private set; } = [];

    /// <summary>
    /// 初始化知識測驗頁面模型
    /// </summary>
    /// <param name="quizService">測驗服務</param>
    /// <param name="animalService">動物服務</param>
    /// <param name="logger">日誌記錄器</param>
    public IndexModel(
        IQuizService quizService, 
        IAnimalService animalService,
        ILogger<IndexModel> logger)
    {
        _quizService = quizService;
        _animalService = animalService;
        _logger = logger;
    }

    /// <summary>
    /// 處理 GET 請求，載入測驗頁面資料
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("載入知識測驗頁面");

        var allQuizzes = await _quizService.GetAllAsync(cancellationToken);
        TotalQuizCount = allQuizzes.Count;

        // 取得有測驗題目的動物 ID
        var animalIdsWithQuizzes = allQuizzes
            .Select(q => q.AnimalId)
            .Distinct()
            .ToHashSet();

        // 取得對應的動物資料
        var allAnimals = await _animalService.GetAllAsync(cancellationToken);
        AnimalsWithQuizzes = allAnimals
            .Where(a => animalIdsWithQuizzes.Contains(a.Id))
            .ToList()
            .AsReadOnly();

        _logger.LogInformation(
            "知識測驗頁面載入完成，共 {QuizCount} 道題目，{AnimalCount} 隻動物有測驗",
            TotalQuizCount, AnimalsWithQuizzes.Count);
    }
}
