using System.Net;
using HtmlAgilityPack;

var baseUrl = "http://localhost:5179";
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
        
        // Fix absolute paths to relative paths
        html = FixPaths(html);
        
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

static string FixPaths(string html)
{
    var doc = new HtmlDocument();
    doc.LoadHtml(html);
    
    // Fix <link> tags (CSS)
    foreach (var link in doc.DocumentNode.SelectNodes("//link[@href]") ?? Enumerable.Empty<HtmlNode>())
    {
        var href = link.GetAttributeValue("href", "");
        if (href.StartsWith("/") && !href.StartsWith("//"))
        {
            link.SetAttributeValue("href", href.TrimStart('/'));
        }
    }
    
    // Fix <script> tags (JavaScript)
    foreach (var script in doc.DocumentNode.SelectNodes("//script[@src]") ?? Enumerable.Empty<HtmlNode>())
    {
        var src = script.GetAttributeValue("src", "");
        if (src.StartsWith("/") && !src.StartsWith("//"))
        {
            script.SetAttributeValue("src", src.TrimStart('/'));
        }
    }
    
    // Fix <a> tags (links)
    foreach (var anchor in doc.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>())
    {
        var href = anchor.GetAttributeValue("href", "");
        if (href.StartsWith("/") && !href.StartsWith("//") && !href.StartsWith("#"))
        {
            // Convert /Home/Privacy to Home-Privacy.html
            if (href == "/")
            {
                anchor.SetAttributeValue("href", "index.html");
            }
            else
            {
                var pageName = href.Trim('/').Replace("/", "-") + ".html";
                anchor.SetAttributeValue("href", pageName);
            }
        }
    }
    
    // Fix <img> tags
    foreach (var img in doc.DocumentNode.SelectNodes("//img[@src]") ?? Enumerable.Empty<HtmlNode>())
    {
        var src = img.GetAttributeValue("src", "");
        if (src.StartsWith("/") && !src.StartsWith("//"))
        {
            img.SetAttributeValue("src", src.TrimStart('/'));
        }
    }
    
    return doc.DocumentNode.OuterHtml;
}

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
