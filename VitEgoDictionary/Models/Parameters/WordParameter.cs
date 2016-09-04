using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Passes data from Create/Edit Word views to appropriate controller methods.
    /// </summary>
    public class WordParameter : ItemParameter
    {
        /// <summary>
        /// Word name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Word part of speech ID.
        /// </summary>
        [Required]
        public int SpeechPart { get; set; }

        /// <summary>
        /// Word generic word ID.
        /// </summary>
        public int? GenericWord { get; set; }

        /// <summary>
        /// Word countability ID.
        /// </summary>
        public int? Coutability { get; set; }

        /// <summary>
        /// Word collections of variations.
        /// </summary>
        public IEnumerable<VariationParameter> Variations { get; set; }

        /// <summary>
        /// Word collection of preposition used.
        /// </summary>
        public IEnumerable<PrepositionMixParameter> PrepositionMix { get; set; }
    }
}