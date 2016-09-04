using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using VitEgoDictionary.Models.Parameters;

namespace VitEgoDictionary.Models.ViewModels
{
    /// <summary>
    /// Passes parameters to the 'Account/ChangePassword' page.
    /// </summary>
    public class AccountChangePasswordiewModel : UrlReferrerViewModel
    {
        /// <summary>
        /// The minimum length required for a password.
        /// </summary>
        public int MinRequiredPasswordLength { get; private set; }

        /// <summary>
        /// The minimum number of special characters that must be present in a valid password.
        /// </summary>
        public int MinRequiredNonAlphanumericCharacters { get; private set; }

        /// <summary>
        /// Creates a model for 'Account/ChangePassword' page.
        /// </summary>
        /// <param name="urlReferrer">The referrer URL to go back if canceled.</param>
        /// <param name="isAuthenticated">Gets a value that indicates whether the request has been authenticated.</param>
        /// <param name="user">An object that contains security information for the current HTTP request.</param>
        /// <param name="minRequiredPasswordLength">The minimum length required for a password.</param>
        /// <param name="minRequiredNonAlphanumericCharacters">The minimum number of special characters that must be present in a valid password.</param>
        /// <param name="entities">The application's object context.</param>
        public AccountChangePasswordiewModel(Uri urlReferrer, bool isAuthenticated, IPrincipal user, 
            int minRequiredPasswordLength, int minRequiredNonAlphanumericCharacters, DictionaryEntities entities) : 
            base(urlReferrer, isAuthenticated, user, entities)
        {
            this.MinRequiredPasswordLength = minRequiredPasswordLength;
            this.MinRequiredNonAlphanumericCharacters = minRequiredNonAlphanumericCharacters;
        }
    }
}