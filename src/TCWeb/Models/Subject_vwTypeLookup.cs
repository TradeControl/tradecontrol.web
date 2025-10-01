using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwTypeLookup
    {
        [Display(Name = "Subject Type Code")]
        public short SubjectTypeCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name ="Type")]
        public string SubjectType { get; set; }
        [StringLength(10)]
        [Display(Name = "Mode")]
        public string CashMode { get; set; }
    }
}
