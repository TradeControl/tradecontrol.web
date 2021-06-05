using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwEventLog
    {
        [Required]
        [StringLength(20)]
        [Display(Name = "Log Code")]
        public string LogCode { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Logged")]
        [DataType(DataType.DateTime)]
        public DateTime LoggedOn { get; set; }
        [Display(Name = "Type Code")]
        public short EventTypeCode { get; set; }
        [Required]
        [StringLength(15)]
        [Display(Name = "Type")]
        public string EventType { get; set; }
        [Display(Name = "Message")]
        public string EventMessage { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Inserted By")]
        public string InsertedBy { get; set; }

    }
}
