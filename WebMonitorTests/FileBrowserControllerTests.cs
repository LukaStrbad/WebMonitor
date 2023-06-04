using Microsoft.AspNetCore.Mvc;
using WebMonitor.Controllers;
using WebMonitor.Model;

namespace WebMonitorTests;

[Collection("IO")]
public class FileBrowserControllerTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly FileBrowserController _controller;

    public FileBrowserControllerTests()
    {
        _serviceProvider = new WebMonitorServiceProvider();
        _controller = new FileBrowserController();
    }
    
    [Fact]
    public void Dir_ReturnsValues()
    {
        var dirs = _controller.Dir().Result;

        Assert.NotNull(dirs);
        var jsonResult = Assert.IsType<JsonResult>(dirs);
        Assert.NotNull(jsonResult.Value);
        var directories = Assert.IsAssignableFrom<IEnumerable<FileOrDir>>(jsonResult.Value).ToList();
        Assert.NotEmpty(directories);

        var subDirResult = _controller.Dir(directories.First().Path).Result;
        Assert.NotNull(subDirResult);
        var subDirJsonResult = Assert.IsType<JsonResult>(subDirResult);
        Assert.NotNull(subDirJsonResult.Value);
        var subDirectories = Assert.IsAssignableFrom<IEnumerable<FileOrDir>>(subDirJsonResult.Value).ToList();
        Assert.NotEmpty(subDirectories);

        Assert.NotEqual(directories, subDirectories);
    }
}