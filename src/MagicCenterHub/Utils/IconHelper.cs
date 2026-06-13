using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MagicCenterHub.Utils;

/// <summary>
/// 从 Unicode 字符生成窗口图标
/// </summary>
internal static class IconHelper
{
    /// <summary>
    /// 从 Unicode 字符生成窗口图标
    /// </summary>
    /// <param name="text">要渲染的 Unicode 字符（如 emoji）</param>
    /// <param name="r">颜色 R 分量</param>
    /// <param name="g">颜色 G 分量</param>
    /// <param name="b">颜色 B 分量</param>
    /// <param name="size">图标尺寸（像素）</param>
    public static ImageSource CreateIcon(string text, byte r, byte g, byte b, int size = 32)
    {
        using Bitmap bmp = new Bitmap(size, size);
        using Graphics g2 = Graphics.FromImage(bmp);
        g2.SmoothingMode = SmoothingMode.AntiAlias;
        g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        g2.Clear(System.Drawing.Color.Transparent);

        using SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(r, g, b));
        using Font font = new Font("Segoe UI Emoji", size * 0.55f, FontStyle.Regular, GraphicsUnit.Pixel);
        StringFormat sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g2.DrawString(text, font, brush, new RectangleF(0, 0, size, size), sf);

        // 转为 WPF ImageSource
        using MemoryStream ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        ms.Position = 0;
        PngBitmapDecoder decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

        return decoder.Frames[0];
    }
}