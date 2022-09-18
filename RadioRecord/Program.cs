using Microsoft.Extensions.FileProviders;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var rootPath = app.Configuration.GetValue<string>("MP3Root");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(rootPath), RequestPath = "/Mp3" });
app.MapGet("/Files/{date}", (string? date) =>
    Directory.EnumerateFiles(rootPath, $"{date}*.mp3", SearchOption.AllDirectories)
    .ToArray()
    .OrderBy(x => x)
    .Select(x => new FileInfo(x))
    .Select((x, i) => new
    {
        name = x.Name,
        album = "unknown",
        url = $"/Mp3/{x.FullName.Replace(rootPath,"").Replace("\\","/")}"
    }));

//app.MapGet("/Mp3/{filename}", (string filename) =>
//{
//    var path = rootPath + WebUtility.UrlDecode(filename);
//    return Results.File(File.OpenRead(path), "audio/mpeg");
//});

app.Run();
