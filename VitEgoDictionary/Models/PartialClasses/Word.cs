using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using VitEgoDictionary.Models;

namespace VitEgoDictionary.Models {
    public partial class Word {
        /// <summary>
        /// Finds another words with similar spelling but belonging to differnet parts of speech.
        /// </summary>
        /// <returns>The list of similar words.</returns>
        public IEnumerable<Word> SimilarWords {
            get {
                DictionaryEntities _entities = new DictionaryEntities();
                return _entities.Words.Where(i => i.Name == this.Name && i.IDF_SpeechPart != this.IDF_SpeechPart);
            }
        }

        /// <summary>
        /// Finds all collocations containing this word or its variations.
        /// </summary>
        /// <returns>The list of collocations.</returns>
        public IEnumerable<Collocation> Collocations {
            get {
                DictionaryEntities _entities = new DictionaryEntities();
                var variations = Variations.Select(i => i.Name.ToLower());
                List<Collocation> collocations = _entities.Collocations.Where(i => i.Name.Contains(Name.ToLower()) || variations.Any(j => i.Name.Contains(j))).ToList();
                return collocations.Where(i => i.Name.Split().Contains(Name.ToLower()) || variations.Any(j => i.Name.Split().Contains(j)));
            }
        }

        /// <summary>
        /// Finds all idioms containing this word or its variations.
        /// </summary>
        /// <returns>The list of idioms.</returns>
        public IEnumerable<Idiom> Idioms {
            get {
                DictionaryEntities _entities = new DictionaryEntities();
                var variations = Variations.Select(i => i.Name.ToLower());
                List<Idiom> idioms = _entities.Idioms.Where(i => i.Name.Contains(Name.ToLower()) || variations.Any(j => i.Name.Contains(j))).ToList();
                return idioms.Where(i => i.Name.Split().Contains(Name.ToLower()) || variations.Any(j => i.Name.Split().Contains(j)));
            }
        }
    }
}