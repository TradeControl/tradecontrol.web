using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbUser", Schema = "Usr")]
    [Index(nameof(IsEnabled), nameof(LogonName), Name = "IX_Usr_tbUser_IsEnabled_LogonName", IsUnique = true)]
    [Index(nameof(IsEnabled), nameof(UserName), Name = "IX_Usr_tbUser_IsEnabled_UserName", IsUnique = true)]
    [Index(nameof(LogonName), Name = "IX_Usr_tbUser_LogonName", IsUnique = true)]
    [Index(nameof(UserName), Name = "IX_Usr_tbUser_UserName", IsUnique = true)]
    public partial class Usr_tbUser
    {
        public Usr_tbUser()
        {
            TbCostSets = new HashSet<Task_tbCostSet>();
            TbEntries = new HashSet<Invoice_tbEntry>();
            TbInvoices = new HashSet<Invoice_tbInvoice>();
            TbMenuUsers = new HashSet<Usr_tbMenuUser>();
            TbOps = new HashSet<Task_tbOp>();
            TbPayments = new HashSet<Cash_tbPayment>();
            TbTaskActionBys = new HashSet<Task_tbTask>();
            TbTaskUsers = new HashSet<Task_tbTask>();
        }

        [Key]
        [StringLength(10)]
        [Display (Name = "User Id")]
        [Required]
        public string UserId { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Local Login")]
        public string LogonName { get; set; }
        [StringLength(10)]
        [Display(Name = "Calendar")]
        public string CalendarCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Landline")]
        public string PhoneNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Mobile")]
        public string MobileNumber { get; set; }
        [StringLength(255)]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Home Address")]
        public string Address { get; set; }
        [Column(TypeName = "image")]
        public byte[] Avatar { get; set; }
        [Column(TypeName = "image")]        
        public byte[] Signature { get; set; }
        [Display(Name = "Admin?")]
        public bool IsAdministrator { get; set; }
        [Display(Name = "Enabled?")]
        public short IsEnabled { get; set; }
        public int NextTaskNumber { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Inserted By")]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Inserted On")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Updated On")]
        public DateTime UpdatedOn { get; set; }
        public short MenuViewCode { get; set; }

        [ForeignKey(nameof(CalendarCode))]
        [InverseProperty(nameof(App_tbCalendar.TbUsers))]
        public virtual App_tbCalendar CalendarCodeNavigation { get; set; }
        [ForeignKey(nameof(MenuViewCode))]
        [InverseProperty(nameof(Usr_tbMenuView.TbUsers))]
        public virtual Usr_tbMenuView MenuViewCodeNavigation { get; set; }
        [InverseProperty(nameof(Task_tbCostSet.User))]
        public virtual ICollection<Task_tbCostSet> TbCostSets { get; set; }
        [InverseProperty(nameof(Invoice_tbEntry.User))]
        public virtual ICollection<Invoice_tbEntry> TbEntries { get; set; }
        [InverseProperty(nameof(Invoice_tbInvoice.User))]
        public virtual ICollection<Invoice_tbInvoice> TbInvoices { get; set; }
        [InverseProperty(nameof(Usr_tbMenuUser.User))]
        public virtual ICollection<Usr_tbMenuUser> TbMenuUsers { get; set; }
        [InverseProperty(nameof(Task_tbOp.User))]
        public virtual ICollection<Task_tbOp> TbOps { get; set; }
        [InverseProperty(nameof(Cash_tbPayment.User))]
        public virtual ICollection<Cash_tbPayment> TbPayments { get; set; }
        [InverseProperty(nameof(Task_tbTask.ActionBy))]
        public virtual ICollection<Task_tbTask> TbTaskActionBys { get; set; }
        [InverseProperty(nameof(Task_tbTask.User))]
        public virtual ICollection<Task_tbTask> TbTaskUsers { get; set; }
    }
}
