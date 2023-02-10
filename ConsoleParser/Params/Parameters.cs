using System.Text.Json;

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

        public string CaptchaKey { get; set; }

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
            CaptchaKey = "";
        }

        public Parameters(string filePath)
        {
            var presets = GetFromFile(filePath);

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
            CaptchaKey = presets.CaptchaKey;
        }

        public static Parameters GetFromFile(string filePath)
        { 
            var jsonString = File.ReadAllText(filePath);
            var presets = JsonSerializer.Deserialize<Parameters>(jsonString);

            return presets;
        }

        public static void SaveTemplate()
        {
            var we = new Parameters();
            we.AsyncSave("config/presets.json");
        }
    }
}
