using System.Net;
using HtmlAgilityPack;

var baseUrl = "http://localhost:5179"; // Updated to match your app's port
var outputPath = Path.Combine("..", "SCRANKRealEstate", "wwwroot-static");

// Routes to crawl
var routes = new[]
{
    "/",
    "/Home/Privacy"
};

Console.WriteLine("Starting static site generation...");
Console.WriteLine($"Make sure your app is running at {baseUrl}");
Console.WriteLine();

// Create output directory
if (Directory.Exists(outputPath))
    Directory.Delete(outputPath, true);
Directory.CreateDirectory(outputPath);

using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };

foreach (var route in routes)
{
    try
    {
        Console.WriteLine($"Downloading {route}...");
        var html = await client.GetStringAsync(route);
        
        // Determine output file path
        var fileName = route == "/" ? "index.html" : $"{route.Trim('/').Replace("/", "-")}.html";
        var filePath = Path.Combine(outputPath, fileName);
        
        await File.WriteAllTextAsync(filePath, html);
        Console.WriteLine($"  ✓ Saved to {fileName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✗ Failed: {ex.Message}");
    }
}

// Copy wwwroot folder (css, js, images, etc.)
var wwwrootSource = Path.Combine("..", "SCRANKRealEstate", "wwwroot");
if (Directory.Exists(wwwrootSource))
{
    Console.WriteLine("\nCopying static assets...");
    CopyDirectory(wwwrootSource, outputPath);
    Console.WriteLine("  ✓ Static assets copied");
}

Console.WriteLine("\n✓ Static site generation complete!");
Console.WriteLine($"Output directory: {Path.GetFullPath(outputPath)}");

static void CopyDirectory(string sourceDir, string destDir)
{
    foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
    {
        var relativePath = Path.GetRelativePath(sourceDir, file);
        var destFile = Path.Combine(destDir, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
        File.Copy(file, destFile, true);
    }
}
