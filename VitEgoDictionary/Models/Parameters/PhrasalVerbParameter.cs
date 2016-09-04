using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Passes data from Create/Edit Phrasal Verb views to appropriate controller methods.
    /// </summary>
    public class PhrasalVerbParameter : ItemParameter
    {
        /// <summary>
        /// Phrasal verb the base verb ID.
        /// </summary>
        [Required]
        public int Verb { get; set; }

        /// <summary>
        /// Phrasal verb the preposition ID.
        /// </summary>
        [Required]
        public int Preposition { get; set; }
    }
}