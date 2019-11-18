using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Web.Models.AccountViewModels
{
    /// <summary>
    /// Our LoginViewModel will store all the informaiton that is required for logging into the system, to include
    /// a boolean that will be remembered for future visits to the website. 
    /// 
    /// As this is the standard viewmodel for a login page, we will not change it and use it as it is now.
    /// </summary>
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
