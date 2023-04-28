using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace WebMonitor.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class FileBrowserController : ControllerBase
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        // For ignoring null values
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // For camelCase property names
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [HttpGet("dir")]
    public ActionResult<List<FileOrDir>> Dir(string? requestedDirectory = null)
    {
        if (requestedDirectory == null)
        {
            return new JsonResult(DriveInfo.GetDrives()
                .Select(driveInfo => FileOrDir.Dir(driveInfo.RootDirectory)))
            {
                SerializerSettings = _jsonSerializerOptions
            };
        }

        var dirInfo = new DirectoryInfo(requestedDirectory);

        if (!dirInfo.Exists)
            return new BadRequestResult();

        var dirs = dirInfo
            .GetDirectories()
            .Select(dir => FileOrDir.Dir(dir))
            .ToList();
        dirs.AddRange(dirInfo.GetFiles().Select(FileOrDir.File));

        return new JsonResult(dirs)
        {
            SerializerSettings = _jsonSerializerOptions
        };
    }

    [HttpGet("download-file")]
    public FileResult DownloadFile(string path)
    {
        new FileExtensionContentTypeProvider().TryGetContentType(path, out var contentType);
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        // return new FileStreamResult(stream, contentType ?? "application/octet-stream");
        return File(stream, contentType ?? "application/octet-stream", Path.GetFileName(path));
    }

    [HttpPost("upload-file")]
    public async Task<IActionResult> UploadFile(string path)
    {
        var outputDir = new DirectoryInfo(path);

        if (!outputDir.Exists)
            return new BadRequestObjectResult($"Directory '${path}' doesn't exist");

        var fileStatues = new List<FileUploadInfo>();

        foreach (var file in Request.Form.Files)
        {
            var outputFile = new FileInfo(Path.Combine(outputDir.FullName, file.FileName));
            if (outputFile.Exists)
            {
                fileStatues.Add(new(file.FileName, false));
                continue;
            }

            var buffer = new byte[10 * 1024];
            using var br = new BinaryReader(file.OpenReadStream());
            await using var bw = new BinaryWriter(outputFile.OpenWrite());

            int read;
            do
            {
                read = br.Read(buffer);
                bw.Write(buffer, 0, read);
            } while (read > 0);

            fileStatues.Add(new(file.FileName, true));
        }

        return new JsonResult(fileStatues)
        {
            SerializerSettings = _jsonSerializerOptions
        };
    }
}

public sealed class FileOrDir
{
    public string Type { get; }

    public string Path { get; }

    public long? Size { get; }

    public int? ChildrenCount { get; }

    private FileOrDir(string type, string path, long? size = null, int? childrenCount = null)
    {
        Type = type;
        Path = path;
        Size = size;
        ChildrenCount = childrenCount;
    }

    public static FileOrDir Dir(DirectoryInfo dir)
    {
        int childrenCount;

        try
        {
            childrenCount = dir.GetFileSystemInfos().Length;
        }
        catch (UnauthorizedAccessException)
        {
            childrenCount = -1;
        }

        return new("dir", dir.FullName, childrenCount: childrenCount);
    }
    public static FileOrDir File(FileInfo file) => new("file", file.FullName, file.Length);
}

public class FileUploadInfo
{
    [JsonPropertyName("filename")]
    public string FileName { get; }

    [JsonPropertyName("success")]
    public bool Success { get; }

    public FileUploadInfo(string fileName, bool success)
    {
        FileName = fileName;
        Success = success;
    }
}
