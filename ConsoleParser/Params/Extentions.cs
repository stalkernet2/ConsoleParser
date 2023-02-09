using System.Text.Json;

namespace ConsoleParser
{
    public static class Extentions
    {
        public static async void AsyncSave(this Parameters presets, string filePath)
        {
            await using var fileStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fileStream, presets);
            fileStream.Close();
        }
    }
}
