using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentInventoryDashboard.Features.LogAgingAction
{
    [ApiController]
    [Route("api/vehicles")]
    [Produces("application/json")]
    public class LogAgingActionController : ControllerBase
    {
        private readonly LogAgingActionHandler _handler;

        public LogAgingActionController(LogAgingActionHandler handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Logs an aging action (e.g. "Price Cut", "Promotion") for an existing vehicle.
        /// </summary>
        [HttpPost("aging-actions")]
        [ProducesResponseType(typeof(LogAgingActionResult), StatusCodes.Status201Created)]
     
        public async Task<ActionResult<LogAgingActionResult>> LogAgingAction([FromBody] LogAgingActionCommand command, CancellationToken cancellationToken)
        {
            var result = await _handler.Handle(command, cancellationToken);

            if (result is null)
            {
                return NotFound(new { message = $"Vehicle with Id {command.VehicleId} not found." });
            }

            return Created($"/api/vehicles/{result.VehicleId}/aging-actions/{result.ActionId}", result);
        }
    }
}
