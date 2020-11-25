using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SmartHome
{
   
    /// <summary>
    /// The Device class holds the Smart Home Device.
    /// </summary>
    public class Device
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeviceId { get; set; }
        [Required]
        [StringLength(32, MinimumLength = 5)]
        [Display(Name = "Device Name")]
        public string DeviceName { get; set; }
        [Display(Name = "Device Type")]
        public DeviceType DeviceType { get; set; }
        [Required]
        [ForeignKey("TypeId")]
        public int DeviceTypeTypeId { get; set; }
        public string Description { get; set; }
        [InverseProperty("Device")]
        public List<DeviceImage> Images { get; set; }
        [Required]
        [Display(Name = "Added By")]
        public IdentityUser AddedBy { get; set; }
    }

    /// <summary>
    /// DeviceType is a list of types of the device: switch, plug, thermostat, etc.
    /// </summary>
    public class DeviceType
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TypeId { get; set; }
        [Required]
        [StringLength(32, MinimumLength = 4)]
        [Display(Name = "Device Type")]
        public string TypeName { get; set; }
    }

    /// <summary>
    /// DeviceImage holds a filestream of the image
    /// </summary>
    public class DeviceImage
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImageId { get; set; }
        [Required]
        public Device Device { get; set; }
        [Required]
        [MaxLength(32)]
        public string ContentType { get; set; }
        [Required]
        public byte[] Image { get; set; }
        [Required]
        public byte[] Thumbnail { get; set; }
        public string Description { get; set; }

        [NotMapped]
        public string ImageString => "data:" + ContentType + ";base64," +
                Convert.ToBase64String(Image, 0, Image.Length);
        [NotMapped]
        public string ThumbString => "data:image/jpeg;base64," +
                Convert.ToBase64String(Thumbnail, 0, Thumbnail.Length);
    }
}
