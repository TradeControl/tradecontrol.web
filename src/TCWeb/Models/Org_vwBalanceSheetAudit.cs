using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Org_vwBalanceSheetAudit
    {
        public short YearNumber { get; set; }
        [Required]
        [StringLength(10)]
        public string Description { get; set; }
        [Required]
        [StringLength(10)]
        public string MonthName { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        public string OrganisationType { get; set; }
        [StringLength(10)]
        public string CashMode { get; set; }
        public short AssetTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string AssetType { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        public double Balance { get; set; }
    }
}
