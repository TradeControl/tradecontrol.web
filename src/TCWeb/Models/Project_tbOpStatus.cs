using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbOpStatus", Schema = "Project")]
    public partial class Project_tbOpStatus
    {
        public Project_tbOpStatus()
        {
            TbOps = new HashSet<Project_tbOp>();
        }

        [Key]
        public short OpStatusCode { get; set; }
        [Required]
        [StringLength(50)]
        public string OpStatus { get; set; }

        [InverseProperty(nameof(Project_tbOp.OpStatusCodeNavigation))]
        public virtual ICollection<Project_tbOp> TbOps { get; set; }
    }
}
