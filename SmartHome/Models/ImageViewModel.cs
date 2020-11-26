using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SmartHome.Models
{
    public class ImageViewModel
    {
        [Required]
        public int DeviceId { get; set; }
        [Required]
        public List<IFormFile> Files { get; set; }
    }
}
