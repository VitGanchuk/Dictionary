using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Passes data from ChangePassword Account views to appropriate controller methods.
    /// </summary>
    public class AccountChangePasswordParameter
    {
        /// <summary>
        /// Account old password.
        /// </summary>
        [Required]
        public string OldPassword { get; set; }

        /// <summary>
        /// Account new password.
        /// </summary>
        [Required]
        public string NewPassword { get; set; }

        /// <summary>
        /// Account new password confirmation.
        /// </summary>
        [Required]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Account referrer URL to go back if canceled.
        /// </summary>
        [Required]
        public string UrlReferrer { get; set; }
    }
}