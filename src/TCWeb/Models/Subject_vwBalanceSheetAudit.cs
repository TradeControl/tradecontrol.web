using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwBalanceSheetAudit
    {
        [Display (Name = "Year")]
        public short YearNumber { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Financial Year")]
        public string Description { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Month")]
        public string MonthName { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "A/c")]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Subject")]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Subject Type")]
        public string SubjectType { get; set; }
        [StringLength(10)]
        [Display(Name = "Polarity")]
        public string CashMode { get; set; }
        [Display(Name = "Asset Type Code")]
        public short AssetTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Asset Type")]
        public string AssetType { get; set; }
        [Column(TypeName = "datetime")]
        [DataType(DataType.Date)]
        [Display(Name = "Start On")]
        public DateTime StartOn { get; set; }
        [Display(Name = "Balance")]
        [DataType(DataType.Currency)]
        public double Balance { get; set; }
    }
}
