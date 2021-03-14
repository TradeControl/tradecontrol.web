using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbStatus", Schema = "Cash")]
    public partial class Cash_tbStatus
    {
        public Cash_tbStatus()
        {
            TbYearPeriods = new HashSet<App_tbYearPeriod>();
        }

        [Key]
        public short CashStatusCode { get; set; }
        [Required]
        [StringLength(15)]
        public string CashStatus { get; set; }

        [InverseProperty(nameof(App_tbYearPeriod.CashStatusCodeNavigation))]
        public virtual ICollection<App_tbYearPeriod> TbYearPeriods { get; set; }
    }
}
