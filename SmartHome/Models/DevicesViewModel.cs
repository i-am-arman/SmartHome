using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Collections.Generic;
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

        [AtLeastOne(ErrorMessage = "You must upload an image.")]
        [Display(Name = "Image")]
        public List<IFormFile> Files { get; set; }
    }

    public class AtLeastOne : ValidationAttribute
    {
        private List<string> ContentTypes => new List<string>(new string[]
        {
            "image/jpg",
            "image/jpeg",
            "image/pjpeg",
            "image/gif",
            "image/x-png",
            "image/png",
            "image/webp",
            "image/svg+xml"
        });
        public override bool IsValid(object value)
        {
            return value is List<IFormFile> list ? list.Count > 0 && list.TrueForAll(x => ContentTypes.Contains(x.ContentType)) : false;
        }
    }
}
