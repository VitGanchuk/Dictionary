using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace VitEgoDictionary.Models.ViewModels
{
    /// <summary>
    /// Displays the user's authentification status on all pages.
    /// </summary>
    public class MasterLayoutViewModel
    {
        protected DictionaryEntities _entities;
        
        /// <summary>
        /// Gets a value that indicates whether the request has been authenticated.
        /// </summary>
        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// An object that contains security information for the current HTTP request.
        /// </summary>
        public IPrincipal User { get; private set; }

        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        /// <remarks>It's used itself for 'Home/Index', 'Home/Construction', 'Home/About' pages loading</remarks>
        /// <param name="isAuthenticated">Gets a value that indicates whether the request has been authenticated.</param>
        /// <param name="user">An object that contains security information for the current HTTP request.</param>
        /// <param name="entities">The application's object context.</param>
        public MasterLayoutViewModel(bool isAuthenticated, IPrincipal user, DictionaryEntities entities)
        {
            this._entities = entities;
            this.IsAuthenticated = isAuthenticated;
            this.User = user;
        }
    }
}