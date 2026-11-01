using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentInventoryDashboard.Features.ApproveVehicleUpdate
{
    [ApiController]
    [Route("api/vehicles")]
    [Produces("application/json")]
    [Authorize(Roles = "SeniorManager")]
    public class ApproveUpdateController : ControllerBase
    {
        private readonly ApproveUpdateHandler _handler;

        public ApproveUpdateController(ApproveUpdateHandler handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Checker action (SeniorManager only): approves or rejects a pending price update request.
        /// </summary>
        [HttpPost("approve-update")]
        [ProducesResponseType(typeof(ApproveUpdateResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApproveUpdateResult>> ApproveUpdate([FromBody] ApproveUpdateCommand command, CancellationToken cancellationToken)
        {
            var result = await _handler.Handle(command, cancellationToken);

            if (result is null)
            {
                return NotFound(new { message = $"Vehicle update request with Id {command.RequestId} not found." });
            }

            return Ok(result);
        }
    }
}
