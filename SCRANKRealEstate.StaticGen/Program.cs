using System.Net;
using HtmlAgilityPack;

var baseUrl = "http://localhost:5179";
var outputPath = Path.Combine("..", "SCRANKRealEstate", "wwwroot-static");

// Routes to crawl - ADD YOUR NEW PAGES HERE
var routes = new[]
{
    "/",
    "/Home/Privacy",
    "/Home/About",
    "/Home/Contact",
    "/Home/MortgageCalculator",
    "/Home/FAQ", 
    "/Home/Properties",
    "/Home/PropertyDetails/1",
    "/Home/PropertyDetails/2",
    "/Home/PropertyDetails/3",
    "/Home/PropertyDetails/4",
    "/Home/PropertyDetails/5",
    "/Home/PropertyDetails/6",
    "/Home/PropertyDetails/7",
    "/Home/PropertyDetails/8",
    "/Home/PropertyDetails/9",
    "/Home/PropertyDetails/10",
    "/Home/PropertyDetails/11",
    "/Home/PropertyDetails/12"
};

Console.WriteLine("Starting static site generation...");
Console.WriteLine($"Make sure your app is running at {baseUrl}");
Console.WriteLine();

// Create or clean output directory
try
{
    if (Directory.Exists(outputPath))
    {
        Console.WriteLine("Cleaning output directory...");
        
        // Try to delete with retry logic
        for (int i = 0; i < 3; i++)
        {
            try
            {
                DeleteDirectory(outputPath);
                break;
            }
            catch (UnauthorizedAccessException) when (i < 2)
            {
                Console.WriteLine($"  Retrying... ({i + 1}/3)");
                Thread.Sleep(1000);
            }
        }
    }
    
    Directory.CreateDirectory(outputPath);
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Could not clean directory: {ex.Message}");
    Console.WriteLine("Continuing with existing files...");
}

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

// Copy wwwroot folder (css, js, images, CNAME, etc.)
var wwwrootSource = Path.Combine("..", "SCRANKRealEstate", "wwwroot");
if (Directory.Exists(wwwrootSource))
{
    Console.WriteLine("\nCopying static assets...");
    CopyDirectory(wwwrootSource, outputPath);
    Console.WriteLine("  ✓ Static assets copied");
    
    // Verify and fix CNAME file
    var cnameSourceFile = Path.Combine(wwwrootSource, "CNAME");
    var cnameDestFile = Path.Combine(outputPath, "CNAME");
    
    if (File.Exists(cnameSourceFile))
    {
        var cnameContent = File.ReadAllText(cnameSourceFile).Trim();
        if (string.IsNullOrWhiteSpace(cnameContent))
        {
            Console.WriteLine("  ⚠ Warning: CNAME file is empty in source!");
            Console.WriteLine("  Creating CNAME file with default domain...");
            cnameContent = "chandlerrealestateteam.com";
            File.WriteAllText(cnameSourceFile, cnameContent);
        }
        
        // Ensure CNAME is copied to destination
        File.WriteAllText(cnameDestFile, cnameContent);
        Console.WriteLine($"  ✓ CNAME file copied: {cnameContent}");
    }
    else
    {
        Console.WriteLine("  ⚠ Warning: CNAME file not found in source!");
        Console.WriteLine("  Creating CNAME file...");
        var cnameContent = "chandlerrealestateteam.com";
        File.WriteAllText(cnameSourceFile, cnameContent);
        File.WriteAllText(cnameDestFile, cnameContent);
        Console.WriteLine($"  ✓ CNAME file created: {cnameContent}");
    }
}

Console.WriteLine("\n✓ Static site generation complete!");
Console.WriteLine($"Output directory: {Path.GetFullPath(outputPath)}");

static void DeleteDirectory(string path)
{
    if (!Directory.Exists(path))
        return;

    // Remove read-only attributes
    var directory = new DirectoryInfo(path);
    foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
    {
        file.Attributes = FileAttributes.Normal;
    }

    // Now delete
    Directory.Delete(path, true);
}

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
        try
        {
            var relativePath = Path.GetRelativePath(sourceDir, file);
            var destFile = Path.Combine(destDir, relativePath);
            
            Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
            
            // Remove read-only attribute if exists
            if (File.Exists(destFile))
            {
                File.SetAttributes(destFile, FileAttributes.Normal);
            }
            
            File.Copy(file, destFile, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Could not copy {file}: {ex.Message}");
        }
    }
}
