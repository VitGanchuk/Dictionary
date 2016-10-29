using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models {
    public partial class Idiom {
        /// <summary>
        /// Finds all words or their varations associated with this idiom.
        /// </summary>
        /// <returns>The list of collocations.</returns>
        public IEnumerable<Word> AssociatedWords {
            get {
                DictionaryEntities _entities = new DictionaryEntities();
                List<string> items = Name.Split().ToList();
                return _entities.Words.Where(i => items.Any(j => j == i.Name.ToLower()) || items.Intersect(i.Variations.Select(k => k.Name)).Count() > 0 );
            }
        }
    }
}