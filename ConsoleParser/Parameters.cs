using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleParser
{
    public struct Parameters
    {
        public int StartPage { get; set; }

        public int EndPage { get; set; }

        public string URL { get; set; }

        public bool Ozon { get; set; }

        public bool VseInstrumenti { get; set; }

        public bool Yandex { get; set; }

        public string DivClass { get; set; }

        public string AClass { get; set; }

        public string SecretJson { get; set; }

        public string APIName { get; set; }

        public string SpreadsheetId { get; set; }

        public Parameters()
        {
            StartPage = 0;
            EndPage = 0;
            URL = "https://ot-i-do.ru/catalog/svarochnoe-oborudovanie/";
            Ozon = false;
            VseInstrumenti = false;
            Yandex = false;
            DivClass = "k1p kp2";
            AClass = "dt9";
            SecretJson = "config/client_secrets.json";
            APIName = "GoogleSheetsAPI";
            SpreadsheetId = "1wCKGnnt1UMo473a2dMhz81qOVnuhaZHPJBJ0aCTg5mI";
        }

        public Parameters(string filePath)
        {
            var presets = Read(filePath);

            StartPage = presets.StartPage;
            EndPage = presets.EndPage;
            URL = presets.URL;
            Ozon = presets.Ozon;
            VseInstrumenti = presets.VseInstrumenti;
            Yandex = presets.Yandex;
            DivClass = presets.DivClass;
            AClass = presets.AClass;
            SecretJson = presets.SecretJson;
            APIName = presets.APIName;
            SpreadsheetId = presets.SpreadsheetId;

            Logger.LogNewLine($"Параметры из {filePath} успешно загружены!");
        }

        public static Parameters Read(string filePath)
        {
            Logger.LogNewLine($"Загрузка параметров из {filePath}...");

            if (!File.Exists(filePath))
            {
                Logger.LogNewLine("Параметры небыли загружены! Файла с параметрами не существует.", LogEnum.Error);
                return new Parameters();
            }

            var jsonString = File.ReadAllText(filePath);
            var presets = JsonSerializer.Deserialize<Parameters>(jsonString);

            Logger.LogNewLine("Инициализация параметров...");
            return presets;
        }

        public static void DebugSave()
        {
            var we = new Parameters();
            we.AsyncSave("config/presets.json");
        }
    }

    public static class External
    {
        public static async void AsyncSave(this Parameters presets, string filePath)
        {
            Logger.LogNewLine($"Сохранение параметров {filePath}...");
            try
            {
                await using FileStream fileStream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(fileStream, presets);
                Logger.LogNewLine($"Параметры в {filePath} успешно сохранен!");
            }
            catch (Exception ex)
            {
                Logger.LogNewLine(ex.ToString(), LogEnum.Error);
            }
        }
    }
}
