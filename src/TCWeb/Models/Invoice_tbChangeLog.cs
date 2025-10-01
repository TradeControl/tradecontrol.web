using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbChangeLog", Schema = "Invoice")]
    [Index(nameof(ChangedOn), Name = "IX_Invoice_tbChangeLog_ChangedOn")]
    [Index(nameof(LogId), Name = "IX_Invoice_tbChangeLog_LogId", IsUnique = true)]
    [Index(nameof(TransmitStatusCode), nameof(ChangedOn), Name = "IX_Invoice_tbChangeLog_TransmitStatus")]
    public partial class Invoice_tbChangeLog
    {
        [Key]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Key]
        public int LogId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ChangedOn { get; set; }
        public short TransmitStatusCode { get; set; }
        public short InvoiceStatusCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DueOn { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidTaxValue { get; set; }

        [ForeignKey(nameof(InvoiceNumber))]
        [InverseProperty(nameof(Invoice_tbInvoice.TbChangeLogs))]
        public virtual Invoice_tbInvoice InvoiceNumberNavigation { get; set; }
        [ForeignKey(nameof(TransmitStatusCode))]
        [InverseProperty(nameof(Subject_tbTransmitStatus.TbInvoiceChangeLogs))]
        public virtual Subject_tbTransmitStatus TransmitStatusCodeNavigation { get; set; }
    }
}
