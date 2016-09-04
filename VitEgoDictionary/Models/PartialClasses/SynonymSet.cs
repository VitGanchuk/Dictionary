using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using VitEgoDictionary.Models;

namespace VitEgoDictionary.Models
{
    public partial class SynonymSet
    {
        /// <summary>
        /// Checks whether is empty and can be deleted
        /// </summary>
        /// <returns>True if the set contains only one or no items or true otherwise</returns>
        public bool IsEmpty()
        {
            int overall = this.WordMeanings.Count;
            overall += this.PhrasalVerbMeanings.Count;
            overall += this.CollocationMeanings.Count;
            overall += this.IdiomMeanings.Count;
            return overall <= 1;
        }
    }
}