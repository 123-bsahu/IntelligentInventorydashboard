using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentInventoryDashboard.Features.RequestVehicleUpdate
{
    [ApiController]
    [Route("api/vehicles")]
    [Produces("application/json")]
    public class RequestUpdateController : ControllerBase
    {
        private readonly RequestUpdateHandler _handler;

        public RequestUpdateController(RequestUpdateHandler handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Maker action: proposes a price change for a vehicle, creating a pending update request.
        /// </summary>
        [HttpPost("request-update")]
        [ProducesResponseType(typeof(RequestUpdateResult), StatusCodes.Status201Created)]
       
        public async Task<ActionResult<RequestUpdateResult>> RequestUpdate([FromBody] RequestUpdateCommand command, CancellationToken cancellationToken)
        {
            var result = await _handler.Handle(command, cancellationToken);

            if (result is null)
            {
                return NotFound(new { message = $"Vehicle with Id {command.VehicleId} not found." });
            }

            return Created($"/api/vehicles/request-update/{result.RequestId}", result);
        }
    }
}
