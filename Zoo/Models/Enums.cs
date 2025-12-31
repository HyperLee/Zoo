namespace Zoo.Models;

/// <summary>
/// 生物分類
/// </summary>
public enum BiologicalClass
{
    /// <summary>哺乳類</summary>
    Mammal,
    /// <summary>鳥類</summary>
    Bird,
    /// <summary>爬蟲類</summary>
    Reptile,
    /// <summary>兩棲類</summary>
    Amphibian,
    /// <summary>魚類</summary>
    Fish,
    /// <summary>無脊椎動物</summary>
    Invertebrate
}

/// <summary>
/// 棲息地
/// </summary>
public enum Habitat
{
    /// <summary>熱帶雨林</summary>
    TropicalRainforest,
    /// <summary>沙漠</summary>
    Desert,
    /// <summary>草原</summary>
    Grassland,
    /// <summary>極地</summary>
    Polar,
    /// <summary>海洋</summary>
    Ocean,
    /// <summary>淡水</summary>
    Freshwater,
    /// <summary>山區</summary>
    Mountain
}

/// <summary>
/// 飲食習性
/// </summary>
public enum Diet
{
    /// <summary>肉食性</summary>
    Carnivore,
    /// <summary>草食性</summary>
    Herbivore,
    /// <summary>雜食性</summary>
    Omnivore
}

/// <summary>
/// 活動時間
/// </summary>
public enum ActivityPattern
{
    /// <summary>日行性</summary>
    Diurnal,
    /// <summary>夜行性</summary>
    Nocturnal,
    /// <summary>晨昏性</summary>
    Crepuscular
}

/// <summary>
/// IUCN 保育等級
/// </summary>
public enum ConservationStatus
{
    /// <summary>無危 (Least Concern)</summary>
    LC,
    /// <summary>近危 (Near Threatened)</summary>
    NT,
    /// <summary>易危 (Vulnerable)</summary>
    VU,
    /// <summary>瀕危 (Endangered)</summary>
    EN,
    /// <summary>極危 (Critically Endangered)</summary>
    CR,
    /// <summary>野外滅絕 (Extinct in the Wild)</summary>
    EW,
    /// <summary>滅絕 (Extinct)</summary>
    EX
}

/// <summary>
/// 路線類型
/// </summary>
public enum RouteType
{
    /// <summary>完整路線</summary>
    Complete,
    /// <summary>精華路線</summary>
    Highlights,
    /// <summary>親子路線</summary>
    FamilyFriendly,
    /// <summary>攝影路線</summary>
    Photography,
    /// <summary>主題路線</summary>
    Themed
}

/// <summary>
/// 設施類型
/// </summary>
public enum FacilityType
{
    /// <summary>休息區</summary>
    RestArea,
    /// <summary>洗手間</summary>
    Restroom,
    /// <summary>服務中心</summary>
    ServiceCenter,
    /// <summary>急救站</summary>
    FirstAid,
    /// <summary>餐飲區</summary>
    FoodCourt,
    /// <summary>紀念品店</summary>
    GiftShop
}

/// <summary>
/// 測驗題目類型
/// </summary>
public enum QuizType
{
    /// <summary>選擇題</summary>
    MultipleChoice,
    /// <summary>是非題</summary>
    TrueFalse
}
