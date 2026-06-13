namespace MagicCenterHub.Models;

/// <summary>
/// LED 灯效模式枚举，共 20 种模式
/// </summary>
public enum LedMode
{
    /// <summary>
    /// 全灭
    /// </summary>
    BothOff = 0,

    /// <summary>
    /// 三灯同频闪烁 500ms
    /// </summary>
    BothFlash = 1,

    /// <summary>
    /// 绿灯闪烁 500ms
    /// </summary>
    GreenFlash = 2,

    /// <summary>
    /// 红灯闪烁 500ms
    /// </summary>
    RedFlash = 3,

    /// <summary>
    /// 黄灯闪烁 500ms
    /// </summary>
    YellowFlash = 4,

    /// <summary>
    /// 绿灯常亮
    /// </summary>
    GreenOn = 5,

    /// <summary>
    /// 红灯常亮
    /// </summary>
    RedOn = 6,

    /// <summary>
    /// 黄灯常亮
    /// </summary>
    YellowOn = 7,

    /// <summary>
    /// 三灯常亮
    /// </summary>
    BothOn = 8,

    /// <summary>
    /// 警车：红绿交替 300ms + 黄灯独立快闪 150ms
    /// </summary>
    PoliceAlt = 9,

    /// <summary>
    /// 心跳：80ms亮→100ms灭→80ms亮→600ms灭
    /// </summary>
    Heartbeat = 10,

    /// <summary>
    /// SOS 摩尔斯求救码：三短三长三短
    /// </summary>
    Sos = 11,

    /// <summary>
    /// 三色轮转呼吸灯，红绿反向渐变 + 黄灯 120° 相位偏移
    /// </summary>
    Breathing = 12,

    /// <summary>
    /// 萤火虫：三灯独立正弦波，周期不同产生非对称律动
    /// </summary>
    Firefly = 13,

    /// <summary>
    /// 心电波模拟：红灯 ECG 波形，绿灯 QRS 峰暴闪，黄灯 P/T 波柔和发光
    /// </summary>
    Ecg = 14,

    /// <summary>
    /// 三相正弦追逐：绿 sin、红 cos、黄 sin+120°，3s 周期
    /// </summary>
    PhaseChase = 15,

    /// <summary>
    /// 急救爆闪追击：绿→黄→红各闪 3 下，80ms 亮/80ms 灭
    /// </summary>
    StrobeChase = 16,

    /// <summary>
    /// 太极呼吸：红绿 sin³ 对立消长，黄灯独立 120° 相位偏移
    /// </summary>
    Taichi = 17,

    /// <summary>
    /// HELLO 摩尔斯码广播
    /// </summary>
    HelloMorse = 18,

    /// <summary>
    /// 雷达扫描与锁定：3s 绿灯正弦扫描 + 黄灯回波 → 1s 红灯锁定闪动 → 0.5s 三灯全亮
    /// </summary>
    Radar = 19
}