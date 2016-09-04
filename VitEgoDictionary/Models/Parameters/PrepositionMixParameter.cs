using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Passes Preposition mix data from Create/Edit Word views to appropriate controller methods.
    /// </summary>
    public class PrepositionMixParameter
    {
        /// <summary>
        /// Preposition mix ID.
        /// </summary>
        [Required]
        public int ID { get; set; }
        
        /// <summary>
        /// Preposition mix preposition ID.
        /// </summary>
        [Required]
        public int Preposition { get; set; }
        
        /// <summary>
        /// Preposition mix collection of examples.
        /// </summary>
        [Required]
        public IEnumerable<ExampleParameter> Examples { get; set; }
    }
}