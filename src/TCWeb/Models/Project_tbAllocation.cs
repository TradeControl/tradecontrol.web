using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAllocation", Schema = "Project")]
    [Index(nameof(SubjectCode), nameof(AllocationCode), Name = "IX_Project_tbAllocation_ObjectCode")]
    [Index(nameof(SubjectCode), nameof(ProjectCode), Name = "IX_Project_tbAllocation_ProjectCode")]
    [Index(nameof(ProjectStatusCode), nameof(SubjectCode), nameof(AllocationCode), nameof(ActionOn), Name = "IX_Project_tbAllocation_ProjectStatusCode")]
    public partial class Project_tbAllocation
    {
        public Project_tbAllocation()
        {
            TbAllocationEvents = new HashSet<Project_tbAllocationEvent>();
        }

        [Key]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(50)]
        public string AllocationCode { get; set; }
        [StringLength(256)]
        public string AllocationDescription { get; set; }
        [Required]
        [StringLength(20)]
        public string ProjectCode { get; set; }
        [StringLength(100)]
        public string ProjectTitle { get; set; }
        public short CashPolarityCode { get; set; }
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }
        [StringLength(5)]
        public string UnitOfCharge { get; set; }
        public short ProjectStatusCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TaxRate { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal QuantityOrdered { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal QuantityDelivered { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }

        [ForeignKey(nameof(SubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbAllocations))]
        public virtual Subject_tbSubject SubjectCodeNavigation { get; set; }
        [ForeignKey(nameof(CashPolarityCode))]
        [InverseProperty(nameof(Cash_tbPolarity.TbAllocations))]
        public virtual Cash_tbPolarity CashPolarityCodeNavigation { get; set; }
        [ForeignKey(nameof(ProjectStatusCode))]
        [InverseProperty(nameof(Project_tbStatus.TbAllocations))]
        public virtual Project_tbStatus ProjectStatusCodeNavigation { get; set; }
        [InverseProperty(nameof(Project_tbAllocationEvent.ContractAddressNavigation))]
        public virtual ICollection<Project_tbAllocationEvent> TbAllocationEvents { get; set; }
    }
}
