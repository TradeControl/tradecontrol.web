using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbSector", Schema = "Subject")]
    public partial class Subject_tbSector
    {
        [Key]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Key]
        [StringLength(50)]
        public string IndustrySector { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(SubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbSectors))]
        public virtual Subject_tbSubject SubjectCodeNavigation { get; set; }
    }
}
