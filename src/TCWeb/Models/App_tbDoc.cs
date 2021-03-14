using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbDoc", Schema = "App")]
    public partial class App_tbDoc
    {
        [Key]
        public short DocTypeCode { get; set; }
        [Key]
        [StringLength(50)]
        public string ReportName { get; set; }
        public short OpenMode { get; set; }
        [Required]
        [StringLength(50)]
        public string Description { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(OpenMode))]
        [InverseProperty(nameof(Usr_tbMenuOpenMode.TbDocs))]
        public virtual Usr_tbMenuOpenMode OpenModeNavigation { get; set; }
    }
}
