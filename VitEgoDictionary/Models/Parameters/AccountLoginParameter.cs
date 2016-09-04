using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Passes data from Login Account views to appropriate controller methods.
    /// </summary>
    public class AccountLoginParameter
    {
        /// <summary>
        /// Account user name.
        /// </summary>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// Account password.
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Account value that indicates whether the user should be remembered.
        /// </summary>
        [Required]
        public bool RememberMe { get; set; }

        /// <summary>
        /// Account referrer URL to go back if canceled.
        /// </summary>
        [Required]
        public string UrlReferrer { get; set; }
    }
}