using Zoo.Models;

namespace Zoo.Services;

/// <summary>
/// 搜尋服務實作，提供動物搜尋與篩選功能
/// </summary>
public class SearchService : ISearchService
{
    private readonly IAnimalService _animalService;
    private readonly ILogger<SearchService> _logger;

    /// <summary>
    /// 初始化搜尋服務
    /// </summary>
    /// <param name="animalService">動物服務</param>
    /// <param name="logger">日誌記錄器</param>
    public SearchService(IAnimalService animalService, ILogger<SearchService> logger)
    {
        _animalService = animalService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchResult>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("執行搜尋，篩選條件: {@Filter}", filter);

        var animals = await _animalService.GetAllAsync(cancellationToken);
        var results = new List<SearchResult>();

        foreach (var animal in animals)
        {
            // 檢查篩選條件
            if (!MatchesFilter(animal, filter))
            {
                continue;
            }

            // 計算關鍵字匹配
            var (score, matchedFields) = CalculateKeywordScore(animal, filter.Keyword);

            // 如果有關鍵字但沒有匹配，跳過此動物
            if (!string.IsNullOrWhiteSpace(filter.Keyword) && score == 0)
            {
                continue;
            }

            // 沒有關鍵字時，給予基礎分數
            if (string.IsNullOrWhiteSpace(filter.Keyword))
            {
                score = 50;
            }

            results.Add(new SearchResult
            {
                Animal = animal,
                Score = score,
                MatchedFields = matchedFields
            });
        }

        // 按分數排序
        var sortedResults = results
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.Animal.ChineseName)
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("搜尋完成，找到 {Count} 隻動物", sortedResults.Count);

        return sortedResults;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchSuggestion>> SuggestAsync(string keyword, int limit = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            _logger.LogDebug("搜尋建議關鍵字為空，回傳空清單");
            return [];
        }

        _logger.LogDebug("取得搜尋建議，關鍵字: {Keyword}, 上限: {Limit}", keyword, limit);

        var animals = await _animalService.GetAllAsync(cancellationToken);
        var suggestions = new List<(SearchSuggestion Suggestion, double Score)>();

        foreach (var animal in animals)
        {
            var (score, _) = CalculateKeywordScore(animal, keyword);

            if (score > 0)
            {
                suggestions.Add((new SearchSuggestion
                {
                    Id = animal.Id,
                    Name = animal.ChineseName,
                    EnglishName = animal.EnglishName,
                    ThumbnailUrl = animal.Media.ThumbnailPath
                }, score));
            }
        }

        var result = suggestions
            .OrderByDescending(s => s.Score)
            .Take(limit)
            .Select(s => s.Suggestion)
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("搜尋建議完成，找到 {Count} 個建議", result.Count);

        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Animal>> FilterAsync(SearchFilter filter, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("執行篩選，條件: {@Filter}", filter);

        var animals = await _animalService.GetAllAsync(cancellationToken);

        var filteredAnimals = animals
            .Where(a => MatchesFilter(a, filter))
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("篩選完成，找到 {Count} 隻動物", filteredAnimals.Count);

        return filteredAnimals;
    }

    /// <summary>
    /// 檢查動物是否符合篩選條件（不含關鍵字）
    /// </summary>
    /// <param name="animal">待檢查的動物</param>
    /// <param name="filter">篩選條件</param>
    /// <returns>是否符合條件</returns>
    private static bool MatchesFilter(Animal animal, SearchFilter filter)
    {
        // 生物分類篩選
        if (filter.BiologicalClass.HasValue &&
            animal.Classification.BiologicalClass != filter.BiologicalClass.Value)
        {
            return false;
        }

        // 棲息地篩選
        if (filter.Habitat.HasValue &&
            animal.Classification.Habitat != filter.Habitat.Value)
        {
            return false;
        }

        // 飲食習性篩選
        if (filter.Diet.HasValue &&
            animal.Classification.Diet != filter.Diet.Value)
        {
            return false;
        }

        // 活動時間篩選
        if (filter.ActivityPattern.HasValue &&
            animal.Classification.ActivityPattern != filter.ActivityPattern.Value)
        {
            return false;
        }

        // 區域篩選
        if (!string.IsNullOrWhiteSpace(filter.ZoneId) &&
            !animal.ZoneId.Equals(filter.ZoneId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 計算關鍵字匹配分數
    /// </summary>
    /// <param name="animal">動物資料</param>
    /// <param name="keyword">搜尋關鍵字</param>
    /// <returns>匹配分數 (0-100) 和匹配的欄位清單</returns>
    private static (double Score, List<string> MatchedFields) CalculateKeywordScore(Animal animal, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return (0, []);
        }

        var normalizedKeyword = keyword.Trim().ToLowerInvariant();
        double score = 0;
        var matchedFields = new List<string>();

        // 中文名稱完全匹配 (最高分)
        if (animal.ChineseName.Equals(keyword, StringComparison.OrdinalIgnoreCase))
        {
            score += 100;
            matchedFields.Add(nameof(animal.ChineseName));
        }
        // 中文名稱包含關鍵字
        else if (animal.ChineseName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        {
            score += 80;
            matchedFields.Add(nameof(animal.ChineseName));
        }

        // 英文名稱完全匹配
        if (animal.EnglishName.Equals(keyword, StringComparison.OrdinalIgnoreCase))
        {
            score += 90;
            matchedFields.Add(nameof(animal.EnglishName));
        }
        // 英文名稱包含關鍵字
        else if (animal.EnglishName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        {
            score += 70;
            matchedFields.Add(nameof(animal.EnglishName));
        }

        // 學名匹配
        if (animal.ScientificName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        {
            score += 60;
            matchedFields.Add(nameof(animal.ScientificName));
        }

        // 完整描述匹配
        if (animal.Description.FullDescriptionZh.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            animal.Description.FullDescriptionEn.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        {
            score += 30;
            matchedFields.Add("Description");
        }

        // 趣味事實匹配
        if (animal.FunFacts.Any(f => f.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            score += 20;
            matchedFields.Add(nameof(animal.FunFacts));
        }

        // 外觀特色匹配
        if (animal.Description.Appearance.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        {
            score += 25;
            matchedFields.Add("Appearance");
        }

        // 生活習性匹配
        if (animal.Description.Behavior.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        {
            score += 25;
            matchedFields.Add("Behavior");
        }

        // 標準化分數為 0-100
        score = Math.Min(score, 100);

        return (score, matchedFields);
    }
}
