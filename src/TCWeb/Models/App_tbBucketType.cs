using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbBucketType", Schema = "App")]
    public partial class App_tbBucketType
    {
        public App_tbBucketType()
        {
            TbOptions = new HashSet<App_tbOption>();
        }

        [Key]
        public short BucketTypeCode { get; set; }
        [Required]
        [StringLength(25)]
        public string BucketType { get; set; }

        [InverseProperty(nameof(App_tbOption.BucketTypeCodeNavigation))]
        public virtual ICollection<App_tbOption> TbOptions { get; set; }
    }
}
