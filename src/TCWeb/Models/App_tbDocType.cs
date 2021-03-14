using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbDocType", Schema = "App")]
    public partial class App_tbDocType
    {
        public App_tbDocType()
        {
            TbDocSpools = new HashSet<App_tbDocSpool>();
        }

        [Key]
        public short DocTypeCode { get; set; }
        [Required]
        [StringLength(50)]
        public string DocType { get; set; }
        public short DocClassCode { get; set; }

        [ForeignKey(nameof(DocClassCode))]
        [InverseProperty(nameof(App_tbDocClass.TbDocTypes))]
        public virtual App_tbDocClass DocClassCodeNavigation { get; set; }
        [InverseProperty(nameof(App_tbDocSpool.DocTypeCodeNavigation))]
        public virtual ICollection<App_tbDocSpool> TbDocSpools { get; set; }
    }
}
