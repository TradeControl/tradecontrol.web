using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwStatement
    {
        [Required]
        [StringLength(10)]
        [Display (Name = "Account Code")]
        public string AccountCode { get; set; }
        [Display(Name = "Id")]
        public int RowNumber { get; set; }
        [Column(TypeName = "datetime")]
        [DataType (DataType.Date)]
        [Display(Name = "Transacted")]
        public DateTime TransactedOn { get; set; }
        [StringLength(50)]
        [Display(Name = "Ref.")]
        public string Reference { get; set; }
        [StringLength(30)]
        [Display(Name = "Type")]
        public string StatementType { get; set; }
        [Display(Name = "Charge")]
        [DataType (DataType.Currency)]
        public double Charge { get; set; }
        [Display(Name = "Balance")]
        [DataType (DataType.Currency)]
        public double Balance { get; set; }
    }
}
