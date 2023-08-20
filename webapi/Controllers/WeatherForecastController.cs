using BoldReports.Web.ReportViewer;
using BoldReports.Web.ReportDesigner;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}

[ApiController]
[Route("api/[controller]/[action]")]
//[Microsoft.AspNetCore.Cors.EnableCors("AllowAllOrigins")]
public class ReportViewerController : Controller, IReportController
{
    // Report viewer requires a memory cache to store the information of consecutive client request and
    // have the rendered report viewer information in server.
    private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    // IWebHostEnvironment used with sample to get the application data from wwwroot.
    private Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;

    // Post action to process the report from server based json parameters and send the result back to the client.
    public ReportViewerController(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
    {
        _cache = memoryCache;
        _hostingEnvironment = hostingEnvironment;
    }

    // Post action to process the report from server based json parameters and send the result back to the client.
    [HttpPost]
    public object PostReportAction([FromBody] Dictionary<string, object> jsonArray)
    {
        return ReportHelper.ProcessReport(jsonArray, this, this._cache);
    }

    // Method will be called to initialize the report information to load the report with ReportHelper for processing.
    [NonAction]
    public void OnInitReportOptions(ReportViewerOptions reportOption)
    {
        string basePath = _hostingEnvironment.WebRootPath;
        // Here, we have loaded the sales-order-detail.rdl report from application the folder wwwroot\Resources. sales-order-detail.rdl should be there in wwwroot\Resources application folder.
        System.IO.FileStream reportStream = new System.IO.FileStream(basePath + @"report.rdl", System.IO.FileMode.Open, System.IO.FileAccess.Read);
        reportOption.ReportModel.Stream = reportStream;
    }

    // Method will be called when reported is loaded with internally to start to layout process with ReportHelper.
    [NonAction]
    public void OnReportLoaded(ReportViewerOptions reportOption)
    {
    }

    //Get action for getting resources from the report
    [ActionName("GetResource")]
    [AcceptVerbs("GET")]
    // Method will be called from Report Viewer client to get the image src for Image report item.
    public object GetResource(ReportResource resource)
    {
        return ReportHelper.GetResource(resource, this, _cache);
    }

    [HttpPost]
    public object PostFormReportAction()
    {
        return ReportHelper.ProcessReport(null, this, _cache);
    }
}

[ApiController]
[Route("api/[controller]/[action]")]
public class ReportingAPIController : Controller, BoldReports.Web.ReportDesigner.IReportDesignerController
{
    private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
    private Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;

    public ReportingAPIController(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
    {
        _cache = memoryCache;
        _hostingEnvironment = hostingEnvironment;
    }

    /// <summary>
    /// Get the path of specific file
    /// </summary>
    /// <param name="itemName">Name of the file to get the full path</param>
    /// <param name="key">The unique key for report designer</param>
    /// <returns>Returns the full path of file</returns>
    [NonAction]
    private string GetFilePath(string itemName, string key)
    {
        string dirPath = Path.Combine(this._hostingEnvironment.WebRootPath + "\\" + "Cache", key);

        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }

        return Path.Combine(dirPath, itemName);
    }

    /// <summary>
    /// Action (HttpGet) method for getting resource of images in the report.
    /// </summary>
    /// <param name="key">The unique key for request identification.</param>
    /// <param name="image">The name of requested image.</param>
    /// <returns>Returns the image as HttpResponseMessage content.</returns>
    public object GetImage(string key, string image)
    {
        return ReportDesignerHelper.GetImage(key, image, this);
    }

    /// <summary>
    /// Send a GET request and returns the requested resource for a report.
    /// </summary>
    /// <param name="resource">Contains report resource information.</param>
    /// <returns> Resource object for the given key</returns>
    public object GetResource(ReportResource resource)
    {
        return ReportHelper.GetResource(resource, this, _cache);
    }

    [NonAction]
    public void OnInitReportOptions(ReportViewerOptions reportOption)
    {

    }

    [NonAction]
    public void OnReportLoaded(ReportViewerOptions reportOption)
    {

    }

    /// <summary>
    /// Action (HttpPost) method for posting the request for designer actions.
    /// </summary>
    /// <param name="jsonResult">A collection of keys and values to process the designer request.</param>
    /// <returns>Json result for the current request.</returns>
    [HttpPost]
    public object PostDesignerAction([FromBody] Dictionary<string, object> jsonResult)
    {
        return ReportDesignerHelper.ProcessDesigner(jsonResult, this, null, this._cache);
    }

    public object PostFormDesignerAction()
    {
        return ReportDesignerHelper.ProcessDesigner(null, this, null, this._cache);
    }

    public object PostFormReportAction()
    {
        return ReportHelper.ProcessReport(null, this, this._cache);
    }

    /// <summary>
    /// Action (HttpPost) method for posting the request for report process.
    /// </summary>
    /// <param name="jsonResult">The JSON data posted for processing report.</param>
    /// <returns>The object data.</returns>
    [HttpPost]
    public object PostReportAction([FromBody] Dictionary<string, object> jsonResult)
    {
        return ReportHelper.ProcessReport(jsonResult, this, this._cache);
    }

    /// <summary>
    /// Sets the resource into storage location.
    /// </summary>
    /// <param name="key">The unique key for request identification.</param>
    /// <param name="itemId">The unique key to get the required resource.</param>
    /// <param name="itemData">Contains the resource data.</param>
    /// <param name="errorMessage">Returns the error message, if the write action is failed.</param>
    /// <returns>Returns true, if resource is successfully written into storage location.</returns>
    [NonAction]
    public bool SetData(string key, string itemId, ItemInfo itemData, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (itemData.Data != null)
        {
            System.IO.File.WriteAllBytes(this.GetFilePath(itemId, key), itemData.Data);
        }
        else if (itemData.PostedFile != null)
        {
            var fileName = itemId;
            if (string.IsNullOrEmpty(itemId))
            {
                fileName = System.IO.Path.GetFileName(itemData.PostedFile.FileName);
            }

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                itemData.PostedFile.OpenReadStream().CopyTo(stream);
                byte[] bytes = stream.ToArray();
                var writePath = this.GetFilePath(fileName, key);

                System.IO.File.WriteAllBytes(writePath, bytes);
                stream.Close();
                stream.Dispose();
            }
        }
        return true;
    }

    /// <summary>
    /// Gets the resource from storage location.
    /// </summary>
    /// <param name="key">The unique key for request identification.</param>
    /// <param name="itemId">The unique key to get the required resource.</param>
    ///  <returns>Returns the resource data and error message.</returns>
    [NonAction]
    public ResourceInfo GetData(string key, string itemId)
    {
        var resource = new ResourceInfo();
        try
        {
            var filePath = this.GetFilePath(itemId, key);
            if (itemId.Equals(Path.GetFileName(filePath), StringComparison.InvariantCultureIgnoreCase) && System.IO.File.Exists(filePath))
            {
                resource.Data = System.IO.File.ReadAllBytes(filePath);
            }
            else
            {
                resource.ErrorMessage = "File not found from the specified path";
            }
        }
        catch (Exception ex)
        {
            resource.ErrorMessage = ex.Message;
        }
        return resource;
    }

    /// <summary>
    /// Action (HttpPost) method for posted or uploaded file actions.
    /// </summary>
    [HttpPost]
    public void UploadReportAction()
    {
        ReportDesignerHelper.ProcessDesigner(null, this, this.Request.Form.Files[0], this._cache);
    }
}