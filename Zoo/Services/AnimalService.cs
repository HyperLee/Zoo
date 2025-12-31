using Zoo.Models;

namespace Zoo.Services;

/// <summary>
/// 動物服務實作，提供動物資料的存取功能
/// </summary>
public class AnimalService : IAnimalService
{
    private readonly IJsonDataService _jsonDataService;
    private readonly ILogger<AnimalService> _logger;

    /// <summary>
    /// 初始化動物服務
    /// </summary>
    /// <param name="jsonDataService">JSON 資料服務</param>
    /// <param name="logger">日誌記錄器</param>
    public AnimalService(IJsonDataService jsonDataService, ILogger<AnimalService> logger)
    {
        _jsonDataService = jsonDataService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Animal>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("取得所有動物資料");

        var animals = await _jsonDataService.LoadAsync<Animal>(
            "animals.json",
            "animals",
            cancellationToken);

        _logger.LogInformation("成功取得 {Count} 隻動物", animals.Count);

        return animals;
    }

    /// <inheritdoc />
    public async Task<Animal?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("動物 ID 不可為空");
            return null;
        }

        _logger.LogDebug("根據 ID 取得動物: {Id}", id);

        var animals = await GetAllAsync(cancellationToken);
        var animal = animals.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (animal is null)
        {
            _logger.LogWarning("找不到 ID 為 {Id} 的動物", id);
        }
        else
        {
            _logger.LogDebug("成功取得動物: {ChineseName} ({Id})", animal.ChineseName, animal.Id);
        }

        return animal;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Animal>> GetRelatedAsync(string animalId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(animalId))
        {
            _logger.LogWarning("動物 ID 不可為空");
            return [];
        }

        _logger.LogDebug("取得動物 {Id} 的相關動物", animalId);

        var animal = await GetByIdAsync(animalId, cancellationToken);

        if (animal is null)
        {
            _logger.LogWarning("找不到動物 {Id}，無法取得相關動物", animalId);
            return [];
        }

        if (animal.RelatedAnimalIds.Count == 0)
        {
            _logger.LogDebug("動物 {Id} 沒有相關動物", animalId);
            return [];
        }

        var allAnimals = await GetAllAsync(cancellationToken);
        var relatedAnimals = allAnimals
            .Where(a => animal.RelatedAnimalIds.Contains(a.Id))
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("取得動物 {Id} 的 {Count} 隻相關動物", animalId, relatedAnimals.Count);

        return relatedAnimals;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Animal>> GetFeaturedAsync(int count = 3, CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            _logger.LogWarning("精選動物數量必須大於 0");
            return [];
        }

        _logger.LogDebug("取得 {Count} 隻精選動物", count);

        var animals = await GetAllAsync(cancellationToken);

        // 優先選擇有瀕危保育狀態的動物作為精選
        var featured = animals
            .OrderByDescending(a => GetConservationPriority(a.ConservationStatus))
            .ThenBy(a => a.ChineseName)
            .Take(count)
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("成功取得 {Count} 隻精選動物", featured.Count);

        return featured;
    }

    /// <summary>
    /// 取得保育等級的優先順序（用於排序精選動物）
    /// </summary>
    /// <param name="status">保育等級</param>
    /// <returns>優先順序數值（越高越優先）</returns>
    private static int GetConservationPriority(ConservationStatus status)
    {
        return status switch
        {
            ConservationStatus.CR => 6, // 極危
            ConservationStatus.EN => 5, // 瀕危
            ConservationStatus.VU => 4, // 易危
            ConservationStatus.NT => 3, // 近危
            ConservationStatus.LC => 2, // 無危
            ConservationStatus.EW => 1, // 野外滅絕
            ConservationStatus.EX => 0, // 滅絕
            _ => 0
        };
    }
}
