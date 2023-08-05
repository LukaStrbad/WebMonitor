using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using WebMonitor.Attributes;
using WebMonitor.Middleware;
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

    private readonly JwtOptions _jwtOptions;

    public FileBrowserController(IServiceProvider serviceProvider)
    {
        _jwtOptions = serviceProvider.GetRequiredService<JwtOptions>();
    }

    [HttpGet("dir")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.FileBrowser))]
    public ActionResult<List<FileOrDir>> Dir(string? requestedDirectory = null)
    {
        if (requestedDirectory is null)
        {
            var rootDirs = GetRootDirs();

            if (!rootDirs.Any())
                return new NotFoundResult();

            return new JsonResult(rootDirs)
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

    internal static List<FileOrDir> GetRootDirs()
    {
        List<FileOrDir> rootDirs;
        if (OperatingSystem.IsLinux())
        {
            // On Linux hardcode folders to / and ~
            return new List<FileOrDir>
            {
                FileOrDir.Dir(new DirectoryInfo("/")),
                FileOrDir.Dir(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)))
            };
        }

        return DriveInfo
            .GetDrives()
            .Select(driveInfo => FileOrDir.Dir(driveInfo.RootDirectory))
            .ToList();
    }

    [HttpGet("file-info")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.FileBrowser))]
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
    public async Task<ActionResult> DownloadFile([FromQuery] string path,
        [FromQuery(Name = "access_token")] string accessToken)
    {
        // Authenticate user using the access token
        var tokenHandler = new JwtSecurityTokenHandler();
        var validateToken = tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(_jwtOptions.Key),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out _);

        if (validateToken.Identity is not { IsAuthenticated: true })
            return new UnauthorizedResult();

        HttpContext.User = validateToken;

        // Check if the user is allowed to download files
        if (!await AllowedFeatureMiddleware.IsAllowedAsync(
                new FeatureGuardAttribute(nameof(AllowedFeatures.FileDownload)), HttpContext))
            return new OkObjectResult("You are not allowed to download files");

        new FileExtensionContentTypeProvider().TryGetContentType(path, out var contentType);
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        // return new FileStreamResult(stream, contentType ?? "application/octet-stream");
        return File(stream, contentType ?? "application/octet-stream", Path.GetFileName(path));
    }

    [HttpPost("upload-file")]
    [Authorize]
    [FeatureGuard(nameof(AllowedFeatures.FileUpload))]
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