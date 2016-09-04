using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Security.Principal;
using System.Web;
using VitEgoDictionary.Models.Parameters;
using VitEgoDictionary.Models.Utilities;

namespace VitEgoDictionary.Models.ViewModels
{
    /// <summary>
    /// Passes an item to a ~/Item views.
    /// </summary>
    public class ItemViewModel : UrlReferrerViewModel
    {
        /// <summary>
        /// The current item.
        /// </summary>
        public EntityObject Item { get; private set; }

        /// <summary>
        /// Creates a model for the '~/Item' views.
        /// </summary>
        /// <param name="urlReferrer">The referrer URL to go back if canceled.</param>
        /// <param name="item">The item's type to be passed.</param>
        /// <param name="id">The item's ID.</param>
        /// <param name="isAuthenticated">Gets a value that indicates whether the request has been authenticated.</param>
        /// <param name="user">An object that contains security information for the current HTTP request.</param>
        /// <param name="entities">The application's object context.</param>
        public ItemViewModel(Uri urlReferrer, Item item, int id, bool isAuthenticated, IPrincipal user, DictionaryEntities entities) :
            base(urlReferrer, isAuthenticated, user, entities)
        {
            switch(item)
            {
                case Utilities.Item.Word: this.Item = _entities.Words.FirstOrDefault(i => i.ID == id); break;
                case Utilities.Item.PhrasalVerb: this.Item = _entities.PhrasalVerbs.FirstOrDefault(i => i.ID == id); break;
                case Utilities.Item.Idiom: this.Item = _entities.Idioms.FirstOrDefault(i => i.ID == id); break;
                case Utilities.Item.Collocation: this.Item = _entities.Collocations.FirstOrDefault(i => i.ID == id); break;
                default: this.Item = null; break;
            }
        }
    }
}