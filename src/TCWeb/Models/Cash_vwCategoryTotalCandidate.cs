using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwCategoryTotalCandidate
    {
        [Required]
        [StringLength(10)]
        public string CategoryCode { get; set; }
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
        [Required]
        [StringLength(20)]
        public string CategoryType { get; set; }
        [StringLength(25)]
        public string CashType { get; set; }
        [StringLength(10)]
        public string CashPolarity { get; set; }
    }
}
