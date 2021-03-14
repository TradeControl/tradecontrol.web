using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbPeriod", Schema = "Cash")]
    public partial class App_tbPeriod
    {
        [Key]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Key]
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Column(TypeName = "ntext")]
        public string Note { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceTax { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal ForecastValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal ForecastTax { get; set; }

        [ForeignKey(nameof(CashCode))]
        [InverseProperty(nameof(Cash_tbCode.TbPeriods))]
        public virtual Cash_tbCode CashCodeNavigation { get; set; }
        public virtual App_tbYearPeriod StartOnNavigation { get; set; }
    }
}
