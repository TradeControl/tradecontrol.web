using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Project_vwQuote
    {
        [Required]
        [StringLength(10)]
        public string UserId { get; set; }
        public short CashPolarityCode { get; set; }
        [StringLength(10)]
        public string CashPolarity { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [Required]
        [StringLength(20)]
        public string ProjectCode { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [StringLength(100)]
        public string ContactName { get; set; }
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        [StringLength(100)]
        public string ProjectTitle { get; set; }
        [StringLength(20)]
        public string SecondReference { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalCharge { get; set; }
        public short Period { get; set; }
        [StringLength(10)]
        public string BucketId { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(50)]
        public string OwnerName { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
