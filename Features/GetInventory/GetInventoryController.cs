using Microsoft.AspNetCore.Mvc;

namespace IntelligentInventoryDashboard.Features.GetInventory
{
    [ApiController]
    [Route("api/vehicles")]
    [Produces("application/json")]
    public class GetInventoryController : ControllerBase
    {
        private readonly GetInventoryHandler _handler;

        public GetInventoryController(GetInventoryHandler handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Returns the vehicle inventory with calculated days in stock and smart recommendations.
        /// </summary>
        [HttpGet("inventory")]
        [ProducesResponseType(typeof(IEnumerable<GetInventoryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GetInventoryResponse>>> GetInventory(CancellationToken cancellationToken)
        {
            var inventory = await _handler.Handle(new GetInventoryQuery(), cancellationToken);

            return Ok(inventory);
        }
    }
}
