using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTaxCode", Schema = "App")]
    public partial class App_tbTaxCode
    {
        public App_tbTaxCode()
        {
            TbCodes = new HashSet<Cash_tbCode>();
            TbEntries = new HashSet<Invoice_tbEntry>();
            TbItems = new HashSet<Invoice_tbItem>();
            TbSubjects = new HashSet<Subject_tbSubject>();
            TbPayments = new HashSet<Cash_tbPayment>();
            TbTasks = new HashSet<Task_tbTask>();
            TbInvoiceTasks = new HashSet<Invoice_tbTask>();
        }

        [Key]
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Description")]
        public string TaxDescription { get; set; }
        [Display(Name = "Tax Type")]
        public short TaxTypeCode { get; set; }
        [Display(Name = "Rounding")]
        public short RoundingCode { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        [Display(Name = "Tax Rate")]
        [DisplayFormat(DataFormatString = "{2:p}")]
        public decimal TaxRate { get; set; }
        [Display(Name = "Decimals")]
        public short Decimals { get; set; }
        [Required]
        [StringLength(50)]
        [Display (Name = "Updated By")]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Updated On")]
        public DateTime UpdatedOn { get; set; }

        [ForeignKey(nameof(RoundingCode))]
        [InverseProperty(nameof(App_tbRounding.TbTaxCodes))]
        public virtual App_tbRounding RoundingCodeNavigation { get; set; }
        [ForeignKey(nameof(TaxTypeCode))]
        [InverseProperty(nameof(Cash_tbTaxType.TbTaxCodes))]
        public virtual Cash_tbTaxType TaxTypeCodeNavigation { get; set; }
        [InverseProperty(nameof(Cash_tbCode.TaxCodeNavigation))]
        public virtual ICollection<Cash_tbCode> TbCodes { get; set; }
        [InverseProperty(nameof(Invoice_tbEntry.TaxCodeNavigation))]
        public virtual ICollection<Invoice_tbEntry> TbEntries { get; set; }
        [InverseProperty(nameof(Invoice_tbItem.TaxCodeNavigation))]
        public virtual ICollection<Invoice_tbItem> TbItems { get; set; }
        [InverseProperty(nameof(Subject_tbSubject.TaxCodeNavigation))]
        public virtual ICollection<Subject_tbSubject> TbSubjects { get; set; }
        [InverseProperty(nameof(Cash_tbPayment.TaxCodeNavigation))]
        public virtual ICollection<Cash_tbPayment> TbPayments { get; set; }
        [InverseProperty(nameof(Task_tbTask.TaxCodeNavigation))]
        public virtual ICollection<Task_tbTask> TbTasks { get; set; }
        [InverseProperty(nameof(Invoice_tbTask.TaxCodeNavigation))]
        public virtual ICollection<Invoice_tbTask> TbInvoiceTasks { get; set; }
    }
}
