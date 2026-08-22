using System.Runtime.InteropServices;
using WinForms = System.Windows.Forms;

namespace MagicCenterHub.Services;

/// <summary>
/// 根据 Windows 显示设备名称查找对应屏幕。
/// </summary>
internal static class DisplayLocator
{
    private const uint DisplayDeviceAttachedToDesktop = 0x00000001;

    public static WinForms.Screen? FindScreen(string displayName)
    {
        for (uint adapterIndex = 0; ; adapterIndex++)
        {
            DisplayDevice adapter = DisplayDevice.Create();
            if (!EnumDisplayDevices(null, adapterIndex, ref adapter, 0))
                break;

            if ((adapter.StateFlags & DisplayDeviceAttachedToDesktop) == 0)
                continue;

            if (ContainsName(adapter.DeviceString, displayName) ||
                HasMatchingMonitor(adapter.DeviceName, displayName))
            {
                return WinForms.Screen.AllScreens.FirstOrDefault(screen =>
                    string.Equals(screen.DeviceName, adapter.DeviceName, StringComparison.OrdinalIgnoreCase));
            }
        }

        return null;
    }

    private static bool HasMatchingMonitor(string adapterName, string displayName)
    {
        for (uint monitorIndex = 0; ; monitorIndex++)
        {
            DisplayDevice monitor = DisplayDevice.Create();
            if (!EnumDisplayDevices(adapterName, monitorIndex, ref monitor, 0))
                return false;

            if (ContainsName(monitor.DeviceString, displayName))
                return true;
        }
    }

    private static bool ContainsName(string value, string displayName)
    {
        return value.Contains(displayName, StringComparison.OrdinalIgnoreCase);
    }

    [DllImport("user32.dll", EntryPoint = "EnumDisplayDevicesW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayDevices(
        string? deviceName,
        uint deviceIndex,
        ref DisplayDevice displayDevice,
        uint flags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DisplayDevice
    {
        public uint Size;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;

        public uint StateFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;

        public static DisplayDevice Create()
        {
            return new DisplayDevice
            {
                Size = (uint)Marshal.SizeOf<DisplayDevice>(),
                DeviceName = string.Empty,
                DeviceString = string.Empty,
                DeviceId = string.Empty,
                DeviceKey = string.Empty
            };
        }
    }
}