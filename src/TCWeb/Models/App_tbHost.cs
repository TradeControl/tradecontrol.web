using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbHost", Schema = "App")]
    public partial class App_tbHost
    {
        public App_tbHost()
        {
            TbOptions = new HashSet<App_tbOption>();
        }

        [Key]
        [Display(Name = "Id")]
        public int HostId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Host Desc.")]
        public string HostDescription { get; set; }

        [Required]
        [StringLength(256)]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [StringLength(50)]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string EmailPassword { get; set; }

        [Display(Name = "Use SMTP Auth")]
        public bool IsSmtpAuth { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "Host Name")]
        public string HostName { get; set; }

        [Required]
        [Display(Name = "Port")]
        public int HostPort { get; set; }

        [StringLength(50)]
        [Display(Name = "Inserted By")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string InsertedBy { get; set; }

        [Column(TypeName = "datetime")]
        [Display(Name = "Inserted On")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime InsertedOn { get; set; }

        [InverseProperty(nameof(App_tbOption.HostIdNavigation))]
        public virtual ICollection<App_tbOption> TbOptions { get; set; }
    }
}
