using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbBucketInterval", Schema = "App")]
    public partial class App_tbBucketInterval
    {
        public App_tbBucketInterval()
        {
            TbOptions = new HashSet<App_tbOption>();
        }

        [Key]
        public short BucketIntervalCode { get; set; }
        [Required]
        [StringLength(15)]
        public string BucketInterval { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [InverseProperty(nameof(App_tbOption.BucketIntervalCodeNavigation))]
        public virtual ICollection<App_tbOption> TbOptions { get; set; }
    }
}
