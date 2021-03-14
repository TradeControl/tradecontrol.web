using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbActivity", Schema = "Activity")]
    public partial class Activity_tbActivity
    {
        public Activity_tbActivity()
        {
            TbAttributes = new HashSet<Activity_tbAttribute>();
            TbFlowChildCodeNavigations = new HashSet<Activity_tbFlow>();
            TbFlowParentCodeNavigations = new HashSet<Activity_tbFlow>();
            TbMirrors = new HashSet<Activity_tbMirror>();
            TbOps = new HashSet<Activity_tbOp>();
            TbTasks = new HashSet<Task_tbTask>();
        }

        [Key]
        [StringLength(50)]
        public string ActivityCode { get; set; }
        public short TaskStatusCode { get; set; }
        [Required]
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        public bool Printed { get; set; }
        [StringLength(50)]
        public string RegisterName { get; set; }
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
        [StringLength(100)]
        public string ActivityDescription { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }

        [ForeignKey(nameof(CashCode))]
        [InverseProperty(nameof(Cash_tbCode.TbActivities))]
        public virtual Cash_tbCode CashCodeNavigation { get; set; }
        [ForeignKey(nameof(RegisterName))]
        [InverseProperty(nameof(App_tbRegister.TbActivities))]
        public virtual App_tbRegister RegisterNameNavigation { get; set; }
        [ForeignKey(nameof(UnitOfMeasure))]
        [InverseProperty(nameof(App_tbUom.TbActivities))]
        public virtual App_tbUom UnitOfMeasureNavigation { get; set; }
        [InverseProperty(nameof(Activity_tbAttribute.ActivityCodeNavigation))]
        public virtual ICollection<Activity_tbAttribute> TbAttributes { get; set; }
        [InverseProperty(nameof(Activity_tbFlow.ChildCodeNavigation))]
        public virtual ICollection<Activity_tbFlow> TbFlowChildCodeNavigations { get; set; }
        [InverseProperty(nameof(Activity_tbFlow.ParentCodeNavigation))]
        public virtual ICollection<Activity_tbFlow> TbFlowParentCodeNavigations { get; set; }
        [InverseProperty(nameof(Activity_tbMirror.ActivityCodeNavigation))]
        public virtual ICollection<Activity_tbMirror> TbMirrors { get; set; }
        [InverseProperty(nameof(Activity_tbOp.ActivityCodeNavigation))]
        public virtual ICollection<Activity_tbOp> TbOps { get; set; }
        [InverseProperty(nameof(Task_tbTask.ActivityCodeNavigation))]
        public virtual ICollection<Task_tbTask> TbTasks { get; set; }
    }
}
