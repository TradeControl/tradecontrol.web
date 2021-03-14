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
        public string LogCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LoggedOn { get; set; }
        public short EventTypeCode { get; set; }
        [Required]
        [StringLength(15)]
        public string EventType { get; set; }
        public string EventMessage { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
