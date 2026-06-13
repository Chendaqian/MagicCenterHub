using MagicCenterHub.Models;

namespace MagicCenterHub.Effects;

/// <summary>
/// LED 灯效工厂，根据模式编号创建对应的灯效实例
/// </summary>
public static class LedEffectFactory
{
    /// <summary>
    /// 创建指定模式的灯效实例
    /// </summary>
    /// <param name="mode">灯效模式编号（0-19）</param>
    /// <returns>对应的灯效实现，未知模式返回全灭</returns>
    public static ILedEffect Create(LedMode mode)
    {
        return mode switch
        {
            LedMode.BothOff => new BothOffEffect(),
            LedMode.BothFlash => new BothFlashEffect(),
            LedMode.GreenFlash => new GreenFlashEffect(),
            LedMode.RedFlash => new RedFlashEffect(),
            LedMode.YellowFlash => new YellowFlashEffect(),
            LedMode.GreenOn => new GreenOnEffect(),
            LedMode.RedOn => new RedOnEffect(),
            LedMode.YellowOn => new YellowOnEffect(),
            LedMode.BothOn => new BothOnEffect(),
            LedMode.PoliceAlt => new PoliceAltEffect(),
            LedMode.Heartbeat => new HeartbeatEffect(),
            LedMode.Sos => new SosEffect(),
            LedMode.Breathing => new BreathingEffect(),
            LedMode.Firefly => new FireflyEffect(),
            LedMode.Ecg => new EcgEffect(),
            LedMode.PhaseChase => new PhaseChaseEffect(),
            LedMode.StrobeChase => new StrobeChaseEffect(),
            LedMode.Taichi => new TaichiEffect(),
            LedMode.HelloMorse => new HelloMorseEffect(),
            LedMode.Radar => new RadarEffect(),
            _ => new BothOffEffect()
        };
    }
}