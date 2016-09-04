using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using VitEgoDictionary.Models;

namespace VitEgoDictionary.Models
{
    public partial class PhrasalVerb
    {
        /// <summary>
        /// The entire phrasal verb's name is composed from a base word and a preposition.
        /// </summary>
        public string Name
        {
            get
            {
                return this.Verb.Name + ' ' + this.Preposition.Name;
            }
        }
    }
}