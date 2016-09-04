using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Passes data from Index views to appropriate controller methods.
    /// </summary>
    public class ItemListParameters
    {
        /// <summary>
        /// The page to display.
        /// </summary>
        public int Destination { get; set; }

        /// <summary>
        /// The set of selected topics (if applicable).
        /// </summary>
        public IEnumerable<int> Topics { get; set; }

        /// <summary>
        /// The set of selected formalities.
        /// </summary>
        public IEnumerable<int> Formalities { get; set; }

        /// <summary>
        /// The set of selected parts of speech (if applicable).
        /// </summary>
        public IEnumerable<int> SpeechParts { get; set; }

        /// <summary>
        /// The selected base verb ID (if applicable).
        /// </summary>
        public int BaseVerb { get; set; }
    }
}