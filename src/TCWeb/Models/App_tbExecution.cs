using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbExecution", Schema = "App")]
    [Index(nameof(ExecutionStatusCode), nameof(QueuedOn), Name = "IX_App_tbExecution_ExecutionStatusCode_QueuedOn")]
    [Index(nameof(QueuedBy), nameof(QueuedOn), Name = "IX_App_tbExecution_QueuedBy_QueuedOn")]
    public partial class App_tbExecution
    {
        [Key]
        [StringLength(20)]
        public string ExecutionCode { get; set; }

        [Required]
        [StringLength(50)]
        public string ExecutionType { get; set; }

        public short ExecutionStatusCode { get; set; }

        [StringLength(10)]
        public string QueuedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime QueuedOn { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? StartedOn { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CompletedOn { get; set; }

        public string Arguments { get; set; }

        [StringLength(255)]
        public string ProgressMessage { get; set; }

        public string ErrorMessage { get; set; }

        //[Timestamp]
        //public byte[] RowVer { get; set; }

        [ForeignKey(nameof(ExecutionStatusCode))]
        [InverseProperty(nameof(App_tbExecutionStatus.App_tbExecutions))]
        public virtual App_tbExecutionStatus ExecutionStatusCodeNavigation { get; set; }

        [ForeignKey(nameof(QueuedBy))]
        public virtual Usr_tbUser QueuedByNavigation { get; set; }
    }
}
