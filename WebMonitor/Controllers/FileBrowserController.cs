using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace WebMonitor.Controllers;

[ApiController]
[Route("[controller]")]
public class FileBrowserController : ControllerBase
{
	private readonly JsonSerializerOptions _jsonSerializerOptions = new()
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	[HttpGet("dir")]
	public IActionResult Dir(string? requestedDirectory = null)
	{
		if (requestedDirectory == null)
		{
			return new JsonResult(DriveInfo.GetDrives()
				.Select(driveInfo => FileOrDir.Dir(driveInfo.RootDirectory.FullName)))
			{
				SerializerSettings = _jsonSerializerOptions
			};
		}

		var dirInfo = new DirectoryInfo(requestedDirectory);

		if (!dirInfo.Exists)
			return new BadRequestResult();

		var dirs = dirInfo
			.GetDirectories()
			.Select(dir => FileOrDir.Dir(dir.FullName))
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
	[JsonPropertyName("type")]
	public string Type { get; }

	[JsonPropertyName("path")]
	public string Path { get; }

	[JsonPropertyName("size")]
	public long? Size { get; }

	private FileOrDir(string type, string path, long? size = null)
	{
		Type = type;
		Path = path;
		Size = size;
	}

	public static FileOrDir Dir(string path) => new("dir", path);
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