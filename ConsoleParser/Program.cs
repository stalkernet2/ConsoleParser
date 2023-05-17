using System.Runtime.InteropServices;
using ConsoleParser.Parse;

namespace ConsoleParser
{
    public class Program
    {
        public static string Name = "ConsoleParser";

        static void Main(string[] args)
        {
            Console.Title = Name;
            //DebugMoment();

            Logger.Init(false);
            Logger.LogNewLine("Инициализация логгера успешна!");

            Logger.LogNewLine("Версия платформы: " + RuntimeInformation.FrameworkDescription);
            Logger.LogNewLine("Версия приложения: " + typeof(Program).Assembly.GetName().Version);

            Parameters parameters;
            Logger.LogNewLine("Загрузка параметров...");
            try
            {
                parameters = new Parameters("config/presets.json");
            }
            catch(FileNotFoundException ex)
            {
                Logger.LogNewLine("...неудачна!", LogEnum.Error);
                Logger.LogNewLine($"Файла с параметрами не существует. Создаем...");
                File.Create("config/presets.json").Close();
                Parameters.SaveTemplate();
                return;
            }
            catch (Exception ex)
            {
                Logger.LogNewLine("...неудачна!", LogEnum.Error);
                Logger.LogNewLine($"Причина неудачной загрузки: {ex.Message}");
                return;
            }
            Logger.LogNewLine("...успешна!");

            Logger.LogNewLine("StartPage:      " + parameters.StartPage);
            Logger.LogNewLine("EndPage:        " + parameters.EndPage);
            Logger.LogNewLine("URL:            " + parameters.URL);
            Logger.LogNewLine("Ozon:           " + parameters.Ozon);
            Logger.LogNewLine("VseInstrumenti: " + parameters.VseInstrumenti);
            Logger.LogNewLine("Yandex:         " + parameters.Yandex);
            Logger.LogNewLine("DivClass:       " + parameters.DivClass);
            Logger.LogNewLine("AClass:         " + parameters.AClass);
            Logger.LogNewLine("SecretJSON:     " + parameters.SecretJson);
            Logger.LogNewLine("APIName:        " + parameters.APIName);
            Logger.LogNewLine("SpreadsheetId:  " + parameters.SpreadsheetId);

            if (parameters.Ozon)
            {
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

                    Logger.OnLine();
                }
            }

            if (parameters.EndPage < parameters.StartPage)
            {
                Logger.LogNewLine("Начальная страница больше конечной!", LogEnum.Error);
                return;
            }

            if (parameters.URL == "")
            {
                Logger.LogNewLine("URL-ссылка на каталог не указана!", LogEnum.Error);
                return;
            }

            Logger.LogNewLine("Какой режим работы установить для парсера:" +
                              "\n1. Стандартный запуск - пермаментный запуск парсера " +
                              "\n2. Запуск по времени - запускается в установленное время(при выборе этого пункта будет установлено далее), либо, в случае не введенного времени, в дефолтное время(01:00:00(час ночи)) " +
                              "\n3. Дебаг момент - сохранение параметров", LogEnum.Action);

            var tempBool = true;

            while (tempBool)
            {
                var pressedKey = Console.ReadKey().Key;

                switch (pressedKey)
                {
                    case ConsoleKey.D1:
                        tempBool = false;
                        Logger.LogNewLine("Запуск парсера.");
                        new Thread(() => Parser.Start(parameters)).Start();
                        break;
                    case ConsoleKey.D2:
                        tempBool = false;
                        Logger.LogNewLine("Запуск таймера.");
                        //TimerP.Init(parameters, new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second + 10), new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second + 20)); // debug
                        TimerP.Init(parameters, new TimeSpan(1, 0, 0), new TimeSpan(6, 0, 0));
                        break;
                    case ConsoleKey.D3:
                        tempBool = false;
                        Parameters.SaveTemplate();
                        break;
                    case ConsoleKey.NumPad1:
                        goto case ConsoleKey.D1;
                    case ConsoleKey.NumPad2:
                        goto case ConsoleKey.D2;
                    case ConsoleKey.NumPad3:
                        goto case ConsoleKey.D3;
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                }
                // Logger.OnLine();
            }

            Logger.LogNewLine("Нажмите любую клавишу(кроме кнопки выключения компьютера), чтобы закрыть это окно");
            Console.ReadKey();
        }

        private static void DebugMoment()
        {
            var ya = new YandexDriver("");
            var we = ya.GetValidURL("asd", "", Array.Empty<string>(), "Я.Маркете ТЕСТ");
            Console.ReadKey();
        }
    }
}