using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.Forum.ViewModels
{
    public class AccountConfirmResetPasswordViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public string UserId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Nova senha")]
        public string NewPassword { get; set; }
    }
}