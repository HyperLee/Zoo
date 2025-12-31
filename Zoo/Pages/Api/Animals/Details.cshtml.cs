using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Services;

namespace Zoo.Pages.Api.Animals;

/// <summary>
/// 單一動物詳情 API 端點
/// GET /api/animals/{id}
/// </summary>
public class DetailsModel : PageModel
{
    private readonly IAnimalService _animalService;
    private readonly ILogger<DetailsModel> _logger;

    /// <summary>
    /// 初始化單一動物 API
    /// </summary>
    /// <param name="animalService">動物服務</param>
    /// <param name="logger">日誌記錄器</param>
    public DetailsModel(IAnimalService animalService, ILogger<DetailsModel> logger)
    {
        _animalService = animalService;
        _logger = logger;
    }

    /// <summary>
    /// 處理單一動物請求
    /// </summary>
    /// <param name="id">動物 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>動物詳情的 JSON 回應</returns>
    public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("收到單一動物請求，ID: {Id}", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            return new JsonResult(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = "動物 ID 不得為空",
                instance = HttpContext.Request.Path.ToString()
            })
            {
                StatusCode = 400
            };
        }

        var animal = await _animalService.GetByIdAsync(id, cancellationToken);

        if (animal is null)
        {
            _logger.LogWarning("找不到動物，ID: {Id}", id);

            return new JsonResult(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Not Found",
                status = 404,
                detail = $"找不到 ID 為 '{id}' 的動物",
                instance = HttpContext.Request.Path.ToString()
            })
            {
                StatusCode = 404
            };
        }

        // 取得相關動物
        var relatedAnimals = await _animalService.GetRelatedAsync(id, cancellationToken);

        _logger.LogInformation("動物詳情請求完成，ID: {Id}", id);

        return new JsonResult(new
        {
            id = animal.Id,
            chineseName = animal.ChineseName,
            englishName = animal.EnglishName,
            scientificName = animal.ScientificName,
            zoneId = animal.ZoneId,
            classification = new
            {
                biologicalClass = animal.Classification.BiologicalClass.ToString(),
                habitat = animal.Classification.Habitat.ToString(),
                diet = animal.Classification.Diet.ToString(),
                activityPattern = animal.Classification.ActivityPattern.ToString()
            },
            description = new
            {
                size = animal.Description.Size,
                appearance = animal.Description.Appearance,
                behavior = animal.Description.Behavior,
                fullDescriptionZh = animal.Description.FullDescriptionZh,
                fullDescriptionEn = animal.Description.FullDescriptionEn
            },
            funFacts = animal.FunFacts,
            conservationStatus = animal.ConservationStatus.ToString(),
            media = new
            {
                images = animal.Media.Images,
                soundPath = animal.Media.SoundPath,
                thumbnailPath = animal.Media.ThumbnailPath
            },
            relatedAnimals = relatedAnimals.Select(r => new
            {
                id = r.Id,
                chineseName = r.ChineseName,
                englishName = r.EnglishName,
                thumbnailUrl = r.Media.ThumbnailPath
            })
        });
    }
}
