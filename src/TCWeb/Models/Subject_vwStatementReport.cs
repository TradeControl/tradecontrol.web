using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwStatementReport
    {
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        public int RowNumber { get; set; }
        public short YearNumber { get; set; }
        [Required]
        [StringLength(10)]
        public string Description { get; set; }
        [Required]
        [StringLength(10)]
        public string MonthName { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime TransactedOn { get; set; }
        [StringLength(50)]
        public string Reference { get; set; }
        [StringLength(30)]
        public string StatementType { get; set; }
        public double Charge { get; set; }
        public double Balance { get; set; }
    }
}
