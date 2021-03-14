using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbRecurrence", Schema = "App")]
    public partial class App_tbRecurrence
    {
        public App_tbRecurrence()
        {
            TbTaxTypes = new HashSet<Cash_tbTaxType>();
        }

        [Key]
        public short RecurrenceCode { get; set; }
        [Required]
        [StringLength(20)]
        public string Recurrence { get; set; }

        [InverseProperty(nameof(Cash_tbTaxType.RecurrenceCodeNavigation))]
        public virtual ICollection<Cash_tbTaxType> TbTaxTypes { get; set; }
    }
}
