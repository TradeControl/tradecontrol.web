using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbProject", Schema = "Project")]
    [Index(nameof(ActionOn), nameof(ProjectStatusCode), nameof(CashCode), nameof(ProjectCode), Name = "IX_Project_tbProject_ActionOn_Status_CashCode")]
    [Index(nameof(ActionOn), nameof(ProjectCode), nameof(CashCode), nameof(ProjectStatusCode), nameof(SubjectCode), Name = "IX_Project_tbProject_ActionOn_ProjectCode_CashCode")]
    [Index(nameof(ProjectStatusCode), nameof(TaxCode), nameof(ProjectCode), nameof(CashCode), nameof(ActionOn), Name = "IX_Project_tbProject_Status_TaxCode_ProjectCode")]
    [Index(nameof(ProjectCode), nameof(CashCode), Name = "IX_Project_tbProject_ProjectCode_CashCode")]
    [Index(nameof(ProjectCode), nameof(TaxCode), nameof(CashCode), nameof(ActionOn), Name = "IX_Project_tbProject_ProjectCode_TaxCode_CashCode")]
    public partial class Project_tbProject
    {
        public Project_tbProject()
        {
            TbAttribute1s = new HashSet<Project_tbAttribute>();
            TbCostSets = new HashSet<Project_tbCostSet>();
            TbDocs = new HashSet<Project_tbDoc>();
            TbFlowChildProjectCodeNavigations = new HashSet<Project_tbFlow>();
            TbFlowParentProjectCodeNavigations = new HashSet<Project_tbFlow>();
            TbOps = new HashSet<Project_tbOp>();
            TbQuotes = new HashSet<Project_tbQuote>();
            TbProjects = new HashSet<Invoice_tbProject>();
        }

        [Key]
        [StringLength(20)]
        public string ProjectCode { get; set; }
        [Required]
        [StringLength(10)]
        public string UserId { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [StringLength(20)]
        public string SecondReference { get; set; }
        [StringLength(100)]
        public string ProjectTitle { get; set; }
        [StringLength(100)]
        public string ContactName { get; set; }
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        public short ProjectStatusCode { get; set; }
        [Required]
        [StringLength(10)]
        public string ActionById { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionedOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime PaymentOn { get; set; }
        [StringLength(255)]
        public string ProjectNotes { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [StringLength(15)]
        public string AddressCodeFrom { get; set; }
        [StringLength(15)]
        public string AddressCodeTo { get; set; }
        public bool Spooled { get; set; }
        public bool Printed { get; set; }
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
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalCharge { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }

        [ForeignKey(nameof(SubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbProjects))]
        public virtual Subject_tbSubject SubjectCodeNavigation { get; set; }
        [ForeignKey(nameof(ActionById))]
        [InverseProperty(nameof(Usr_tbUser.TbProjectActionBys))]
        public virtual Usr_tbUser ActionBy { get; set; }
        [ForeignKey(nameof(ObjectCode))]
        [InverseProperty(nameof(Object_tbObject.TbProjects))]
        public virtual Object_tbObject ObjectCodeNavigation { get; set; }
        [ForeignKey(nameof(AddressCodeFrom))]
        [InverseProperty(nameof(Subject_tbAddress.TbProjectAddressCodeFromNavigations))]
        public virtual Subject_tbAddress AddressCodeFromNavigation { get; set; }
        [ForeignKey(nameof(AddressCodeTo))]
        [InverseProperty(nameof(Subject_tbAddress.TbProjectAddressCodeToNavigations))]
        public virtual Subject_tbAddress AddressCodeToNavigation { get; set; }
        [ForeignKey(nameof(CashCode))]
        [InverseProperty(nameof(Cash_tbCode.TbProjects))]
        public virtual Cash_tbCode CashCodeNavigation { get; set; }
        [ForeignKey(nameof(ProjectStatusCode))]
        [InverseProperty(nameof(Project_tbStatus.TbProjects))]
        public virtual Project_tbStatus ProjectStatusCodeNavigation { get; set; }
        [ForeignKey(nameof(TaxCode))]
        [InverseProperty(nameof(App_tbTaxCode.TbProjects))]
        public virtual App_tbTaxCode TaxCodeNavigation { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Usr_tbUser.TbProjectUsers))]
        public virtual Usr_tbUser User { get; set; }
        [InverseProperty(nameof(Project_tbAttribute.ProjectCodeNavigation))]
        public virtual ICollection<Project_tbAttribute> TbAttribute1s { get; set; }
        [InverseProperty(nameof(Project_tbCostSet.ProjectCodeNavigation))]
        public virtual ICollection<Project_tbCostSet> TbCostSets { get; set; }
        [InverseProperty(nameof(Project_tbDoc.ProjectCodeNavigation))]
        public virtual ICollection<Project_tbDoc> TbDocs { get; set; }
        [InverseProperty(nameof(Project_tbFlow.ChildProjectCodeNavigation))]
        public virtual ICollection<Project_tbFlow> TbFlowChildProjectCodeNavigations { get; set; }
        [InverseProperty(nameof(Project_tbFlow.ParentProjectCodeNavigation))]
        public virtual ICollection<Project_tbFlow> TbFlowParentProjectCodeNavigations { get; set; }
        [InverseProperty(nameof(Project_tbOp.ProjectCodeNavigation))]
        public virtual ICollection<Project_tbOp> TbOps { get; set; }
        [InverseProperty(nameof(Project_tbQuote.ProjectCodeNavigation))]
        public virtual ICollection<Project_tbQuote> TbQuotes { get; set; }
        [InverseProperty(nameof(Invoice_tbProject.ProjectCodeNavigation))]
        public virtual ICollection<Invoice_tbProject> TbProjects { get; set; }
    }
}
