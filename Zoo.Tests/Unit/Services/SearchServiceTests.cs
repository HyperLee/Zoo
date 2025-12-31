using Microsoft.Extensions.Logging;
using Moq;
using Zoo.Models;
using Zoo.Services;

namespace Zoo.Tests.Unit.Services;

/// <summary>
/// SearchService 單元測試
/// </summary>
public class SearchServiceTests
{
    private readonly Mock<IAnimalService> _mockAnimalService;
    private readonly Mock<ILogger<SearchService>> _mockLogger;
    private readonly SearchService _sut;

    public SearchServiceTests()
    {
        _mockAnimalService = new Mock<IAnimalService>();
        _mockLogger = new Mock<ILogger<SearchService>>();
        _sut = new SearchService(_mockAnimalService.Object, _mockLogger.Object);
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
                    Behavior = "群居動物，以家族為單位生活",
                    FullDescriptionZh = "非洲獅是現存最大的貓科動物之一",
                    FullDescriptionEn = "The African lion is one of the largest cats"
                },
                FunFacts = ["獅子每天可睡眠長達 20 小時", "獅子的吼叫聲可傳達 8 公里遠"],
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
                Id = "penguin-001",
                ChineseName = "皇帝企鵝",
                EnglishName = "Emperor Penguin",
                ScientificName = "Aptenodytes forsteri",
                ZoneId = "polar-zone",
                Classification = new Classification
                {
                    BiologicalClass = BiologicalClass.Bird,
                    Habitat = Habitat.Polar,
                    Diet = Diet.Carnivore,
                    ActivityPattern = ActivityPattern.Diurnal
                },
                Description = new Description
                {
                    Size = "身高 1.1-1.3 公尺",
                    Appearance = "黑白相間的羽毛，頸部有金黃色斑塊",
                    Behavior = "群居動物，擅長游泳",
                    FullDescriptionZh = "皇帝企鵝是現存最大的企鵝物種",
                    FullDescriptionEn = "The emperor penguin is the largest penguin species"
                },
                FunFacts = ["皇帝企鵝可以潛水超過 500 公尺"],
                ConservationStatus = ConservationStatus.NT,
                Media = new MediaResources
                {
                    Images = ["/images/animals/penguin-001-1.webp"],
                    ThumbnailPath = "/images/animals/penguin-001-thumb.webp"
                },
                RelatedAnimalIds = []
            }
        ];
    }

    private void SetupMockAnimals()
    {
        var testAnimals = CreateTestAnimals();
        _mockAnimalService
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(testAnimals.AsReadOnly());
    }

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_WithKeyword_ReturnsMatchingAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { Keyword = "獅" };

        var result = await _sut.SearchAsync(filter);

        Assert.Single(result);
        Assert.Equal("lion-001", result[0].Animal.Id);
        Assert.True(result[0].Score > 0);
    }

    [Fact]
    public async Task SearchAsync_WithEnglishKeyword_ReturnsMatchingAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { Keyword = "Lion" };

        var result = await _sut.SearchAsync(filter);

        Assert.Single(result);
        Assert.Equal("lion-001", result[0].Animal.Id);
    }

    [Fact]
    public async Task SearchAsync_WithBiologicalClassFilter_ReturnsMatchingAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { BiologicalClass = BiologicalClass.Bird };

        var result = await _sut.SearchAsync(filter);

        Assert.Single(result);
        Assert.Equal("penguin-001", result[0].Animal.Id);
    }

    [Fact]
    public async Task SearchAsync_WithHabitatFilter_ReturnsMatchingAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { Habitat = Habitat.Polar };

        var result = await _sut.SearchAsync(filter);

        Assert.Single(result);
        Assert.Equal("penguin-001", result[0].Animal.Id);
    }

    [Fact]
    public async Task SearchAsync_WithDietFilter_ReturnsMatchingAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { Diet = Diet.Herbivore };

        var result = await _sut.SearchAsync(filter);

        Assert.Single(result);
        Assert.Equal("elephant-001", result[0].Animal.Id);
    }

    [Fact]
    public async Task SearchAsync_WithActivityPatternFilter_ReturnsMatchingAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { ActivityPattern = ActivityPattern.Crepuscular };

        var result = await _sut.SearchAsync(filter);

        Assert.Single(result);
        Assert.Equal("lion-001", result[0].Animal.Id);
    }

    [Fact]
    public async Task SearchAsync_WithZoneIdFilter_ReturnsMatchingAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { ZoneId = "africa-zone" };

        var result = await _sut.SearchAsync(filter);

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal("africa-zone", r.Animal.ZoneId));
    }

    [Fact]
    public async Task SearchAsync_WithMultipleFilters_ReturnsMatchingAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter
        {
            BiologicalClass = BiologicalClass.Mammal,
            Diet = Diet.Carnivore
        };

        var result = await _sut.SearchAsync(filter);

        Assert.Single(result);
        Assert.Equal("lion-001", result[0].Animal.Id);
    }

    [Fact]
    public async Task SearchAsync_WithKeywordAndFilters_ReturnsMatchingAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter
        {
            Keyword = "非洲",
            BiologicalClass = BiologicalClass.Mammal
        };

        var result = await _sut.SearchAsync(filter);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Animal.Id == "lion-001");
        Assert.Contains(result, r => r.Animal.Id == "elephant-001");
    }

    [Fact]
    public async Task SearchAsync_WithNoMatchingKeyword_ReturnsEmptyList()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { Keyword = "不存在的動物" };

        var result = await _sut.SearchAsync(filter);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_WithNoFilters_ReturnsAllAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter();

        var result = await _sut.SearchAsync(filter);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SearchAsync_SortsByScoreDescending()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { Keyword = "非洲" };

        var result = await _sut.SearchAsync(filter);

        for (int i = 0; i < result.Count - 1; i++)
        {
            Assert.True(result[i].Score >= result[i + 1].Score);
        }
    }

    [Fact]
    public async Task SearchAsync_MatchesPartialChineseName()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { Keyword = "企鵝" };

        var result = await _sut.SearchAsync(filter);

        Assert.Single(result);
        Assert.Equal("penguin-001", result[0].Animal.Id);
    }

    [Fact]
    public async Task SearchAsync_MatchesDescription()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { Keyword = "鬃毛" };

        var result = await _sut.SearchAsync(filter);

        Assert.Single(result);
        Assert.Equal("lion-001", result[0].Animal.Id);
    }

    [Fact]
    public async Task SearchAsync_MatchesFunFacts()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { Keyword = "跳躍" };

        var result = await _sut.SearchAsync(filter);

        Assert.Single(result);
        Assert.Equal("elephant-001", result[0].Animal.Id);
    }

    #endregion

    #region SuggestAsync Tests

    [Fact]
    public async Task SuggestAsync_WithKeyword_ReturnsSuggestions()
    {
        SetupMockAnimals();

        var result = await _sut.SuggestAsync("獅", 5);

        Assert.Single(result);
        Assert.Equal("lion-001", result[0].Id);
        Assert.Equal("非洲獅", result[0].Name);
    }

    [Fact]
    public async Task SuggestAsync_WithEmptyKeyword_ReturnsEmptyList()
    {
        SetupMockAnimals();

        var result = await _sut.SuggestAsync("", 5);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SuggestAsync_WithNullKeyword_ReturnsEmptyList()
    {
        SetupMockAnimals();

        var result = await _sut.SuggestAsync(null!, 5);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SuggestAsync_RespectsLimit()
    {
        SetupMockAnimals();

        var result = await _sut.SuggestAsync("非洲", 1);

        Assert.Single(result);
    }

    [Fact]
    public async Task SuggestAsync_WithNoMatchingKeyword_ReturnsEmptyList()
    {
        SetupMockAnimals();

        var result = await _sut.SuggestAsync("不存在", 5);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SuggestAsync_SortsByScoreDescending()
    {
        SetupMockAnimals();

        var result = await _sut.SuggestAsync("非", 5);

        Assert.True(result.Count >= 2);
    }

    #endregion

    #region FilterAsync Tests

    [Fact]
    public async Task FilterAsync_WithBiologicalClassFilter_ReturnsMatchingAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { BiologicalClass = BiologicalClass.Mammal };

        var result = await _sut.FilterAsync(filter);

        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal(BiologicalClass.Mammal, a.Classification.BiologicalClass));
    }

    [Fact]
    public async Task FilterAsync_WithHabitatFilter_ReturnsMatchingAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter { Habitat = Habitat.Grassland };

        var result = await _sut.FilterAsync(filter);

        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal(Habitat.Grassland, a.Classification.Habitat));
    }

    [Fact]
    public async Task FilterAsync_WithMultipleFilters_ReturnsMatchingAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter
        {
            BiologicalClass = BiologicalClass.Mammal,
            Habitat = Habitat.Grassland,
            Diet = Diet.Carnivore
        };

        var result = await _sut.FilterAsync(filter);

        Assert.Single(result);
        Assert.Equal("lion-001", result[0].Id);
    }

    [Fact]
    public async Task FilterAsync_WithNoFilters_ReturnsAllAnimals()
    {
        SetupMockAnimals();
        var filter = new SearchFilter();

        var result = await _sut.FilterAsync(filter);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task FilterAsync_WithNoMatchingFilter_ReturnsEmptyList()
    {
        SetupMockAnimals();
        var filter = new SearchFilter
        {
            BiologicalClass = BiologicalClass.Reptile
        };

        var result = await _sut.FilterAsync(filter);

        Assert.Empty(result);
    }

    [Fact]
    public async Task FilterAsync_IgnoresKeywordInFilter()
    {
        SetupMockAnimals();
        var filter = new SearchFilter
        {
            Keyword = "獅",
            BiologicalClass = BiologicalClass.Mammal
        };

        var result = await _sut.FilterAsync(filter);

        Assert.Equal(2, result.Count);
    }

    #endregion
}
