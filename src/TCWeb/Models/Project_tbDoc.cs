using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbDoc", Schema = "Project")]
    public partial class Project_tbDoc
    {
        [Key]
        [StringLength(20)]
        public string ProjectCode { get; set; }
        [Key]
        [StringLength(255)]
        public string DocumentName { get; set; }
        [Column(TypeName = "ntext")]
        public string DocumentDescription { get; set; }
        [Required]
        [Column(TypeName = "image")]
        public byte[] DocumentImage { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(ProjectCode))]
        [InverseProperty(nameof(Project_tbProject.TbDocs))]
        public virtual Project_tbProject ProjectCodeNavigation { get; set; }
    }
}
