using System.Globalization;

namespace Meditation;

/// <summary>
/// boolean 値を反転させるコンバーター。
/// XAML 内で "IsVisible={Binding IsAlarmActive, Converter={StaticResource InvertedBoolConverter}}" 
/// のように使うことで、アラーム鳴動中に通常のボタンを隠すなどの制御ができる。
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return value;
    }
}
