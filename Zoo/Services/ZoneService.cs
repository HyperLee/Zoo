using Zoo.Models;

namespace Zoo.Services;

/// <summary>
/// 區域服務實作，提供園區區域資料的存取功能
/// </summary>
public class ZoneService : IZoneService
{
    private readonly IJsonDataService _jsonDataService;
    private readonly IAnimalService _animalService;
    private readonly ILogger<ZoneService> _logger;

    /// <summary>
    /// 初始化區域服務
    /// </summary>
    /// <param name="jsonDataService">JSON 資料服務</param>
    /// <param name="animalService">動物服務</param>
    /// <param name="logger">日誌記錄器</param>
    public ZoneService(
        IJsonDataService jsonDataService,
        IAnimalService animalService,
        ILogger<ZoneService> logger)
    {
        _jsonDataService = jsonDataService;
        _animalService = animalService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Zone>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("取得所有區域資料");

        var zones = await _jsonDataService.LoadAsync<Zone>(
            "zones.json",
            "zones",
            cancellationToken);

        _logger.LogInformation("成功取得 {Count} 個區域", zones.Count);

        return zones;
    }

    /// <inheritdoc />
    public async Task<Zone?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("區域 ID 不可為空");
            return null;
        }

        _logger.LogDebug("根據 ID 取得區域: {Id}", id);

        var zones = await GetAllAsync(cancellationToken);
        var zone = zones.FirstOrDefault(z => z.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (zone is null)
        {
            _logger.LogWarning("找不到 ID 為 {Id} 的區域", id);
        }
        else
        {
            _logger.LogDebug("成功取得區域: {NameZh} ({Id})", zone.NameZh, zone.Id);
        }

        return zone;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Animal>> GetAnimalsByZoneAsync(string zoneId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(zoneId))
        {
            _logger.LogWarning("區域 ID 不可為空");
            return [];
        }

        _logger.LogDebug("取得區域 {ZoneId} 內的動物", zoneId);

        var animals = await _animalService.GetAllAsync(cancellationToken);
        var zoneAnimals = animals
            .Where(a => a.ZoneId.Equals(zoneId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _logger.LogInformation("區域 {ZoneId} 內有 {Count} 隻動物", zoneId, zoneAnimals.Count);

        return zoneAnimals;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Zone, int>> GetZoneAnimalCountsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("取得所有區域及其動物數量");

        var zones = await GetAllAsync(cancellationToken);
        var animals = await _animalService.GetAllAsync(cancellationToken);

        var zoneCounts = zones.ToDictionary(
            zone => zone,
            zone => animals.Count(a => a.ZoneId.Equals(zone.Id, StringComparison.OrdinalIgnoreCase)));

        _logger.LogInformation("成功計算 {Count} 個區域的動物數量", zoneCounts.Count);

        return zoneCounts;
    }
}
