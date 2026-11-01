namespace IntelligentInventoryDashboard.Entities
{
    public class AgingAction
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }

        /// <summary>
        /// e.g. "Price Cut", "Promotion"
        /// </summary>
        public string ActionType { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

      
        public Vehicles? Vehicle { get; set; }
    }
}
