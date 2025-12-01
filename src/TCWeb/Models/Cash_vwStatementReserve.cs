using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwStatementReserve
    {
        public long RowNumber { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime TransactOn { get; set; }
        public short CashEntryTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string CashEntryType { get; set; }
        [StringLength(30)]
        public string ReferenceCode { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        public double PayOut { get; set; }
        public double PayIn { get; set; }
        public double Balance { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(100)]
        public string CashDescription { get; set; }
    }
}
