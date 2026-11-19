using Microsoft.AspNetCore.Mvc;
using VehicleExplorer.Api.Services;

namespace VehicleExplorer.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleCatalog _catalog;

    public VehiclesController(IVehicleCatalog catalog)
    {
        _catalog = catalog;
    }

    [HttpGet("makes")]
    public async Task<IActionResult> GetMakes(
        CancellationToken cancellationToken)
    {
        var makes = await _catalog.GetMakesAsync(
            cancellationToken);

        return Ok(makes);
    }

    [HttpGet("makes/{makeId:int}/types")]
    public async Task<IActionResult> GetVehicleTypes(
        [FromRoute] int makeId,
        CancellationToken cancellationToken)
    {
        if (makeId <= 0)
            return BadRequest("Invalid make ID.");

        var types = await _catalog.GetTypesAsync(
            makeId,
            cancellationToken);

        return Ok(types);
    }

    [HttpGet("models")]
    public async Task<IActionResult> GetModels(
        [FromQuery] int makeId,
        [FromQuery] int year,
        [FromQuery] string? type,
        CancellationToken cancellationToken)
    {
        if (makeId <= 0)
            return BadRequest("Invalid make ID.");

        if (year < 1886 || year > DateTime.UtcNow.Year + 1)
            return BadRequest("Invalid model year.");

        var models = await _catalog.GetModelsAsync(
            makeId,
            year,
            type,
            cancellationToken);

        return Ok(models);
    }
}