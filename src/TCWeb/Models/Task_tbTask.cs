using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTask", Schema = "Task")]
    [Index(nameof(ActionOn), nameof(TaskStatusCode), nameof(CashCode), nameof(TaskCode), Name = "IX_Task_tbTask_ActionOn_Status_CashCode")]
    [Index(nameof(ActionOn), nameof(TaskCode), nameof(CashCode), nameof(TaskStatusCode), nameof(AccountCode), Name = "IX_Task_tbTask_ActionOn_TaskCode_CashCode")]
    [Index(nameof(TaskStatusCode), nameof(TaxCode), nameof(TaskCode), nameof(CashCode), nameof(ActionOn), Name = "IX_Task_tbTask_Status_TaxCode_TaskCode")]
    [Index(nameof(TaskCode), nameof(CashCode), Name = "IX_Task_tbTask_TaskCode_CashCode")]
    [Index(nameof(TaskCode), nameof(TaxCode), nameof(CashCode), nameof(ActionOn), Name = "IX_Task_tbTask_TaskCode_TaxCode_CashCode")]
    public partial class Task_tbTask
    {
        public Task_tbTask()
        {
            TbAttribute1s = new HashSet<Task_tbAttribute>();
            TbCostSets = new HashSet<Task_tbCostSet>();
            TbDocs = new HashSet<Task_tbDoc>();
            TbFlowChildTaskCodeNavigations = new HashSet<Task_tbFlow>();
            TbFlowParentTaskCodeNavigations = new HashSet<Task_tbFlow>();
            TbOps = new HashSet<Task_tbOp>();
            TbQuotes = new HashSet<Task_tbQuote>();
            TbTasks = new HashSet<Invoice_tbTask>();
        }

        [Key]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [Required]
        [StringLength(10)]
        public string UserId { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [StringLength(20)]
        public string SecondReference { get; set; }
        [StringLength(100)]
        public string TaskTitle { get; set; }
        [StringLength(100)]
        public string ContactName { get; set; }
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        public short TaskStatusCode { get; set; }
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
        public string TaskNotes { get; set; }
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

        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbTasks))]
        public virtual Subject_tbSubject AccountCodeNavigation { get; set; }
        [ForeignKey(nameof(ActionById))]
        [InverseProperty(nameof(Usr_tbUser.TbTaskActionBys))]
        public virtual Usr_tbUser ActionBy { get; set; }
        [ForeignKey(nameof(ObjectCode))]
        [InverseProperty(nameof(Object_tbObject.TbTasks))]
        public virtual Object_tbObject ObjectCodeNavigation { get; set; }
        [ForeignKey(nameof(AddressCodeFrom))]
        [InverseProperty(nameof(Subject_tbAddress.TbTaskAddressCodeFromNavigations))]
        public virtual Subject_tbAddress AddressCodeFromNavigation { get; set; }
        [ForeignKey(nameof(AddressCodeTo))]
        [InverseProperty(nameof(Subject_tbAddress.TbTaskAddressCodeToNavigations))]
        public virtual Subject_tbAddress AddressCodeToNavigation { get; set; }
        [ForeignKey(nameof(CashCode))]
        [InverseProperty(nameof(Cash_tbCode.TbTasks))]
        public virtual Cash_tbCode CashCodeNavigation { get; set; }
        [ForeignKey(nameof(TaskStatusCode))]
        [InverseProperty(nameof(Task_tbStatus.TbTasks))]
        public virtual Task_tbStatus TaskStatusCodeNavigation { get; set; }
        [ForeignKey(nameof(TaxCode))]
        [InverseProperty(nameof(App_tbTaxCode.TbTasks))]
        public virtual App_tbTaxCode TaxCodeNavigation { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Usr_tbUser.TbTaskUsers))]
        public virtual Usr_tbUser User { get; set; }
        [InverseProperty(nameof(Task_tbAttribute.TaskCodeNavigation))]
        public virtual ICollection<Task_tbAttribute> TbAttribute1s { get; set; }
        [InverseProperty(nameof(Task_tbCostSet.TaskCodeNavigation))]
        public virtual ICollection<Task_tbCostSet> TbCostSets { get; set; }
        [InverseProperty(nameof(Task_tbDoc.TaskCodeNavigation))]
        public virtual ICollection<Task_tbDoc> TbDocs { get; set; }
        [InverseProperty(nameof(Task_tbFlow.ChildTaskCodeNavigation))]
        public virtual ICollection<Task_tbFlow> TbFlowChildTaskCodeNavigations { get; set; }
        [InverseProperty(nameof(Task_tbFlow.ParentTaskCodeNavigation))]
        public virtual ICollection<Task_tbFlow> TbFlowParentTaskCodeNavigations { get; set; }
        [InverseProperty(nameof(Task_tbOp.TaskCodeNavigation))]
        public virtual ICollection<Task_tbOp> TbOps { get; set; }
        [InverseProperty(nameof(Task_tbQuote.TaskCodeNavigation))]
        public virtual ICollection<Task_tbQuote> TbQuotes { get; set; }
        [InverseProperty(nameof(Invoice_tbTask.TaskCodeNavigation))]
        public virtual ICollection<Invoice_tbTask> TbTasks { get; set; }
    }
}
