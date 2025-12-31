using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace Zoo.Services;

/// <summary>
/// JSON 資料服務實作，提供從 JSON 檔案載入資料的功能，並使用記憶體快取提升效能
/// </summary>
public class JsonDataService : IJsonDataService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<JsonDataService> _logger;
    private readonly string _dataPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly HashSet<string> _cacheKeys = [];
    private readonly object _cacheKeysLock = new();

    /// <summary>
    /// 初始化 JSON 資料服務
    /// </summary>
    /// <param name="cache">記憶體快取</param>
    /// <param name="logger">日誌記錄器</param>
    /// <param name="configuration">應用程式設定</param>
    public JsonDataService(
        IMemoryCache cache,
        ILogger<JsonDataService> logger,
        IConfiguration configuration)
    {
        _cache = cache;
        _logger = logger;
        _dataPath = configuration["DataPaths:JsonData"] ?? "Data";

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> LoadAsync<T>(
        string fileName,
        string rootPropertyName,
        CancellationToken cancellationToken = default) where T : class
    {
        var cacheKey = $"JsonData_{fileName}_{rootPropertyName}_{typeof(T).Name}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<T>? cachedData) && cachedData is not null)
        {
            _logger.LogDebug("從快取載入 {FileName} 的 {TypeName} 資料", fileName, typeof(T).Name);
            return cachedData;
        }

        var filePath = Path.Combine(_dataPath, fileName);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("JSON 檔案不存在: {FilePath}", filePath);
            return [];
        }

        try
        {
            await using var fileStream = File.OpenRead(filePath);
            using var document = await JsonDocument.ParseAsync(fileStream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty(rootPropertyName, out var arrayElement))
            {
                _logger.LogWarning(
                    "JSON 檔案 {FileName} 中找不到屬性 {PropertyName}",
                    fileName,
                    rootPropertyName);
                return [];
            }

            var data = arrayElement.Deserialize<List<T>>(_jsonOptions) ?? [];
            var readOnlyData = data.AsReadOnly();

            // 快取資料，設定 5 分鐘過期
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            _cache.Set(cacheKey, readOnlyData, cacheOptions);

            lock (_cacheKeysLock)
            {
                _cacheKeys.Add(cacheKey);
            }

            _logger.LogInformation(
                "從 {FileName} 載入 {Count} 筆 {TypeName} 資料",
                fileName,
                readOnlyData.Count,
                typeof(T).Name);

            return readOnlyData;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "解析 JSON 檔案 {FileName} 時發生錯誤", fileName);
            throw;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "讀取 JSON 檔案 {FileName} 時發生錯誤", fileName);
            throw;
        }
    }

    /// <inheritdoc />
    public void ClearCache(string fileName)
    {
        lock (_cacheKeysLock)
        {
            var keysToRemove = _cacheKeys
                .Where(k => k.StartsWith($"JsonData_{fileName}_", StringComparison.Ordinal))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _cacheKeys.Remove(key);
            }

            _logger.LogInformation("已清除 {FileName} 的快取，共 {Count} 筆", fileName, keysToRemove.Count);
        }
    }

    /// <inheritdoc />
    public void ClearAllCache()
    {
        lock (_cacheKeysLock)
        {
            foreach (var key in _cacheKeys)
            {
                _cache.Remove(key);
            }

            var count = _cacheKeys.Count;
            _cacheKeys.Clear();

            _logger.LogInformation("已清除所有 JSON 資料快取，共 {Count} 筆", count);
        }
    }
}
