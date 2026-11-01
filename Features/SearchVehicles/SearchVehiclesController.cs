using IntelligentInventoryDashboard.Features.GetInventory;
using IntelligentInventoryDashboard.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentInventoryDashboard.Features.SearchVehicles
{
    [ApiController]
    [Route("api/vehicles")]
    [Produces("application/json")]
    public class SearchVehiclesController : ControllerBase
    {
        private readonly SearchVehiclesHandler _handler;
        private readonly NLPSearchService _nlp;

        public SearchVehiclesController(SearchVehiclesHandler handler, NLPSearchService nlp)
        {
            _handler = handler;
            _nlp = nlp;
        }

        /// <summary>
        /// NLP-powered vehicle search: the natural-language query is translated into
        /// structured filters (make, model, max price, min days in stock) via OpenAI
        /// gpt-4o-mini, with a keyword-only fallback when the AI service is unavailable.
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<GetInventoryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GetInventoryResponse>>> Search(
            [FromQuery(Name = "query")] string? query,
            CancellationToken cancellationToken)
        {
            var filters = await _nlp.ExtractFiltersAsync(query, cancellationToken);

            var vehicles = await _handler.Handle(filters, cancellationToken);

            return Ok(vehicles);
        }
    }
}
