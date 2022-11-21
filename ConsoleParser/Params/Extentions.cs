using System.Text.Json;

namespace ConsoleParser
{
    public static class Extentions
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
