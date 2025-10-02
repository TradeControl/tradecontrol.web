using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwUnMirrored
    {
        public int CandidateId { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(50)]
        public string ChargeCode { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        [StringLength(100)]
        public string ChargeDescription { get; set; }
        public short CashPolarityCode { get; set; }
        [StringLength(10)]
        public string CashPolarity { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(38, 20)")]
        public decimal TaxRate { get; set; }
    }
}
