﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Org_vwAddressList
    {
        [StringLength(10)]
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; }
        [StringLength(15)]
        [Display(Name = "Address Code")]
        public string AddressCode { get; set; }
        [StringLength(255)]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }
        [Display(Name = "Status Code")]
        public short OrganisationStatusCode { get; set; }
        [StringLength(255)]
        [Display(Name = "Status")]
        public string OrganisationStatus { get; set; }
        [Display(Name = "Org Type Code")]
        public short OrganisationTypeCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string OrganisationType { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Address")]
        public string Address { get; set; }
        [StringLength(50)]
        [Display(Name = "Insert By")]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Insert On")]
        public DateTime InsertedOn { get; set; }
        [Display(Name = "Is Admin?")]
        public bool IsAdminAddress { get; set; }
    }
}
