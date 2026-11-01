namespace IntelligentInventoryDashboard.Services
{
    /// <summary>
    /// Structured search filters extracted from a natural-language query.
    /// Individual filters are null when the query does not specify them.
    /// </summary>
    public class VehicleQueryFilters
    {
        public string? Keyword { get; set; }

        public string? Make { get; set; }

        public string? Model { get; set; }

        public decimal? MaxPrice { get; set; }

        public int? MinDays { get; set; }
    }
}
