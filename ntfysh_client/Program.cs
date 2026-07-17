using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;
using ntfysh_client.Notifications;

namespace ntfysh_client
{
    static class Program
    {
        private static readonly NotificationListener NotificationListener = new();
        public static SettingsModel Settings { get; set; } = null!;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            args = args.Select(a => a.ToLower()).ToArray();

            if (args.Contains("-h") || args.Contains("--help"))
            {
                MessageBox.Show("帮助：\n    -h\n    --help\n\n最小化到托盘启动：\n    -t\n    --start-in-tray\n\n注册开机自启动：\n    --install-autostart\n\n移除开机自启动：\n    --remove-autostart\n\n允许多个实例：\n    -m\n    --allow-multiple-instances", "帮助菜单", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (args.Contains("--install-autostart"))
            {
                SetAutoStart(true);
                return;
            }

            if (args.Contains("--remove-autostart"))
            {
                SetAutoStart(false);
                return;
            }

            bool startInTray = args.Contains("-t") || args.Contains("--start-in-tray");
            bool allowMultipleInstances = args.Contains("-m") || args.Contains("--allow-multiple-instances");

            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()!.Location)).Length > 1)
            {
                if (!allowMultipleInstances)
                {
                    MessageBox.Show("另一个实例已在运行。\n\n如果您希望启动第二个重复实例，请使用 -m 或 --allow-multiple-instances 参数。\n\n此实例即将关闭。", "多实例检测", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(NotificationListener, startInTray));
        }

        internal static void SetAutoStart(bool enable)
        {
            const string key = @"Software\Microsoft\Windows\CurrentVersion\Run";
            const string valueName = "ntfy.sh Windows";

            using RegistryKey? runKey = Registry.CurrentUser.OpenSubKey(key, writable: true);
            if (runKey is null) return;

            if (enable)
            {
                string? exePath = Assembly.GetEntryAssembly()?.Location;
                if (exePath is not null)
                {
                    // Assembly.GetEntryAssembly().Location returns the DLL path in .NET.
                    // We need the .exe path for autostart registration.
                    if (exePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        exePath = Path.ChangeExtension(exePath, ".exe");
                    }
                    runKey.SetValue(valueName, $"\"{exePath}\" -t");
                }
            }
            else
            {
                if (runKey.GetValue(valueName) is not null)
                {
                    runKey.DeleteValue(valueName);
                }
            }
        }
    }
}
