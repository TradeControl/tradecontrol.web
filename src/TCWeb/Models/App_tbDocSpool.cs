using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbDocSpool", Schema = "App")]
    public partial class App_tbDocSpool
    {
        [Key]
        [StringLength(50)]
        public string UserName { get; set; }
        [Key]
        public short DocTypeCode { get; set; }
        [Key]
        [StringLength(25)]
        public string DocumentNumber { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime SpooledOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(DocTypeCode))]
        [InverseProperty(nameof(App_tbDocType.TbDocSpools))]
        public virtual App_tbDocType DocTypeCodeNavigation { get; set; }
    }
}
