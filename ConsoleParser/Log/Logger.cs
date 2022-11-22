using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColoredObj;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleParser
{
    public class Logger
    {
        private static string fileLog = "";
        private static string path = "logs";

        public static void Init()
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            fileLog = "Log" + DateTime.Now.Date.ToString("dd/MM/yyyy").Replace('.', '_');

            var i = 0;
            while (File.Exists($"{path}/" + fileLog + (i == 0 ? "" : $" ({i})") + ".txt"))
                i++;

            fileLog += (i == 0 ? "" : $" ({i})") + ".txt";

            using var fileStream = new FileStream($"{path}/{fileLog}", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public static void LogOnLine(string text, LogEnum @enum = LogEnum.Info)
        {
            LogNewLine(text, @enum);

            OnLine();
        }

        public static void LogNewLine(string text, LogEnum @enum = LogEnum.Info)
        {
            WriteTextInLogFile($"[{DateTime.Now:dd.MM.yyyy || HH:mm:ss.ffffff}]: {@enum} " + text.Replace('│', '|').Replace('├', '|').Replace('└', '\\').Replace('┌', '/').Replace('─', '-')); // da

            switch (@enum)
            {
                case LogEnum.Info:
                    Info(text);
                    break;
                case LogEnum.Warning:
                    Warning(text);
                    break;
                case LogEnum.Error:
                    Error(text);
                    break;
                case LogEnum.Action:
                    Action(text);
                    break;
                default:
                    break;
            }
        }

        public static void OnLine()
        {
            (int left, int top) = Console.GetCursorPosition();
            Console.SetCursorPosition(left, top - 1);
        }

        private static void Info(string text) => ColoredText(ConsoleColor.Gray, text);

        private static void Error(string text) => ColoredText(ConsoleColor.Red, text);

        private static void Warning(string text) => ColoredText(ConsoleColor.DarkYellow, text);

        private static void Action(string text) => ColoredText(ConsoleColor.Cyan, text);

        private static void ColoredText(ConsoleColor color, string text)
        {
            Console.Write($"[{DateTime.Now:dd.MM.yyyy ║ HH:mm:ss.ffffff}]: ");
            text += "\n";
            text.SetColor(color);
        }

        private static void WriteTextInLogFile(string value)
        {
            var sw = File.AppendText($"{path}/{fileLog}");
            sw.WriteLine(value);
            sw.Close();
        }
    }
}
