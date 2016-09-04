using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Passes synonym data from Create/Edit views to appropriate controller methods.
    /// </summary>
    public class SynonymMeaningParameter
    {
        /// <summary>
        /// Synonym set item meaning ID.
        /// </summary>
        [Required]
        public int Meaning { get; set; }

        /// <summary>
        /// Synonym set item type.
        /// </summary>
        [Required]
        public int ItemType { get; set; }
    }
}