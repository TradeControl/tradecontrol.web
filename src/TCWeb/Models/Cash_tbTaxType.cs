using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTaxType", Schema = "Cash")]
    [Index(nameof(CashCode), Name = "IX_tbTaxType_CashCode")]
    public partial class Cash_tbTaxType
    {
        public Cash_tbTaxType()
        {
            TbTaxCodes = new HashSet<App_tbTaxCode>();
        }

        [Key]
        public short TaxTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string TaxType { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        public short MonthNumber { get; set; }
        public short RecurrenceCode { get; set; }
        [StringLength(10)]
        public string AccountCode { get; set; }
        public short OffsetDays { get; set; }

        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Org_tbOrg.TbTaxTypes))]
        public virtual Org_tbOrg AccountCodeNavigation { get; set; }
        [ForeignKey(nameof(CashCode))]
        [InverseProperty(nameof(Cash_tbCode.TbTaxTypes))]
        public virtual Cash_tbCode CashCodeNavigation { get; set; }
        [ForeignKey(nameof(MonthNumber))]
        [InverseProperty(nameof(App_tbMonth.TbTaxTypes))]
        public virtual App_tbMonth MonthNumberNavigation { get; set; }
        [ForeignKey(nameof(RecurrenceCode))]
        [InverseProperty(nameof(App_tbRecurrence.TbTaxTypes))]
        public virtual App_tbRecurrence RecurrenceCodeNavigation { get; set; }
        [InverseProperty(nameof(App_tbTaxCode.TaxTypeCodeNavigation))]
        public virtual ICollection<App_tbTaxCode> TbTaxCodes { get; set; }
    }
}
