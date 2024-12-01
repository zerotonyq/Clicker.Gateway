using Newtonsoft.Json.Linq;

namespace ClickerGateway;

public class ApiClassGenerator
{
    public async Task GenerateApiClass(List<string> swaggerUrls)
    {
         // URL для swagger.json
        string outputPath = "C:\\UnityProjects\\UniversityClickerClient\\Assets\\Scripts\\ApiPaths.cs"; // Файл для сгенерированного класса

        string baseUrl = "http://localhost:5000";
        
        try
        {
            // Загружаем swagger.json из URL
            Console.WriteLine("Downloading swagger.json...");
            List<string> swaggerContents = new();

            foreach (var swaggerUrl in swaggerUrls) swaggerContents.Add(await DownloadSwaggerJson(swaggerUrl));
            
            List<string> paths = [];
           
            foreach (var swaggerJson in swaggerContents.Select(JToken.Parse))
            {
                // Извлекаем пути
                paths.AddRange(  swaggerJson.Value<JObject>("paths").Properties().Select(ch => ch.Name));
            }
            
            if (paths.Count == 0)
            {
                Console.WriteLine("No paths found in swagger.json.");
                return;
            }
            
            // Генерация C# класса
            var classBuilder = new List<string>();
            classBuilder.Add("public static class ApiPaths");
            classBuilder.Add("{");

            foreach (var path in paths)
            {
                string route = path;
                string constantName = GetConstantName(route);

                classBuilder.Add($"    public const string {constantName} = \"{baseUrl}{route}\";");
            }

            classBuilder.Add("}");

            // Запись в файл
            await File.WriteAllLinesAsync(outputPath, classBuilder);

            Console.WriteLine($"Class generated: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // Скачивает swagger.json по URL
    static async Task<string> DownloadSwaggerJson(string url)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to download swagger.json: {response.StatusCode}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    // Преобразует путь в название константы
    static string GetConstantName(string route)
    {
        // Удаляем слэши, заменяем на подчеркивания, убираем фигурные скобки
        string name = route.Replace("/", "_")
                           .Replace("{", "")
                           .Replace("}", "")
                           .Replace("-", "_")
                           .Replace(".", "_")
                           .Trim('_');

        return name.ToUpper(); // Константы обычно пишут в верхнем регистре
    }
}