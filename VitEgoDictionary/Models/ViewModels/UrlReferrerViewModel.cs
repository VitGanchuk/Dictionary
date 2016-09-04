using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace VitEgoDictionary.Models.ViewModels
{
    /// <summary>
    /// The base 'Create' view model with the referrer URL to go back if canceled.
    /// </summary>
    public class UrlReferrerViewModel : MasterLayoutViewModel
    {
        /// <summary>
        /// The referrer URL to go back if canceled.
        /// </summary>
        public Uri UrlReferrer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlReferrer"></param>
        /// <param name="isAuthenticated"></param>
        /// <param name="user"></param>
        /// <param name="entities"></param>
        public UrlReferrerViewModel(Uri urlReferrer, bool isAuthenticated, IPrincipal user, DictionaryEntities entities)
            : base(isAuthenticated, user, entities)
        {
            UrlReferrer = urlReferrer;
        }
    }
}