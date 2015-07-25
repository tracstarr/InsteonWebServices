using System;
using ServiceStack.DataAnnotations;

namespace Insteon.Data
{
    public class InsteonDeviceModel
    {
        [AutoIncrement]
        public int Id { get; set; }
        [Required]
        [Index(Unique = true)]
        public string Address { get; set; }
        public string DisplayName { get; set; }
        [Required]
        public byte Category { get; set; }
        [Required]
        public byte SubCategory { get; set; }
        [Required]
        public byte Firmware { get; set; }
        public byte ProductKeyHigh { get; set; }
        public byte ProductKeyMid { get; set; }
        public byte ProductKeyLow { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
