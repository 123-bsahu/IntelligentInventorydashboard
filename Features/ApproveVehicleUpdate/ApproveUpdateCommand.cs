namespace IntelligentInventoryDashboard.Features.ApproveVehicleUpdate
{
    /// <summary>
    /// Command for a checker (senior manager) to approve or reject a pending price update request.
    /// </summary>
    public class ApproveUpdateCommand
    {
        public int RequestId { get; set; }

        public bool IsApproved { get; set; }
    }
}
