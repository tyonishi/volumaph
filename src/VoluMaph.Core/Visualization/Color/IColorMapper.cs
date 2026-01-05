namespace VoluMaph.Core.Color;

public interface IColorMapper
{
    Color GetColor(long size, long totalSize, ColorTheme theme);
    Color GetColor(double percentage, ColorTheme theme);
}
