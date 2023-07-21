using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ModifyPermissionController : ControllerBase
{
    private readonly PermissionDbContext _dbContext;
    private readonly IElasticClient _elasticClient;

    public ModifyPermissionController(PermissionDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;

        var elasticsearchUri = new Uri("http://localhost:9200");
        var connectionSettings = new ConnectionSettings(elasticsearchUri);
        _elasticClient = new ElasticClient(connectionSettings);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ModifyPermission(int id, [FromBody] Permission permission)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            // Update permission in SQL Server
            var existingPermission = _dbContext.Permissions.FirstOrDefault(p => p.Id == id);
            if (existingPermission == null)
                return NotFound();

            existingPermission.NombreEmpleado = permission.NombreEmpleado;
            existingPermission.ApellidoEmpleado = permission.ApellidoEmpleado;
            existingPermission.TipoPermiso = permission.TipoPermiso;
            existingPermission.FechaPermiso = permission.FechaPermiso;

            await _dbContext.SaveChangesAsync();

            // Update permission in Elasticsearch
            var updateResponse = await _elasticClient.UpdateAsync<Permission>(id, u => u.Doc(permission));
            if (!updateResponse.IsValid)
            {
                // Handle Elasticsearch update error
                return StatusCode(500, "Error updating permission in Elasticsearch.");
            }

            return Ok(existingPermission);
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            return StatusCode(500, "An error occurred while processing the request.");
        }
    }
}
