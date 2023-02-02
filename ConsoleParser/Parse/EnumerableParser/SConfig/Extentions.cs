using System.Text.Json;

namespace ConsoleParser.Parse.EnumerableParser.SConfig
{
    public static class Extentions
    {
        public static async void AsyncSave(this SearchConfig sConfig, string filePath)
        {
            Logger.LogNewLine($"Сохранение конфига {filePath}...");
            try
            {
                await using FileStream fileStream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(fileStream, sConfig);
                Logger.LogNewLine($"Поисковой конфиг успешно сохранен в {filePath}!");
            }
            catch (Exception ex)
            {
                Logger.LogNewLine(ex.ToString(), LogEnum.Error);
            }
        }
    }
}
