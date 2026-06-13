using System.Windows.Media;

namespace MagicCenterHub.Effects;

/// <summary>
/// 模式 0: 全灭
/// </summary>
public class BothOffEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs) => new();
}

/// <summary>
/// 模式 1: 红绿黄三灯同频闪烁，500ms 亮 / 500ms 灭
/// </summary>
public class BothFlashEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs)
    {
        bool on = (elapsedMs / 500) % 2 == 0;
        return new LedColor
        {
            Green = on ? Color.FromRgb(0, 192, 0) : Colors.Black,
            Red = on ? Color.FromRgb(255, 68, 68) : Colors.Black,
            Yellow = on ? Color.FromRgb(240, 160, 48) : Colors.Black
        };
    }
}

/// <summary>
/// 模式 2: 绿灯闪烁，500ms 间隔
/// </summary>
public class GreenFlashEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs)
    {
        bool on = (elapsedMs / 500) % 2 == 0;
        return new LedColor
        {
            Green = on ? Color.FromRgb(0, 192, 0) : Colors.Black,
            Red = Colors.Black,
            Yellow = Colors.Black
        };
    }
}

/// <summary>
/// 模式 3: 红灯闪烁，500ms 间隔
/// </summary>
public class RedFlashEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs)
    {
        bool on = (elapsedMs / 500) % 2 == 0;
        return new LedColor
        {
            Green = Colors.Black,
            Red = on ? Color.FromRgb(255, 68, 68) : Colors.Black,
            Yellow = Colors.Black
        };
    }
}

/// <summary>
/// 模式 4: 黄灯闪烁，500ms 间隔
/// </summary>
public class YellowFlashEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs)
    {
        bool on = (elapsedMs / 500) % 2 == 0;
        return new LedColor
        {
            Green = Colors.Black,
            Red = Colors.Black,
            Yellow = on ? Color.FromRgb(240, 160, 48) : Colors.Black
        };
    }
}

/// <summary>
/// 模式 5: 绿灯常亮
/// </summary>
public class GreenOnEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs) => new()
    {
        Green = Color.FromRgb(0, 192, 0),
        Red = Colors.Black,
        Yellow = Colors.Black
    };
}

/// <summary>
/// 模式 6: 红灯常亮
/// </summary>
public class RedOnEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs) => new()
    {
        Green = Colors.Black,
        Red = Color.FromRgb(255, 68, 68),
        Yellow = Colors.Black
    };
}

/// <summary>
/// 模式 7: 黄灯常亮
/// </summary>
public class YellowOnEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs) => new()
    {
        Green = Colors.Black,
        Red = Colors.Black,
        Yellow = Color.FromRgb(240, 160, 48)
    };
}

/// <summary>
/// 模式 8: 红绿黄三灯同时常亮
/// </summary>
public class BothOnEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs) => new()
    {
        Green = Color.FromRgb(0, 192, 0),
        Red = Color.FromRgb(255, 68, 68),
        Yellow = Color.FromRgb(240, 160, 48)
    };
}

/// <summary>
/// 模式 9: 警车交替快闪，红绿灯反相交替，黄灯独立闪烁
/// </summary>
public class PoliceAltEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>
    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>
    public LedColor Update(long elapsedMs)
    {
        bool phase = (elapsedMs / 300) % 2 == 0;
        bool yellowFlash = (elapsedMs / 150) % 2 == 0; // 黄灯独立快闪
        return new LedColor
        {
            Green = phase ? Color.FromRgb(0, 192, 0) : Colors.Black,
            Red = phase ? Colors.Black : Color.FromRgb(255, 68, 68),
            Yellow = yellowFlash ? Color.FromRgb(240, 160, 48) : Colors.Black
        };
    }
}

/// <summary>
/// 模式 10: 心跳双闪，80ms亮→100ms灭→80ms亮→600ms灭
/// </summary>
public class HeartbeatEffect : ILedEffect
{
    private static readonly int[] Pattern = [80, 100, 80, 600];

