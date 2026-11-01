using System.ComponentModel.DataAnnotations;

namespace IntelligentInventoryDashboard.Features.CreateVehicle
{
    /// <summary>
    /// Command for registering a new vehicle into the inventory.
    /// </summary>
    public class CreateVehicleCommand
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(32)]
        public string ChassisNumber { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [StringLength(64)]
        public string Make { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [StringLength(64)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int Year { get; set; }

      
        public decimal Price { get; set; }

        [Required]
        public DateOnly ArrivalDate { get; set; }
    }
}
