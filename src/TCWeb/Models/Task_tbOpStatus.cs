using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbOpStatus", Schema = "Task")]
    public partial class Task_tbOpStatus
    {
        public Task_tbOpStatus()
        {
            TbOps = new HashSet<Task_tbOp>();
        }

        [Key]
        public short OpStatusCode { get; set; }
        [Required]
        [StringLength(50)]
        public string OpStatus { get; set; }

        [InverseProperty(nameof(Task_tbOp.OpStatusCodeNavigation))]
        public virtual ICollection<Task_tbOp> TbOps { get; set; }
    }
}
