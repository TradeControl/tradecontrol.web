using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwCurrentAccount
    {
        [StringLength(10)]
        public string AccountCode { get; set; }
        public short LiquidityLevel { get; set; }
        [StringLength(50)]
        public string AccountName { get; set; }
        [StringLength(10)]
        public string SortCode { get; set; }
        [StringLength(20)]
        public string AccountNumber { get; set; }
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [StringLength(255)]
        public string SubjectName { get; set; }
    }
}
