using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Passes Variation data from Create/Edit Word views to appropriate controller methods.
    /// </summary>
    public class VariationParameter
    {
        /// <summary>
        /// Variation name.
        /// </summary>
        [Required]
        public string Name { get; set; }
        
        /// <summary>
        /// Varaitaion grammar structure ID.
        /// </summary>
        [Required]
        public int GrammarStructure { get; set; }
    }
}