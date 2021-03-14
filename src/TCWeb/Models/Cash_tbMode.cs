using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMode", Schema = "Cash")]
    public partial class Cash_tbMode
    {
        public Cash_tbMode()
        {
            TbAllocations = new HashSet<Task_tbAllocation>();
            TbCategories = new HashSet<Cash_tbCategory>();
            TbInvoiceType = new HashSet<Invoice_tbType>();
            TbOrgType = new HashSet<Org_tbType>();
        }

        [Key]
        public short CashModeCode { get; set; }
        [StringLength(10)]
        public string CashMode { get; set; }

        [InverseProperty(nameof(Task_tbAllocation.CashModeCodeNavigation))]
        public virtual ICollection<Task_tbAllocation> TbAllocations { get; set; }
        [InverseProperty(nameof(Cash_tbCategory.CashModeCodeNavigation))]
        public virtual ICollection<Cash_tbCategory> TbCategories { get; set; }
        [InverseProperty(nameof(Invoice_tbType.CashModeCodeNavigation))]
        public virtual ICollection<Invoice_tbType> TbInvoiceType { get; set; }
        [InverseProperty(nameof(Org_tbType.CashModeCodeNavigation))]
        public virtual ICollection<Org_tbType> TbOrgType { get; set; }
    }
}
