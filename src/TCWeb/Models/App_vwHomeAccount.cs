using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TradeControl.Web.Models
{
    [Keyless]
    public class App_vwHomeAccount
    {
        [StringLength(10)]
        [Display(Name = "Account Code")]
        public string SubjectCode { get; set; }
        [StringLength(255)]
        [Display(Name = "Account Name")]
        public string SubjectName { get; set; }
    }
}
