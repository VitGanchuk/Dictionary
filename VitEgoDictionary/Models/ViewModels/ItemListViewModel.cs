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
    /// Builds up and passes a list of items to a view.
    /// </summary>
    public class ItemListViewModel : MasterLayoutViewModel
    {
        /// <summary>
        /// The number of records per a page.
        /// </summary>
        private int _recordsPerPage;

        /// <summary>
        /// The number of pages on the pager window;
        /// </summary>
        private int _pagesPerWindow;

        /// <summary>
        /// The flag whether the result is filtered or not.
        /// <remarks>It's also used for sorting purpose:
        /// Filtered results are sorted alphabetically, non-filtered - by date.</remarks>
        /// </summary>
        public bool isFiltered = false;

        /// <summary>
        /// The number of records found.
        /// </summary>
        public int OverallRecords;

        /// <summary>
        /// The overall number of pages.
        /// </summary>
        public int OverallPages;

        /// <summary>
        /// The number of the current page (starting from zero).
        /// </summary>
        public int Page { get; protected set; }

        /// <summary>
        /// The type of the items.
        /// </summary>
        public Item Item { get; protected set; }

        /// <summary>
        /// The current set of items.
        /// </summary>
        public IEnumerable<EntityObject> Items { get; protected set; }

        /// <summary>
        /// The set of pages that should be currently displayed.
        /// </summary>
        public List<int> PagesToDisplay { get; protected set; }

        /// <summary>
        /// Creates a model for the '~/Index' pages as well as for AJAX '~/LoadItems' methods.
        /// </summary>
        /// <param name="item">The item's type to be passed.</param>
        /// <param name="parameters"></param>
        /// <param name="isAuthenticated">Gets a value that indicates whether the request has been authenticated.</param>
        /// <param name="user">An object that contains security information for the current HTTP request.</param>
        /// <param name="entities">The application's object context.</param>
        public ItemListViewModel(Item item, ItemListParameters parameters, bool isAuthenticated, IPrincipal user, DictionaryEntities entities) :
            base(isAuthenticated, user, entities)
        {
            Page = parameters.Destination;
            _recordsPerPage = Int32.Parse(ConfigurationManager.AppSettings["RecordsPerPage"]);
            _pagesPerWindow = Int32.Parse(ConfigurationManager.AppSettings["PagesPerWindow"]);
            Item = item;
            switch (item)
            {
                case Utilities.Item.Word:
                    {
                        IEnumerable<Word> result = _entities.Words;
                        if (parameters.SpeechParts != null)
                        {
                            result = result.Where(i => parameters.SpeechParts.Contains(i.IDF_SpeechPart));
                            isFiltered = true;
                        }
                        if (parameters.Topics != null)
                        {
                            result = result.Where(i => parameters.Topics.Contains(i.IDF_Topic));
                            isFiltered = true;
                        }
                        if (parameters.Formalities != null)
                        {
                            result = result.Where(i => parameters.Formalities.Contains(i.IDF_Formality));
                            isFiltered = true;
                        }
                        OverallRecords = result.Count();
                        if (isFiltered) Items = result.OrderBy(i => i.Name).Skip(Page * _recordsPerPage).Take(_recordsPerPage);
                        else Items = result.OrderByDescending(i => i.Date).Skip(Page * _recordsPerPage).Take(_recordsPerPage);
                        break;
                    }
                case Utilities.Item.PhrasalVerb:
                    {
                        IEnumerable<PhrasalVerb> result = _entities.PhrasalVerbs;
                        if (parameters.BaseVerb > 0)
                        {
                            result = result.Where(i => i.IDF_Verb == parameters.BaseVerb);
                            isFiltered = true;
                        }
                        if (parameters.Formalities != null)
                        {
                            result = result.Where(i => parameters.Formalities.Contains(i.IDF_Formality));
                            isFiltered = true;
                        }
                        OverallRecords = result.Count();
                        if (isFiltered) Items = result.OrderBy(i => i.Name).Skip(Page * _recordsPerPage).Take(_recordsPerPage);
                        else Items = result.OrderByDescending(i => i.Date).Skip(Page * _recordsPerPage).Take(_recordsPerPage);
                        break;
                    }
                case Utilities.Item.Idiom:
                    {
                        IEnumerable<Idiom> result = _entities.Idioms;
                        if (parameters.Topics != null)
                        {
                            result = result.Where(i => parameters.Topics.Contains(i.IDF_Topic));
                            isFiltered = true;
                        }
                        if (parameters.Formalities != null)
                        {
                            result = result.Where(i => parameters.Formalities.Contains(i.IDF_Formality));
                            isFiltered = true;
                        }
                        OverallRecords = result.Count();
                        if (isFiltered) Items = result.OrderBy(i => i.Name).Skip(Page * _recordsPerPage).Take(_recordsPerPage);
                        else Items = result.OrderByDescending(i => i.Date).Skip(Page * _recordsPerPage).Take(_recordsPerPage);
                        break;
                    }
                case Utilities.Item.Collocation:
                    {
                        IEnumerable<Collocation> result = _entities.Collocations;
                        if (parameters.Topics != null)
                        {
                            result = result.Where(i => parameters.Topics.Contains(i.IDF_Topic));
                            isFiltered = true;
                        }
                        if (parameters.Formalities != null)
                        {
                            result = result.Where(i => parameters.Formalities.Contains(i.IDF_Formality));
                            isFiltered = true;
                        }
                        OverallRecords = result.Count();
                        if (isFiltered) Items = result.OrderBy(i => i.Name).Skip(Page * _recordsPerPage).Take(_recordsPerPage);
                        else Items = result.OrderByDescending(i => i.Date).Skip(Page * _recordsPerPage).Take(_recordsPerPage);
                        break;
                    }
                default: Items = null; break;
            }
            OverallPages = OverallRecords / _recordsPerPage;
            if (OverallPages == 0) OverallPages = 1;
            else if (OverallRecords % _recordsPerPage != 0) OverallPages += 1;
            PagesToDisplay = new List<int>(); 
            if (OverallPages > _pagesPerWindow)
            {
                if((Page - 2) < 0)
                {
                    for (int i = 0; i < _pagesPerWindow; i++) { PagesToDisplay.Add(i); }
                }
                else if ((Page + 2) >= OverallPages)
                {
                    for (int i = OverallPages - _pagesPerWindow; i < OverallPages; i++) { PagesToDisplay.Add(i); }
                }
                else
                {
                    for (int i = Page - 2; i < Page + 3; i++) { PagesToDisplay.Add(i); }
                }
            }
            else
            {
                for (int i = 0; i < OverallPages; i++) { PagesToDisplay.Add(i); }
            }
        }
    }
}