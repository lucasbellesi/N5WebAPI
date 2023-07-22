using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace YourProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModifyPermissionController : ControllerBase
    {
        private readonly PermissionDbContext _dbContext;

        public ModifyPermissionController(PermissionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ModifyPermission(int id, [FromBody] PermissionType modifiedPermission)
        {
            try
            {
                // Verificar si el PermissionType existe con el ID proporcionado
                var existingPermissionType = await _dbContext.PermissionsType.FindAsync(id);

                // Si no existe, se crea un nuevo PermissionType
                if (existingPermissionType == null)
                {
                    existingPermissionType = new PermissionType
                    {
                        Id = modifiedPermission.Id,
                        Description = modifiedPermission.Description // Otra propiedad para la descripción del PermissionType
                    };
                    _dbContext.PermissionsType.Add(existingPermissionType);
                }

                // Actualizar la descripción del PermissionType
                existingPermissionType.Description = modifiedPermission.Description;

                await _dbContext.SaveChangesAsync();

                return Ok(existingPermissionType);
            }
            catch (Exception ex)
            {
                // Log o manejar la excepción adecuadamente
                return StatusCode(500, "Ha ocurrido un error al modificar el PermissionType.");
            }
        }
    }
}
