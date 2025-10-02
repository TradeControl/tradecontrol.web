using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMirrorProject", Schema = "Invoice")]
    [Index(nameof(ProjectCode), nameof(ContractAddress), Name = "IX_Invoice_tbMirrorProject_ProjectCode")]
    public partial class Invoice_tbMirrorProject
    {
        [Key]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        [Key]
        [StringLength(20)]
        public string ProjectCode { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        public byte[] RowVer { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }

        [ForeignKey(nameof(ContractAddress))]
        [InverseProperty(nameof(Invoice_tbMirror.TbMirrorProjects))]
        public virtual Invoice_tbMirror ContractAddressNavigation { get; set; }
    }
}
