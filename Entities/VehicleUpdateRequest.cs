namespace IntelligentInventoryDashboard.Entities
{
    public enum UpdateRequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class VehicleUpdateRequest
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public decimal ProposedPrice { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string RequestedBy { get; set; } = string.Empty;

        public UpdateRequestStatus Status { get; set; } = UpdateRequestStatus.Pending;

        public DateTime RequestedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Navigation property
        public Vehicles? Vehicle { get; set; }
    }
}
