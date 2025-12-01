using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwStatementWhatIf
    {
        public long RowNumber { get; set; }
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        [StringLength(100)]
        public string EntryDescription { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime TransactOn { get; set; }
        [StringLength(30)]
        public string ReferenceCode { get; set; }
        public int CashEntryTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string CashEntryType { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PayIn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PayOut { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal Balance { get; set; }
    }
}
