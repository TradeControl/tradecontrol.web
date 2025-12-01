using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwListAll
    {
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        [Required]
        [StringLength(50)]
        public string SubjectType { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        public short CashPolarityCode { get; set; }
        public short SubjectStatusCode { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
    }
}
