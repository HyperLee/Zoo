namespace Zoo.Models;

/// <summary>
/// 座標記錄類型，用於地圖定位
/// </summary>
/// <param name="X">X 座標值</param>
/// <param name="Y">Y 座標值</param>
public record Coordinate(double X, double Y);
