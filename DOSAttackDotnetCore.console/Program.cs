using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Press 1 to test DOS with file size");
        Console.WriteLine("Press 2 to test DOS with multiple request");
        var key = Console.ReadKey();
        Console.WriteLine();

        if (key.KeyChar == '1')
        {
            await createLimitFileSizeDOS();
        }
        else if (key.KeyChar == '2')
        {
            await createRateLimiting();
        }
        else
        {
            Console.WriteLine("❌ Invalid input. Press '1' to upload.");
        }
    }

    static async Task createLimitFileSizeDOS()
    {
        Console.Write("📁 Enter full image file path: ");
        string? imagePath = Console.ReadLine()?.Trim('"');

        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            Console.WriteLine("❌ File not found or invalid path.");
            return;
        }

        try
        {
            using var client = new HttpClient();
            using var form = new MultipartFormDataContent();

            var fileBytes = await File.ReadAllBytesAsync(imagePath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

            form.Add(new StringContent("AAA"), "Name");
            form.Add(new StringContent("10"), "Price");
            form.Add(fileContent, "ImageFile", Path.GetFileName(imagePath));

            Console.WriteLine("⏳ Sending request...");

            var response = await client.PostAsync("https://localhost:7149/api/product/createLimitFileSize", form);

            Console.WriteLine($"✅ Status: {response.StatusCode}");
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine("📦 Server Response:\n" + result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error sending request: " + ex.Message);
        }
    }

    static async Task createRateLimiting()
    {
        using var client = new HttpClient();

        Console.Write("📁 Enter full image file path: ");
        string? imagePath = Console.ReadLine()?.Trim('"');

        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            Console.WriteLine("❌ File not found or invalid path.");
            return;
        }

        for (int i = 1; i <= 10; i++)
        {
            using var form = new MultipartFormDataContent();

            byte[] fileBytes = await File.ReadAllBytesAsync(imagePath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

            form.Add(new StringContent("AAA"), "Name");
            form.Add(new StringContent("10"), "Price");
            form.Add(fileContent, "ImageFile", Path.GetFileName(imagePath));

            try
            {
                var response = await client.PostAsync("https://localhost:7149/api/product/createRateLimiting", form);
                Console.WriteLine($"🔹 Request {i}: {(int)response.StatusCode} {response.ReasonPhrase}");

                if ((int)response.StatusCode == 429)
                {
                    string body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("   🚫 Rate limited: " + body);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Request {i} failed: {ex.Message}");
            }
        }
    }
}