    /// <summary>
    /// 重置灯效状态
    /// </summary>
    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>
    public LedColor Update(long elapsedMs)
    {
        long cycleMs = 0;
        foreach (int p in Pattern) cycleMs += p;
        long pos = elapsedMs % cycleMs;
        int step = 0;
        long accumulated = 0;
        for (int i = 0; i < Pattern.Length; i++)
        {
            accumulated += Pattern[i];
            if (pos < accumulated) { step = i; break; }
        }
        bool on = step % 2 == 0;
        return new LedColor
        {
            Green = on ? Color.FromRgb(0, 192, 0) : Colors.Black,
            Red = on ? Color.FromRgb(255, 68, 68) : Colors.Black,
            Yellow = on ? Color.FromRgb(240, 160, 48) : Colors.Black
        };
    }
}

/// <summary>
/// 模式 11: SOS 摩尔斯码
/// </summary>
public class SosEffect : ILedEffect
{
    private static readonly int[] Pattern = [
        200, 200, 200, 200, 200, 200,
        400,
        600, 200, 600, 200, 600, 200,
        400,
        200, 200, 200, 200, 200, 1000
    ];

    /// <summary>
    /// 重置灯效状态
    /// </summary>
    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs)
    {
        long cycleMs = 0;
        foreach (int p in Pattern) cycleMs += p;
        long pos = elapsedMs % cycleMs;
        int step = 0;
        long accumulated = 0;
        for (int i = 0; i < Pattern.Length; i++)
        {
            accumulated += Pattern[i];
            if (pos < accumulated) { step = i; break; }
        }
        bool on = step % 2 == 0;
        return new LedColor
        {
            Green = on ? Color.FromRgb(0, 192, 0) : Colors.Black,
            Red = on ? Color.FromRgb(255, 68, 68) : Colors.Black,
            Yellow = on ? Color.FromRgb(240, 160, 48) : Colors.Black
        };
    }
}

/// <summary>
/// 模式 12: 三色轮转呼吸灯
/// </summary>
public class BreathingEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs)
    {
        const double cycle = 4092.0;
        double t = (elapsedMs % cycle) / cycle;
        double val = t < 0.5 ? t * 2 : 2 - (t * 2);
        byte g = (byte)(val * 255);
        byte r = (byte)((1 - val) * 255);

        double t2 = ((elapsedMs + 1364) % 4092) / 4092.0;
        double val2 = t2 < 0.5 ? t2 * 2 : 2 - (t2 * 2);
        byte y = (byte)(val2 * 255);

        return new LedColor
        {
            Green = Color.FromRgb(0, g, 0),
            Red = Color.FromRgb(r, 0, 0),
            Yellow = Color.FromRgb(y, (byte)(y * 0.67), 0)
        };
    }
}

/// <summary>
/// 模式 13: 萤火虫混沌呼吸，三灯独立正弦波
/// </summary>
public class FireflyEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs)
    {
        double valGreen = (Math.Sin((elapsedMs % 3000) * 2.0 * Math.PI / 3000.0) + 1.0) * 0.5;
        double valRed = (Math.Sin((elapsedMs % 2200) * 2.0 * Math.PI / 2200.0) + 1.0) * 0.5;
        double valYellow = (Math.Sin((elapsedMs % 2600) * 2.0 * Math.PI / 2600.0) + 1.0) * 0.5;

        return new LedColor
        {
            Green = Color.FromRgb(0, (byte)(valGreen * 255), 0),
            Red = Color.FromRgb((byte)(valRed * 255), 0, 0),
            Yellow = Color.FromRgb((byte)(valYellow * 240), (byte)(valYellow * 160), (byte)(valYellow * 48))
        };
    }
}

