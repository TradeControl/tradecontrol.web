using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbStatus", Schema = "Project")]
    public partial class Project_tbStatus
    {
        public Project_tbStatus()
        {
            TbAllocationEvents = new HashSet<Project_tbAllocationEvent>();
            TbAllocations = new HashSet<Project_tbAllocation>();
            TbProjects = new HashSet<Project_tbProject>();
        }

        [Key]
        public short ProjectStatusCode { get; set; }
        [Required]
        [StringLength(100)]
        public string ProjectStatus { get; set; }

        [InverseProperty(nameof(Project_tbAllocationEvent.ProjectStatusCodeNavigation))]
        public virtual ICollection<Project_tbAllocationEvent> TbAllocationEvents { get; set; }
        [InverseProperty(nameof(Project_tbAllocation.ProjectStatusCodeNavigation))]
        public virtual ICollection<Project_tbAllocation> TbAllocations { get; set; }
        [InverseProperty(nameof(Project_tbProject.ProjectStatusCodeNavigation))]
        public virtual ICollection<Project_tbProject> TbProjects { get; set; }
    }
}
