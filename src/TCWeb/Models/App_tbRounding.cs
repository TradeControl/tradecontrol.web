using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbRounding", Schema = "App")]
    public partial class App_tbRounding
    {
        public App_tbRounding()
        {
            TbTaxCodes = new HashSet<App_tbTaxCode>();
        }

        [Key]
        public short RoundingCode { get; set; }
        [Required]
        [StringLength(20)]
        public string Rounding { get; set; }

        [InverseProperty(nameof(App_tbTaxCode.RoundingCodeNavigation))]
        public virtual ICollection<App_tbTaxCode> TbTaxCodes { get; set; }
    }
}