/// <summary>
/// 模式 14: 心电波模拟
/// </summary>
public class EcgEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>
    public LedColor Update(long elapsedMs)
    {
        long pos = elapsedMs % 1200;
        double rVal, gVal;

        if (pos < 100)
        {
            rVal = pos / 100.0 * 0.15;
        }
        else if (pos < 200)
        {
            rVal = 0;
        }
        else if (pos < 280)
        {
            rVal = 1.0;
        }
        else if (pos < 450)
        {
            rVal = 0;
        }
        else if (pos < 700)
        {
            double t = (pos - 450) / 250.0; rVal = Math.Sin(t * Math.PI) * 0.3;
        }
        else
        {
            rVal = 0;
        }

        gVal = (pos >= 200 && pos < 280) ? 1.0 : 0;

        double yVal;
        if (pos < 100)
        {
            yVal = pos / 100.0 * 0.4;
        }
        else if (pos < 200)
        {
            yVal = 0;
        }
        else if (pos < 280)
        {
            yVal = 0.6;
        }
        else if (pos < 450)
        {
            yVal = 0;
        }
        else if (pos < 700)
        {
            double yt = (pos - 450) / 250.0; yVal = Math.Sin(yt * Math.PI) * 0.5;
        }
        else
        {
            yVal = 0;
        }

        return new LedColor
        {
            Green = Color.FromRgb(0, (byte)(gVal * 255), 0),
            Red = Color.FromRgb((byte)(rVal * 255), 0, 0),
            Yellow = Color.FromRgb((byte)(yVal * 240), (byte)(yVal * 160), (byte)(yVal * 48))
        };
    }
}

/// <summary>
/// 模式 15: 三相正弦追逐
/// </summary>
public class PhaseChaseEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>
    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>
    public LedColor Update(long elapsedMs)
    {
        double t = (elapsedMs % 3000) * 2.0 * Math.PI / 3000.0;
        double valGreen = (Math.Sin(t) + 1.0) * 0.5;
        double valRed = (Math.Cos(t) + 1.0) * 0.5;
        double valYellow = (Math.Sin(t + (2.0 * Math.PI / 3.0)) + 1.0) * 0.5;

        return new LedColor
        {
            Green = Color.FromRgb(0, (byte)(valGreen * 255), 0),
            Red = Color.FromRgb((byte)(valRed * 255), 0, 0),
            Yellow = Color.FromRgb((byte)(valYellow * 240), (byte)(valYellow * 160), (byte)(valYellow * 48))
        };
    }
}

/// <summary>
/// 模式 16: 急救爆闪追击，绿→黄→红各闪3下
/// </summary>
public class StrobeChaseEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>
    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>
    public LedColor Update(long elapsedMs)
    {
        long pos = elapsedMs % 1680;
        bool greenPhase = pos < 480;
        bool yellowPhase = pos >= 480 && pos < 960;
        bool redPhase = pos >= 960 && pos < 1440;

        if (greenPhase)
        {
            bool on = (int)(pos % 160) < 80;
            return new LedColor
            {
                Green = on ? Color.FromRgb(0, 192, 0) : Colors.Black,
                Red = Colors.Black,
                Yellow = Colors.Black
            };
        }
        else if (yellowPhase)
        {
            bool on = (int)((pos - 480) % 160) < 80;
            return new LedColor
            {
                Green = Colors.Black,
                Red = Colors.Black,
                Yellow = on ? Color.FromRgb(240, 160, 48) : Colors.Black
            };
        }
        else if (redPhase)
        {
            bool on = (int)((pos - 960) % 160) < 80;
            return new LedColor
            {
                Green = Colors.Black,
                Red = on ? Color.FromRgb(255, 68, 68) : Colors.Black,
                Yellow = Colors.Black
            };
        }
        return new();
    }
}

