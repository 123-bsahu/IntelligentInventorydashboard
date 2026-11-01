using System.ComponentModel.DataAnnotations;

namespace IntelligentInventoryDashboard.Features.LogAgingAction
{
    /// <summary>
    /// Command for logging a stock-aging action taken against a vehicle,
    /// e.g. ActionTypes: "Price Cut", "Promotion", "Auction", "Dealer Transfer".
    /// </summary>
    public class LogAgingActionCommand
    {
        [Range(1, int.MaxValue)]
        public int VehicleId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(32)]
        public string ActionType { get; set; } = string.Empty;

        [StringLength(512)]
        public string Notes { get; set; } = string.Empty;
    }
}
