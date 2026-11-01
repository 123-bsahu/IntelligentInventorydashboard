namespace IntelligentInventoryDashboard.Features.RequestVehicleUpdate
{
    /// <summary>
    /// Command for a maker to propose a price change on a vehicle.
    /// </summary>
    public class RequestUpdateCommand
    {
        public int VehicleId { get; set; }

        public decimal ProposedPrice { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
