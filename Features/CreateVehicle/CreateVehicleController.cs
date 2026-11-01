using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentInventoryDashboard.Features.CreateVehicle
{
    [ApiController]
    [Route("api/vehicles")]
    [Produces("application/json")]
    public class CreateVehicleController : ControllerBase
    {
        private readonly CreateVehicleHandler _handler;

        public CreateVehicleController(CreateVehicleHandler handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Registers a new vehicle with default status "Approved".
        /// </summary>
        [HttpPost("create")]
        [ProducesResponseType(typeof(CreateVehicleResult), StatusCodes.Status201Created)]
       
        public async Task<ActionResult<CreateVehicleResult>> Create([FromBody] CreateVehicleCommand command, CancellationToken cancellationToken)
        {
            var result = await _handler.Handle(command, cancellationToken);

            if (result.Duplicate == DuplicateReason.ChassisNumber)
            {
                return Conflict(new { message = $"A vehicle with chassis number '{command.ChassisNumber}' already exists." });
            }

            return Created($"/api/vehicles/{result.Id}", result);
        }
    }
}
