using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public class App_vwHost
    {
        [Display(Name = "Id")]
        public int HostId { get; set; }
        [StringLength(50)]
        [Display(Name = "Host Desc.")]
        public string HostDescription { get; set; }
        [StringLength(256)]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
        [StringLength(50)]
        [Display(Name = "Password")]
        public string EmailPassword { get; set; }
        [StringLength(256)]
        [Display(Name = "Host Name")]
        public string HostName { get; set; }
        [Display(Name = "Port")]
        public int HostPort { get; set; }
    }
}
