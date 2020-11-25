using System.ComponentModel.DataAnnotations;

namespace SmartHome.Models
{
    public class DevicesViewModel
    {
        public int DeviceId { get; set; }

        [Required]
        [StringLength(32, MinimumLength = 5)]
        [Display(Name = "Device Name")]
        public string DeviceName { get; set; }

        [Required]
        [Display(Name = "Device Type")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Please choose a device type")]
        public string DeviceTypeTypeId { get; set; }

        public string Description { get; set; }
    }
}
