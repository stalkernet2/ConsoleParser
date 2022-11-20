using ConsoleParser.Parse;
using System.Runtime.InteropServices;

namespace ConsoleParser
{
    public class Program
    {
        static void Main(string[] args)
        {
            Logger.Init();
            Logger.LogNewLine("Инициализация логгера успешна!\n", LogEnum.Info);

            Logger.LogNewLine("Версия .Net: " + RuntimeInformation.FrameworkDescription, LogEnum.Info);
            Logger.LogNewLine("Версия приложения: " + typeof(Program).Assembly.GetName().Version, LogEnum.Info);

            Logger.LogNewLine("Дивы и А классы для озона проверены?(y/n)", LogEnum.Action);

            while (true)
            {
                var pressedKey = Console.ReadKey().Key;

                if (pressedKey == ConsoleKey.Y)
                    break;
                else if (pressedKey == ConsoleKey.N)
                {
                    Logger.LogNewLine("Ты знаешь что делать...(измени их в файле, находящиеся по пути config/presets.json)", LogEnum.Warning);
                    Thread.Sleep(5000);
                    return;
                }
            }

            var parameters = new Parameters("config/presets.json");

            Logger.LogNewLine("StartPage:      " + parameters.StartPage,      LogEnum.Info);
            Logger.LogNewLine("EndPage:        " + parameters.EndPage,        LogEnum.Info);
            Logger.LogNewLine("URL:            " + parameters.URL,            LogEnum.Info);
            Logger.LogNewLine("Ozon:           " + parameters.Ozon,           LogEnum.Info);
            Logger.LogNewLine("VseInstrumenti: " + parameters.VseInstrumenti, LogEnum.Info);
            Logger.LogNewLine("Yandex:         " + parameters.Yandex,         LogEnum.Info);
            Logger.LogNewLine("DivClass:       " + parameters.DivClass,       LogEnum.Info);
            Logger.LogNewLine("AClass:         " + parameters.AClass,         LogEnum.Info);
            Logger.LogNewLine("SecretJSON:     " + parameters.SecretJson,     LogEnum.Info);
            Logger.LogNewLine("APIName:        " + parameters.APIName,        LogEnum.Info);
            Logger.LogNewLine("SpreadsheetId:  " + parameters.SpreadsheetId,  LogEnum.Info);

            Thread.Sleep(5000);

            if (parameters.EndPage < parameters.StartPage)
            {
                Logger.LogNewLine("Начальная страница больше конечной!", LogEnum.Error);
                return;
            }

            if (parameters.URL == "")
            {
                Logger.LogNewLine("URL-ссылка на каталог не указан!", LogEnum.Error);
                return;
            }

            Logger.LogNewLine("Запуск парсера", LogEnum.Info);
            Parser.Start(parameters);

            Logger.LogNewLine("Нажмите любую клавишу(кроме кнопки выключения компьютера), чтобы закрыть это окно", LogEnum.Info);
            Console.ReadKey();
        }
    }
}