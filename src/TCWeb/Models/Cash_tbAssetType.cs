using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAssetType", Schema = "Cash")]
    public partial class Cash_tbAssetType
    {
        [Key]
        public short AssetTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string AssetType { get; set; }
    }
}
