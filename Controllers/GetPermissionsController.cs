using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class GetPermissionsController : ControllerBase
{
    private readonly PermissionDbContext _dbContext;
    private readonly IElasticClient _elasticClient;

    public GetPermissionsController(PermissionDbContext dbContext)
    {
        _dbContext = dbContext;

        var elasticsearchUri = new Uri("http://localhost:9200");
        var connectionSettings = new ConnectionSettings(elasticsearchUri);
        _elasticClient = new ElasticClient(connectionSettings);
    }

    [HttpGet]
    public async Task<IActionResult> GetPermissions()
    {
        try
        {
            // Retrieve permissions from SQL Server
            var permissions = _dbContext.Permissions.ToList();

            // Retrieve permissions from Elasticsearch
            var searchResponse = await _elasticClient.SearchAsync<Permission>(s => s.MatchAll());
            if (!searchResponse.IsValid)
            {
                // Handle Elasticsearch search error
                return StatusCode(500, "Error retrieving permissions from Elasticsearch.");
            }

            var elasticPermissions = searchResponse.Documents.ToList();

            // Combine and return the results from both SQL Server and Elasticsearch
            var allPermissions = permissions.Concat(elasticPermissions).ToList();
            return Ok(allPermissions);
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            return StatusCode(500, "An error occurred while processing the request.");
        }
    }
}