/// <summary>
/// 模式 17: 太极呼吸，红绿对立消长，黄灯独立
/// </summary>
public class TaichiEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>

    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>

    public LedColor Update(long elapsedMs)
    {
        double t = (elapsedMs % 3000) * 2.0 * Math.PI / 3000.0;
        double s = Math.Sin(t);
        double s3 = s * s * s;
        double valGreen = (s3 + 1.0) * 0.5;
        double valRed = (1.0 - s3) * 0.5;
        // 黄灯独立 120° 相位偏移
        double tYellow = ((elapsedMs + 1000) % 3000) * 2.0 * Math.PI / 3000.0;
        double valYellow = (Math.Sin(tYellow) + 1.0) * 0.5;

        return new LedColor
        {
            Green = Color.FromRgb(0, (byte)(valGreen * 255), 0),
            Red = Color.FromRgb((byte)(valRed * 255), 0, 0),
            Yellow = Color.FromRgb((byte)(valYellow * 240), (byte)(valYellow * 160), (byte)(valYellow * 48))
        };
    }
}

/// <summary>
/// 模式 18: HELLO 摩尔斯码
/// </summary>
public class HelloMorseEffect : ILedEffect
{
    private static readonly int[] Pattern = BuildHelloPattern();

    private static int[] BuildHelloPattern()
    {
        string[] codes = ["....", ".", ".-..", ".-..", "---"];
        List<int> p = new List<int>();
        for (int i = 0; i < codes.Length; i++)
        {
            string code = codes[i];
            for (int j = 0; j < code.Length; j++)
            {
                p.Add(code[j] == '.' ? 200 : 600);
                p.Add(200);
            }
            p.RemoveAt(p.Count - 1);
            p.Add(i < codes.Length - 1 ? 600 : 1400);
        }
        return [.. p];
    }

    /// <summary>
    /// 重置灯效状态
    /// </summary>
    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>
    public LedColor Update(long elapsedMs)
    {
        long cycleMs = 0;
        foreach (int p in Pattern) cycleMs += p;
        long pos = elapsedMs % cycleMs;
        int step = 0;
        long accumulated = 0;
        for (int i = 0; i < Pattern.Length; i++)
        {
            accumulated += Pattern[i];
            if (pos < accumulated) { step = i; break; }
        }
        bool on = step % 2 == 0;
        return new LedColor
        {
            Green = on ? Color.FromRgb(0, 192, 0) : Colors.Black,
            Red = on ? Color.FromRgb(255, 68, 68) : Colors.Black,
            Yellow = on ? Color.FromRgb(240, 160, 48) : Colors.Black
        };
    }
}

/// <summary>
/// 模式 19: 雷达扫描与锁定
/// </summary>
public class RadarEffect : ILedEffect
{
    /// <summary>
    /// 重置灯效状态
    /// </summary>
    public void Reset()
    { }

    /// <summary>
    /// 计算当前帧颜色输出
    /// </summary>
    /// <param name="elapsedMs">从灯效启动至今的毫秒数</param>
    public LedColor Update(long elapsedMs)
    {
        long pos = elapsedMs % 4500;

        if (pos < 3000)
        {
            double t = pos * 2.0 * Math.PI / 3000.0;
            double val = (200.0 / 1023.0) + ((823.0 / 1023.0) * (Math.Sin(t) + 1.0) * 0.5);
            double ping = (Math.Sin(t) + 1.0) * 0.5;
            double yVal = ping > 0.85 ? (ping - 0.85) / 0.15 : 0;
            return new LedColor
            {
                Green = Color.FromRgb(0, (byte)(val * 255), 0),
                Red = Color.FromRgb(30, 0, 0),
                Yellow = Color.FromRgb((byte)(yVal * 240), (byte)(yVal * 160), (byte)(yVal * 48))
            };
        }
        else if (pos < 4000)
        {
            bool flash = ((int)(pos - 3000) / 250) % 2 == 0;
            return new LedColor
            {
                Green = Color.FromRgb(0, 255, 0),
                Red = flash ? Color.FromRgb(255, 68, 68) : Colors.Black,
                Yellow = flash ? Colors.Black : Color.FromRgb(240, 160, 48)
            };
        }
        else
        {
            return new LedColor
            {
                Green = Color.FromRgb(0, 255, 0),
                Red = Color.FromRgb(255, 68, 68),
                Yellow = Color.FromRgb(240, 160, 48)
            };
        }
    }
}