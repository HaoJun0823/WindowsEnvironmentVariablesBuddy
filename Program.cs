using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices; // 用于广播消息
using System.Text;
using Microsoft.Win32; // 必须引用此命名空间

namespace WindowsEnvironmentVariablesBuddy
{
    internal class Program
    {
        static FileInfo txt;

        // 用于通知系统环境变量更新的 Win32 API
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out UIntPtr lpdwResult);

        static void Main(string[] args)
        {
            Console.WriteLine("Windows Environment Variables Buddy BY Randerion(HaoJun0823)");
            Console.WriteLine("HomePage: https://www.haojun0823.xyz/ Github: https://github.com/HaoJun0823/WindowsEnvironmentVariablesBuddy");
            Console.WriteLine("==============================================");

            txt = new FileInfo("WindowsEnvironmentVariablesBuddy.txt");
            if (CheckFile())
            {
                List<string> lines = ReadLines();

                if (lines.Count > 0)
                {
                    // 1. 更新或创建 BuddyVariables 变量本身
                    BuildBuddyVariables(lines);

                    // 2. 将 %BuddyVariables% 注入到系统的 Path 中
                    InjectBuddyToPath();

                    // 3. 通知系统刷新
                    RefreshSystemEnvironment();

                    Console.WriteLine("==============================================");
                    Console.WriteLine("Done! Please restart your terminal (CMD/PowerShell) to apply changes.");
                }
                else
                {
                    Console.WriteLine("The file is empty. Please fill in the paths (one line each) in {0}.", txt.FullName);
                }
            }
            Console.ReadKey();
        }

        static bool CheckFile()
        {
            if (txt.Exists) return true;

            txt.CreateText().Close();
            Console.WriteLine("Oh, you need to fill in the paths (one line each) in {0}.", txt.FullName);
            return false;
        }

        static List<string> ReadLines()
        {
            List<string> result = new List<string>();
            foreach (string line in File.ReadAllLines(txt.FullName))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string trimmed = line.Trim();
                    result.Add(trimmed);
                    Console.WriteLine("Read path: {0}", trimmed);
                }
            }
            return result;
        }

        static void BuildBuddyVariables(List<string> lines)
        {
            // 使用分号连接所有路径
            string newValue = string.Join(";", lines);

            // 1. 备份旧值
            // 这里可以使用 Environment.GetEnvironmentVariable，因为备份通常希望看到解析后的结果
            // 或者使用注册表获取原始值进行备份
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Environment", true))
            {
                if (key == null) return;

                string oldValue = (string)key.GetValue("BuddyVariables", "", RegistryValueOptions.DoNotExpandEnvironmentNames);

                if (!string.IsNullOrEmpty(oldValue))
                {
                    File.WriteAllText("WindowsEnvironmentVariablesBuddy_OldValue.txt", oldValue);
                    Console.WriteLine("Backup created: WindowsEnvironmentVariablesBuddy_OldValue.txt");
                }

                // 2. 写入 BuddyVariables
                // 使用 ExpandString 是为了万一用户在 txt 里输入了 %UserProfile% 这种路径也能被解析
                key.SetValue("BuddyVariables", newValue, RegistryValueKind.ExpandString);
                Console.WriteLine("Success: BuddyVariables set to -> {0}", newValue);
            }
        }

        /// <summary>
        /// 核心补充：将 %BuddyVariables% 安全地加入 Path
        /// </summary>
        static void InjectBuddyToPath()
        {
            const string target = "%BuddyVariables%";

            // 直接操作注册表以避免“路径展开”问题
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Environment", true))
            {
                if (key == null) return;

                // 获取原始 Path (DoNotExpandEnvironmentNames 是关键)
                string rawPath = (string)key.GetValue("Path", "", RegistryValueOptions.DoNotExpandEnvironmentNames);

                // 检查是否已经包含该变量
                if (rawPath.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Console.WriteLine("Check: {0} is already in your Path.", target);
                    return;
                }

                // 拼接新路径
                string separator = rawPath.Length > 0 && !rawPath.TrimEnd().EndsWith(";") ? ";" : "";
                string newPath = rawPath + separator + target;

                // 以 ExpandString 格式存回注册表
                key.SetValue("Path", newPath, RegistryValueKind.ExpandString);
                Console.WriteLine("Success: {0} injected into Path.", target);
            }
        }

        /// <summary>
        /// 核心补充：广播消息刷新系统环境
        /// </summary>
        static void RefreshSystemEnvironment()
        {
            Console.WriteLine("Refreshing Windows environment...");
            const uint WM_SETTINGCHANGE = 0x1a;
            SendMessageTimeout((IntPtr)0xffff, WM_SETTINGCHANGE, UIntPtr.Zero, "Environment", 0x02, 1000, out _);
        }
    }
}