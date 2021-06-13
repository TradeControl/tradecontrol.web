using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbYearPeriod", Schema = "App")]
    //[Index(nameof(StartOn), Name = "IX_App_tbYearPeriod_StartOn", IsUnique = true)]
    [Index(nameof(YearNumber), nameof(MonthNumber), Name = "IX_App_tbYearPeriod_Year_MonthNumber", IsUnique = true)]
    public partial class App_tbYearPeriod
    {
        public App_tbYearPeriod()
        {
            TbPeriods = new HashSet<App_tbPeriod>();
        }

        [Key]
        public short YearNumber { get; set; }
        [Key]
        [Display(Name = "Month No.")]
        public short MonthNumber { get; set; }
        [Column(TypeName = "datetime")]
        [DataType(DataType.Date)]
        [Display(Name ="Start On")]
        public DateTime StartOn { get; set; }
        [Display(Name ="Mode")]
        public short CashStatusCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name ="Inserted By")]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name ="Inserted")]
        public DateTime InsertedOn { get; set; }
        public float CorporationTaxRate { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxAdjustment { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal VatAdjustment { get; set; }

        [ForeignKey(nameof(CashStatusCode))]
        [InverseProperty(nameof(Cash_tbStatus.TbYearPeriods))]
        public virtual Cash_tbStatus CashStatusCodeNavigation { get; set; }
        [ForeignKey(nameof(MonthNumber))]
        [InverseProperty(nameof(App_tbMonth.TbYearPeriods))]
        public virtual App_tbMonth MonthNumberNavigation { get; set; }
        [ForeignKey(nameof(YearNumber))]
        [InverseProperty(nameof(App_tbYear.TbYearPeriods))]
        public virtual App_tbYear YearNumberNavigation { get; set; }
        public virtual ICollection<App_tbPeriod> TbPeriods { get; set; }
    }
}
