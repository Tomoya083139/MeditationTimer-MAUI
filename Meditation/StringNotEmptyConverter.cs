using System.Globalization;

namespace Meditation;

/// <summary>
/// 文字列が空でない場合に true を返すコンバーター。
/// 保存ステータスラベルの IsVisible などに使用。
/// </summary>
public class StringNotEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrEmpty(s);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
