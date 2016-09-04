using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using VitEgoDictionary.Models;

namespace VitEgoDictionary.Models
{
    public partial class Word
    {
        /// <summary>
        /// Finds another words with similar spelling but belonging to differnet parts of speech
        /// </summary>
        /// <returns>Similar words</returns>
        public IEnumerable<Word> SimilarWords
        {
            get 
            {
                DictionaryEntities _entities = new DictionaryEntities();
                return _entities.Words.Where(i => i.Name == this.Name && i.IDF_SpeechPart != this.IDF_SpeechPart);
            }
        }

        ///// <summary>
        ///// Finds hyponyms for which this word is the generic
        ///// </summary>
        ///// <returns>Hyponyms</returns>
        //public IEnumerable<Word> Hyponyms
        //{
        //    get
        //    {
        //        DictionaryEntities _entities = new DictionaryEntities();
        //        return _entities.Words.Where(i => i.IDF_GenericWord == this.ID).OrderBy(i => i.Name);
        //    }
        //}
    }
}