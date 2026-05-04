using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbExecutionStatus", Schema = "App")]
    public partial class App_tbExecutionStatus
    {
        public App_tbExecutionStatus()
        {
            App_tbExecutions = new HashSet<App_tbExecution>();
        }

        [Key]
        public short ExecutionStatusCode { get; set; }

        [Required]
        [StringLength(25)]
        public string ExecutionStatus { get; set; }

        [InverseProperty(nameof(App_tbExecution.ExecutionStatusCodeNavigation))]
        public virtual ICollection<App_tbExecution> App_tbExecutions { get; set; }
    }
}
