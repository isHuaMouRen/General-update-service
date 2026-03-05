using System;
using System.Collections.Generic;
using System.Text;

namespace GeneralUpdateService.Utils
{
    public static class ErrorReporter
    {
        public static void Report(Exception ex,string message)
        {
            var lastColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"发生错误: {message} ({ex.Message})\n    {ex}");
            Console.ForegroundColor = lastColor;
        }
    }
}
