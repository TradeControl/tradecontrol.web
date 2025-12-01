using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Project_vwProfit
    {
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(20)]
        public string ProjectCode { get; set; }
        public short YearNumber { get; set; }
        [Required]
        [StringLength(10)]
        public string Description { get; set; }
        [Required]
        [StringLength(23)]
        public string Period { get; set; }
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(100)]
        public string ProjectTitle { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(100)]
        public string ProjectStatus { get; set; }
        public short ProjectStatusCode { get; set; }
        public double TotalCharge { get; set; }
        public double InvoicedCharge { get; set; }
        public double InvoicedChargePaid { get; set; }
        public double TotalCost { get; set; }
        public double InvoicedCost { get; set; }
        public double InvoicedCostPaid { get; set; }
        public double Profit { get; set; }
        public double UninvoicedCharge { get; set; }
        public double UnpaidCharge { get; set; }
        public double UninvoicedCost { get; set; }
        public double UnpaidCost { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionedOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime PaymentOn { get; set; }
    }
}
