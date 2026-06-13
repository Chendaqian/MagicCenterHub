using System.Windows.Media;

namespace MagicCenterHub.Effects;

/// <summary>
/// LED 灯效输出颜色，包含红绿黄三个通道的当前颜色值
/// </summary>
public struct LedColor
{
    /// <summary>
    /// 绿灯当前颜色
    /// </summary>
    public Color Green;

    /// <summary>
    /// 红灯当前颜色
    /// </summary>
    public Color Red;

    /// <summary>
    /// 黄灯当前颜色
    /// </summary>
    public Color Yellow;

    /// <summary>
    /// 初始化为全黑（三灯全灭）
    /// </summary>
    public LedColor()
    {
        Green = Colors.Black;
        Red = Colors.Black;
        Yellow = Colors.Black;
    }
}

/// <summary>
/// LED 灯效接口，所有 19 种灯效模式均实现此接口
/// 基于时间戳的非阻塞状态机，与 Arduino millis() 模式一致
/// </summary>
public interface ILedEffect
{
    /// <summary>
    /// 重置灯效状态，切换模式时调用
    /// </summary>
    void Reset();

    /// <summary>
    /// 根据已过去的时间计算当前帧的颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>
    /// <returns>当前帧的红绿黄三通道颜色</returns>
    LedColor Update(long elapsedMs);
}