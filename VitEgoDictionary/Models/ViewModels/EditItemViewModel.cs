using System;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Security.Principal;
using System.Web;
using VitEgoDictionary.Models.Utilities;

namespace VitEgoDictionary.Models.ViewModels
{
    /// <summary>
    /// Passes an Item and some extra data to '~/Item/Edit/Create' views.
    /// </summary>
    public class EditItemViewModel : ItemViewModel
    {
        /// <summary>
        /// The phrasal verb set for Razor's initialization.
        /// </summary>
        public IEnumerable<PhrasalVerb> PhrasalVerbs { get; private set; }

        /// <summary>
        /// The grammar structure set for Razor's initialization.
        /// </summary>
        public IEnumerable<GrammarStructure> GrammarStructures { get; private set; }

        /// <summary>
        /// Creates a model for the '~/Item/Edit/Create' views.
        /// </summary>
        /// <param name="urlReferrer">The referrer URL to go back if canceled.</param>
        /// <param name="item">The item's type to be passed.</param>
        /// <param name="id">The item's ID.</param>
        /// <param name="isAuthenticated">Gets a value that indicates whether the request has been authenticated.</param>
        /// <param name="user">An object that contains security information for the current HTTP request.</param>
        /// <param name="entities">The application's object context.</param>
        public EditItemViewModel(Uri urlReferrer, Item item, int id, bool isAuthenticated, IPrincipal user, DictionaryEntities entities) : 
            base(urlReferrer, item, id, isAuthenticated, user, entities)
        {
            switch (item)
            {
                case Utilities.Item.Word:
                    {
                        PhrasalVerbs = _entities.PhrasalVerbs.OrderBy(i => i.Verb.Name).ThenBy(i => i.Preposition.Name);
                        GrammarStructures = _entities.GrammarStructures.
                            Where(i => i.IDF_SpeechPart == ((Word)Item).IDF_SpeechPart && i.IsAuxiliary == false).OrderBy(i => i.ID);
                        break;
                    }
                case Utilities.Item.Idiom: 
                    {
                        PhrasalVerbs = null;
                        GrammarStructures = null;
                        break;
                    }
                case Utilities.Item.Collocation: 
                    {
                        PhrasalVerbs = null;
                        GrammarStructures = null;
                        break;
                    }
                case Utilities.Item.PhrasalVerb: 
                    {
                        Word verb = ((PhrasalVerb)Item).Verb;
                        PhrasalVerbs = null;
                        GrammarStructures = null;
                        break;
                    }
                default: 
                    {
                        PhrasalVerbs = null;
                        GrammarStructures = null;
                        break;
                    }
            }
        }
    }
}