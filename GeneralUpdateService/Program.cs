using GeneralUpdateService.Classes;
using GeneralUpdateService.Utils;
using System.Diagnostics;

namespace GeneralUpdateService
{
    public static class Program
    {
        private static string? PackPath = null;
        private static string? ExtractDirectory = null;
        private static string? StartProcess = null;
        private static bool SelfUpdate = false;


        static async Task Main(string[] args)
        {
            try
            {
                //参数
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--pack-path":
                            if (args.Length > i + 1)
                                PackPath = args[i + 1];
                            break;
                        case "--extract-dir":
                            if (args.Length > i + 1)
                                ExtractDirectory = args[i + 1];
                            break;
                        case "--start-process":
                            if (args.Length > i + 1)
                                StartProcess = args[i + 1];
                            break;
                        case "--self-update":
                            SelfUpdate = true;
                            break;
                    }
                }
                //输出头信息
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(Globals.HeadInfo);
                Console.WriteLine($"""
                    Version: {Globals.Version}
                    Pack-path: {PackPath}
                    Extract-directory: {ExtractDirectory}
                    {new string('=', 26)}
                    """);
                Console.ResetColor();

                if (string.IsNullOrEmpty(PackPath) || string.IsNullOrEmpty(ExtractDirectory))
                    throw new Exception("参数 \"--pack-path\" 与参数 \"--extract-dir\" 不可为空！");

                //文件检测
                if (!File.Exists(PackPath))
                    throw new FileNotFoundException("更新包文件不存在", PackPath);
                if (!Directory.Exists(ExtractDirectory))
                    throw new DirectoryNotFoundException("输出目录不存在");

                Console.WriteLine("开始解压文件...");
                var progress = new Progress<double>(p => Console.WriteLine($"解压进度: {Math.Round(p)}%"));
                await ZipHelper.ExtractAsync(PackPath, ExtractDirectory, progress);
                Console.WriteLine("解压完毕");

                Console.WriteLine("将于3秒后启动主程序...");

                for (int i = 0; i < 3; i++)
                {
                    Console.WriteLine($"{i + 1}...");
                    await Task.Delay(1000);
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = StartProcess,
                    UseShellExecute = true
                });
            }
            catch(OperationCanceledException)
            {
                Console.Write("用户取消更新，输入任意键退出...");
                Console.Write("按任意键退出...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, "发生错误，程序被迫终止");
                Console.Write("按任意键退出...");
                Console.ReadKey();
            }
        }
    }
}
