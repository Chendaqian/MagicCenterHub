using System.Globalization;
using System.Windows.Data;

namespace MagicCenterHub.Utils;

/// <summary>
/// 百分比→宽度转换器，用于进度条绑定
/// </summary>
/// <remark>ConverterParameter 传入最大宽度值（像素），将 0-100 的百分比转换为对应的像素宽度</remark>
public class PercentToWidthConverter : IValueConverter
{
    /// <summary>
    /// 将百分比值转换为像素宽度
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percent && parameter is string maxStr && double.TryParse(maxStr, out double maxWidth))
        {
            double result = percent / 100.0 * maxWidth;
            return Math.Max(0, Math.Min(result, maxWidth));
        }
        return 0.0;
    }

    /// <summary>
    /// 反向转换（未实现）
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
