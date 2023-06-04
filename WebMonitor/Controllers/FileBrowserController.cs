using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using WebMonitor.Model;

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
            return new NotFoundResult();

        var dirs = dirInfo
            .GetDirectories()
            .Select(FileOrDir.Dir)
            .ToList();
        dirs.AddRange(dirInfo.GetFiles().Select(FileOrDir.File));

        return new JsonResult(dirs)
        {
            SerializerSettings = _jsonSerializerOptions
        };
    }

    [HttpGet("file-info")]
    public ActionResult<FileInformation> FileInformation(string path)
    {
        var fileInfo = new FileInfo(path);

        if (!fileInfo.Exists)
            return new NotFoundResult();

        return new JsonResult(new FileInformation(fileInfo))
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
                fileStatues.Add(new FileUploadInfo(file.FileName, false));
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

            fileStatues.Add(new FileUploadInfo(file.FileName, true));
        }

        return new JsonResult(fileStatues)
        {
            SerializerSettings = _jsonSerializerOptions
        };
    }
}
