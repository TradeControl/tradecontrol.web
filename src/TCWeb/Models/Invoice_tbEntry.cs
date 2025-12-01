using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbEntry", Schema = "Invoice")]
    [Index(nameof(UserId), Name = "IX_Invoice_tbEntry_UserId")]
    public partial class Invoice_tbEntry
    {
        [Required]
        [StringLength(10)]
        [Display(Name = "UID")]
        public string UserId { get; set; }
        [Key]
        [StringLength(10)]
        [Display(Name = "A/c")]
        public string SubjectCode { get; set; }
        [Key]
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [Display(Name = "Type")]
        public short InvoiceTypeCode { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Invoiced")]
        [DataType(DataType.Date)]
        public DateTime InvoicedOn { get; set; }
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Ref.")]
        public string ItemReference { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Total Value (Inc. VAT)")]
        [DataType(DataType.Currency)]
        public decimal TotalValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Invoice Value")]
        [DataType(DataType.Currency)]
        public decimal InvoiceValue { get; set; }
        //[Required]
        //public byte[] RowVer { get; set; }

        [ForeignKey(nameof(SubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbEntries))]
        public virtual Subject_tbSubject SubjectCodeNavigation { get; set; }
        [ForeignKey(nameof(CashCode))]
        [InverseProperty(nameof(Cash_tbCode.TbEntries))]
        public virtual Cash_tbCode CashCodeNavigation { get; set; }
        [ForeignKey(nameof(InvoiceTypeCode))]
        [InverseProperty(nameof(Invoice_tbType.TbEntries))]
        public virtual Invoice_tbType InvoiceTypeCodeNavigation { get; set; }
        [ForeignKey(nameof(TaxCode))]
        [InverseProperty(nameof(App_tbTaxCode.TbEntries))]
        public virtual App_tbTaxCode TaxCodeNavigation { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Usr_tbUser.TbEntries))]
        public virtual Usr_tbUser User { get; set; }
    }
}
