using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbCode", Schema = "Cash")]
    [Index(nameof(CashDescription), Name = "IX_Cash_tbCodeDescription", IsUnique = true)]
    [Index(nameof(CategoryCode), nameof(IsEnabled), nameof(CashCode), Name = "IX_Cash_tbCode_Category_IsEnabled_Code", IsUnique = true)]
    [Index(nameof(IsEnabled), nameof(CashCode), Name = "IX_Cash_tbCode_IsEnabled_Code", IsUnique = true)]
    [Index(nameof(IsEnabled), nameof(CashDescription), Name = "IX_Cash_tbCode_IsEnabled_Description", IsUnique = true)]
    public partial class Cash_tbCode
    {
        public Cash_tbCode()
        {
            TbAccounts = new HashSet<Org_tbAccount>();
            TbActivities = new HashSet<Activity_tbActivity>();
            TbEntries = new HashSet<Invoice_tbEntry>();
            TbItems = new HashSet<Invoice_tbItem>();
            TbMirrors = new HashSet<Cash_tbMirror>();
            TbOptions = new HashSet<App_tbOption>();
            TbPayments = new HashSet<Cash_tbPayment>();
            TbPeriods = new HashSet<App_tbPeriod>();
            TbTasks = new HashSet<Task_tbTask>();
            TbInvoiceTasks = new HashSet<Invoice_tbTask>();
            TbTaxTypes = new HashSet<Cash_tbTaxType>();
        }

        [Key]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(10)]
        public string CategoryCode { get; set; }
        [Required]
        [StringLength(10)]
        public string TaxCode { get; set; }
        public short IsEnabled { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(CategoryCode))]
        [InverseProperty(nameof(Cash_tbCategory.TbCodes))]
        public virtual Cash_tbCategory CategoryCodeNavigation { get; set; }
        [ForeignKey(nameof(TaxCode))]
        [InverseProperty(nameof(App_tbTaxCode.TbCodes))]
        public virtual App_tbTaxCode TaxCodeNavigation { get; set; }
        [InverseProperty(nameof(Org_tbAccount.CashCodeNavigation))]
        public virtual ICollection<Org_tbAccount> TbAccounts { get; set; }
        [InverseProperty(nameof(Activity_tbActivity.CashCodeNavigation))]
        public virtual ICollection<Activity_tbActivity> TbActivities { get; set; }
        [InverseProperty(nameof(Invoice_tbEntry.CashCodeNavigation))]
        public virtual ICollection<Invoice_tbEntry> TbEntries { get; set; }
        [InverseProperty(nameof(Invoice_tbItem.CashCodeNavigation))]
        public virtual ICollection<Invoice_tbItem> TbItems { get; set; }
        [InverseProperty(nameof(Cash_tbMirror.CashCodeNavigation))]
        public virtual ICollection<Cash_tbMirror> TbMirrors { get; set; }
        [InverseProperty(nameof(App_tbOption.MinerFeeCodeNavigation))]
        public virtual ICollection<App_tbOption> TbOptions { get; set; }
        [InverseProperty(nameof(Cash_tbPayment.CashCodeNavigation))]
        public virtual ICollection<Cash_tbPayment> TbPayments { get; set; }
        [InverseProperty(nameof(App_tbPeriod.CashCodeNavigation))]
        public virtual ICollection<App_tbPeriod> TbPeriods { get; set; }
        [InverseProperty(nameof(Task_tbTask.CashCodeNavigation))]
        public virtual ICollection<Task_tbTask> TbTasks { get; set; }
        [InverseProperty(nameof(Invoice_tbTask.CashCodeNavigation))]
        public virtual ICollection<Invoice_tbTask> TbInvoiceTasks { get; set; }
        [InverseProperty(nameof(Cash_tbTaxType.CashCodeNavigation))]
        public virtual ICollection<Cash_tbTaxType> TbTaxTypes { get; set; }
    }
}
