using MagicCenterHub.Models;
using Newtonsoft.Json;
using System.IO;

namespace MagicCenterHub.Services;

/// <summary>
/// 配置持久化服务，JSON 文件存储在 %AppData%\MagicCenterHub\settings.json
/// </summary>
public static class SettingsService
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MagicCenterHub");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    /// <summary>
    /// 加载配置，文件不存在或解析失败时返回默认值
    /// </summary>
    public static Settings Load()
    {
        if (!File.Exists(SettingsPath))
            return new Settings();

        try
        {
            string json = File.ReadAllText(SettingsPath);
            return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
        }
        catch
        {
            return new Settings();
        }
    }

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    public static void Save(Settings settings)
    {
        try
        {
            if (!Directory.Exists(SettingsDir))
                Directory.CreateDirectory(SettingsDir);

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }
        catch // 静默失败，不影响主流程
        {

        }
    }
}