using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Models;
using Zoo.Services;

namespace Zoo.Pages.Animals;

/// <summary>
/// 動物清單頁面模型
/// </summary>
public class IndexModel : PageModel
{
    private readonly ISearchService _searchService;
    private readonly ILogger<IndexModel> _logger;

    /// <summary>
    /// 動物清單
    /// </summary>
    public IReadOnlyList<Animal> Animals { get; private set; } = [];

    /// <summary>
    /// 生物分類篩選
    /// </summary>
    [BindProperty(SupportsGet = true, Name = "class")]
    public string? BiologicalClass { get; set; }

    /// <summary>
    /// 棲息地篩選
    /// </summary>
    [BindProperty(SupportsGet = true, Name = "habitat")]
    public string? HabitatFilter { get; set; }

    /// <summary>
    /// 飲食習性篩選
    /// </summary>
    [BindProperty(SupportsGet = true, Name = "diet")]
    public string? DietFilter { get; set; }

    /// <summary>
    /// 活動時間篩選
    /// </summary>
    [BindProperty(SupportsGet = true, Name = "activity")]
    public string? ActivityFilter { get; set; }

    /// <summary>
    /// 是否有啟用的篩選條件
    /// </summary>
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(BiologicalClass) ||
        !string.IsNullOrWhiteSpace(HabitatFilter) ||
        !string.IsNullOrWhiteSpace(DietFilter) ||
        !string.IsNullOrWhiteSpace(ActivityFilter);

    /// <summary>
    /// 初始化動物清單頁面模型
    /// </summary>
    /// <param name="searchService">搜尋服務</param>
    /// <param name="logger">日誌記錄器</param>
    public IndexModel(ISearchService searchService, ILogger<IndexModel> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// 處理 GET 請求，載入動物清單
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("載入動物清單頁面，篩選條件: class={Class}, habitat={Habitat}, diet={Diet}, activity={Activity}",
            BiologicalClass, HabitatFilter, DietFilter, ActivityFilter);

        var filter = new SearchFilter
        {
            BiologicalClass = ParseEnum<BiologicalClass>(BiologicalClass),
            Habitat = ParseEnum<Habitat>(HabitatFilter),
            Diet = ParseEnum<Diet>(DietFilter),
            ActivityPattern = ParseEnum<ActivityPattern>(ActivityFilter)
        };

        Animals = await _searchService.FilterAsync(filter, cancellationToken);

        _logger.LogInformation("成功載入 {Count} 隻動物", Animals.Count);
    }

    /// <summary>
    /// 取得生物分類的中文名稱
    /// </summary>
    /// <returns>中文名稱</returns>
    public string GetBiologicalClassName() => BiologicalClass switch
    {
        "Mammal" => "哺乳類",
        "Bird" => "鳥類",
        "Reptile" => "爬蟲類",
        "Amphibian" => "兩棲類",
        "Fish" => "魚類",
        "Invertebrate" => "無脊椎動物",
        _ => BiologicalClass ?? ""
    };

    /// <summary>
    /// 取得棲息地的中文名稱
    /// </summary>
    /// <returns>中文名稱</returns>
    public string GetHabitatName() => HabitatFilter switch
    {
        "TropicalRainforest" => "熱帶雨林",
        "Desert" => "沙漠",
        "Grassland" => "草原",
        "Polar" => "極地",
        "Ocean" => "海洋",
        "Freshwater" => "淡水",
        "Mountain" => "山區",
        _ => HabitatFilter ?? ""
    };

    /// <summary>
    /// 取得飲食習性的中文名稱
    /// </summary>
    /// <returns>中文名稱</returns>
    public string GetDietName() => DietFilter switch
    {
        "Carnivore" => "肉食性",
        "Herbivore" => "草食性",
        "Omnivore" => "雜食性",
        _ => DietFilter ?? ""
    };

    /// <summary>
    /// 取得活動時間的中文名稱
    /// </summary>
    /// <returns>中文名稱</returns>
    public string GetActivityName() => ActivityFilter switch
    {
        "Diurnal" => "日行性",
        "Nocturnal" => "夜行性",
        "Crepuscular" => "晨昏性",
        _ => ActivityFilter ?? ""
    };

    /// <summary>
    /// 解析列舉值
    /// </summary>
    /// <typeparam name="T">列舉型別</typeparam>
    /// <param name="value">字串值</param>
    /// <returns>解析後的列舉值，若無效則回傳 null</returns>
    private static T? ParseEnum<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        return null;
    }
}
