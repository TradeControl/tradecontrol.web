using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbEntryType", Schema = "Cash")]
    public partial class Cash_tbEntryType
    {
        [Key]
        public short CashEntryTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string CashEntryType { get; set; }
    }
}
