using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class RequestPermissionController : ControllerBase
{
    private readonly PermissionDbContext _dbContext;
    private readonly IElasticClient _elasticClient;

    public RequestPermissionController(PermissionDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;

        var elasticsearchUri = new Uri(configuration.GetConnectionString("Elasticsearch"));
        var connectionSettings = new ConnectionSettings(elasticsearchUri);
        _elasticClient = new ElasticClient(connectionSettings);
    }

    [HttpPost]
    public async Task<IActionResult> RequestPermission([FromBody] Permission permission)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            // Save permission to SQL Server
            _dbContext.Permissions.Add(permission);
            await _dbContext.SaveChangesAsync();

            // Save permission to Elasticsearch
            var indexResponse = await _elasticClient.IndexDocumentAsync(permission);
            if (!indexResponse.IsValid)
            {
                // Handle Elasticsearch index error
                return StatusCode(500, "Error saving permission to Elasticsearch.");
            }

            return Ok(permission);
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            return StatusCode(500, "An error occurred while processing the request.");
        }
    }
}
