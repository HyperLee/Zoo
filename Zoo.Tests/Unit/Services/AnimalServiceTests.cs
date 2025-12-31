using Microsoft.Extensions.Logging;
using Moq;
using Zoo.Models;
using Zoo.Services;

namespace Zoo.Tests.Unit.Services;

/// <summary>
/// AnimalService 單元測試
/// </summary>
public class AnimalServiceTests
{
    private readonly Mock<IJsonDataService> _mockJsonDataService;
    private readonly Mock<ILogger<AnimalService>> _mockLogger;
    private readonly AnimalService _sut;

    public AnimalServiceTests()
    {
        _mockJsonDataService = new Mock<IJsonDataService>();
        _mockLogger = new Mock<ILogger<AnimalService>>();
        _sut = new AnimalService(_mockJsonDataService.Object, _mockLogger.Object);
    }

    private static List<Animal> CreateTestAnimals()
    {
        return
        [
            new Animal
            {
                Id = "lion-001",
                ChineseName = "非洲獅",
                EnglishName = "African Lion",
                ScientificName = "Panthera leo",
                ZoneId = "africa-zone",
                Classification = new Classification
                {
                    BiologicalClass = BiologicalClass.Mammal,
                    Habitat = Habitat.Grassland,
                    Diet = Diet.Carnivore,
                    ActivityPattern = ActivityPattern.Crepuscular
                },
                Description = new Description
                {
                    Size = "體長 1.4-2.5 公尺",
                    Appearance = "雄獅擁有標誌性的鬃毛",
                    Behavior = "群居動物",
                    FullDescriptionZh = "非洲獅是現存最大的貓科動物之一",
                    FullDescriptionEn = "The African lion is one of the largest cats"
                },
                FunFacts = ["獅子每天可睡眠長達 20 小時"],
                ConservationStatus = ConservationStatus.VU,
                Media = new MediaResources
                {
                    Images = ["/images/animals/lion-001-1.webp"],
                    ThumbnailPath = "/images/animals/lion-001-thumb.webp"
                },
                RelatedAnimalIds = ["elephant-001", "giraffe-001"]
            },
            new Animal
            {
                Id = "elephant-001",
                ChineseName = "非洲象",
                EnglishName = "African Elephant",
                ScientificName = "Loxodonta africana",
                ZoneId = "africa-zone",
                Classification = new Classification
                {
                    BiologicalClass = BiologicalClass.Mammal,
                    Habitat = Habitat.Grassland,
                    Diet = Diet.Herbivore,
                    ActivityPattern = ActivityPattern.Diurnal
                },
                Description = new Description
                {
                    Size = "肩高 3-4 公尺",
                    Appearance = "擁有巨大的耳朵和長長的象鼻",
                    Behavior = "由年長雌象領導的母系社會",
                    FullDescriptionZh = "非洲象是地球上現存最大的陸地動物",
                    FullDescriptionEn = "The African elephant is the largest living land animal"
                },
                FunFacts = ["大象是唯一不能跳躍的哺乳動物"],
                ConservationStatus = ConservationStatus.EN,
                Media = new MediaResources
                {
                    Images = ["/images/animals/elephant-001-1.webp"],
                    ThumbnailPath = "/images/animals/elephant-001-thumb.webp"
                },
                RelatedAnimalIds = ["lion-001"]
            },
            new Animal
            {
                Id = "giraffe-001",
                ChineseName = "長頸鹿",
                EnglishName = "Giraffe",
                ScientificName = "Giraffa camelopardalis",
                ZoneId = "africa-zone",
                Classification = new Classification
                {
                    BiologicalClass = BiologicalClass.Mammal,
                    Habitat = Habitat.Grassland,
                    Diet = Diet.Herbivore,
                    ActivityPattern = ActivityPattern.Diurnal
                },
                Description = new Description
                {
                    Size = "身高 4.5-5.8 公尺",
                    Appearance = "擁有獨特的長脖子和斑點花紋",
                    Behavior = "性情溫和的群居動物",
                    FullDescriptionZh = "長頸鹿是世界上最高的陸地動物",
                    FullDescriptionEn = "The giraffe is the tallest living land animal"
                },
                FunFacts = ["長頸鹿的舌頭長達 50 公分"],
                ConservationStatus = ConservationStatus.VU,
                Media = new MediaResources
                {
                    Images = ["/images/animals/giraffe-001-1.webp"],
                    ThumbnailPath = "/images/animals/giraffe-001-thumb.webp"
                },
                RelatedAnimalIds = []
            }
        ];
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllAnimals()
    {
        var testAnimals = CreateTestAnimals();
        _mockJsonDataService
            .Setup(x => x.LoadAsync<Animal>("animals.json", "animals", It.IsAny<CancellationToken>()))
            .ReturnsAsync(testAnimals.AsReadOnly());

        var result = await _sut.GetAllAsync();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, a => a.Id == "lion-001");
        Assert.Contains(result, a => a.Id == "elephant-001");
        Assert.Contains(result, a => a.Id == "giraffe-001");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoAnimals()
    {
        _mockJsonDataService
            .Setup(x => x.LoadAsync<Animal>("animals.json", "animals", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Animal>().ToList().AsReadOnly());

        var result = await _sut.GetAllAsync();

        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsAnimal_WhenExists()
    {
        var testAnimals = CreateTestAnimals();
        _mockJsonDataService
            .Setup(x => x.LoadAsync<Animal>("animals.json", "animals", It.IsAny<CancellationToken>()))
            .ReturnsAsync(testAnimals.AsReadOnly());

        var result = await _sut.GetByIdAsync("lion-001");

        Assert.NotNull(result);
        Assert.Equal("lion-001", result.Id);
        Assert.Equal("非洲獅", result.ChineseName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var testAnimals = CreateTestAnimals();
        _mockJsonDataService
            .Setup(x => x.LoadAsync<Animal>("animals.json", "animals", It.IsAny<CancellationToken>()))
            .ReturnsAsync(testAnimals.AsReadOnly());

        var result = await _sut.GetByIdAsync("nonexistent-id");

        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByIdAsync_ReturnsNull_WhenIdIsNullOrEmpty(string? id)
    {
        var result = await _sut.GetByIdAsync(id!);

        Assert.Null(result);
        _mockJsonDataService.Verify(
            x => x.LoadAsync<Animal>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_IsCaseInsensitive()
    {
        var testAnimals = CreateTestAnimals();
        _mockJsonDataService
            .Setup(x => x.LoadAsync<Animal>("animals.json", "animals", It.IsAny<CancellationToken>()))
            .ReturnsAsync(testAnimals.AsReadOnly());

        var result = await _sut.GetByIdAsync("LION-001");

        Assert.NotNull(result);
        Assert.Equal("lion-001", result.Id);
    }

    #endregion

    #region GetRelatedAsync Tests

    [Fact]
    public async Task GetRelatedAsync_ReturnsRelatedAnimals()
    {
        var testAnimals = CreateTestAnimals();
        _mockJsonDataService
            .Setup(x => x.LoadAsync<Animal>("animals.json", "animals", It.IsAny<CancellationToken>()))
            .ReturnsAsync(testAnimals.AsReadOnly());

        var result = await _sut.GetRelatedAsync("lion-001");

        Assert.Equal(2, result.Count);
        Assert.Contains(result, a => a.Id == "elephant-001");
        Assert.Contains(result, a => a.Id == "giraffe-001");
    }

    [Fact]
    public async Task GetRelatedAsync_ReturnsEmptyList_WhenNoRelatedAnimals()
    {
        var testAnimals = CreateTestAnimals();
        _mockJsonDataService
            .Setup(x => x.LoadAsync<Animal>("animals.json", "animals", It.IsAny<CancellationToken>()))
            .ReturnsAsync(testAnimals.AsReadOnly());

        var result = await _sut.GetRelatedAsync("giraffe-001");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRelatedAsync_ReturnsEmptyList_WhenAnimalNotFound()
    {
        var testAnimals = CreateTestAnimals();
        _mockJsonDataService
            .Setup(x => x.LoadAsync<Animal>("animals.json", "animals", It.IsAny<CancellationToken>()))
            .ReturnsAsync(testAnimals.AsReadOnly());

        var result = await _sut.GetRelatedAsync("nonexistent-id");

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetRelatedAsync_ReturnsEmptyList_WhenIdIsNullOrEmpty(string? id)
    {
        var result = await _sut.GetRelatedAsync(id!);

        Assert.Empty(result);
    }

    #endregion

    #region GetFeaturedAsync Tests

    [Fact]
    public async Task GetFeaturedAsync_ReturnsRequestedCount()
    {
        var testAnimals = CreateTestAnimals();
        _mockJsonDataService
            .Setup(x => x.LoadAsync<Animal>("animals.json", "animals", It.IsAny<CancellationToken>()))
            .ReturnsAsync(testAnimals.AsReadOnly());

        var result = await _sut.GetFeaturedAsync(2);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetFeaturedAsync_PrioritizesEndangeredAnimals()
    {
        var testAnimals = CreateTestAnimals();
        _mockJsonDataService
            .Setup(x => x.LoadAsync<Animal>("animals.json", "animals", It.IsAny<CancellationToken>()))
            .ReturnsAsync(testAnimals.AsReadOnly());

        var result = await _sut.GetFeaturedAsync(1);

        Assert.Single(result);
        // 非洲象 (EN) 應該排在最前面，因為瀕危等級最高
        Assert.Equal("elephant-001", result[0].Id);
    }

    [Fact]
    public async Task GetFeaturedAsync_ReturnsAllAnimals_WhenCountExceedsTotal()
    {
        var testAnimals = CreateTestAnimals();
        _mockJsonDataService
            .Setup(x => x.LoadAsync<Animal>("animals.json", "animals", It.IsAny<CancellationToken>()))
            .ReturnsAsync(testAnimals.AsReadOnly());

        var result = await _sut.GetFeaturedAsync(10);

        Assert.Equal(3, result.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetFeaturedAsync_ReturnsEmptyList_WhenCountIsInvalid(int count)
    {
        var result = await _sut.GetFeaturedAsync(count);

        Assert.Empty(result);
        _mockJsonDataService.Verify(
            x => x.LoadAsync<Animal>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion
}
