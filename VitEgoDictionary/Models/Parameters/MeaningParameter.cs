using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Passes meaning data from Create/Edit views to appropriate controller methods.
    /// </summary>
    public class MeaningParameter
    {
        /// <summary>
        /// Meaning ID
        /// </summary>
        [Required]
        public int ID { get; set; }

        /// <summary>
        /// Meaning description
        /// </summary>
        [Required]
        public string Meaning { get; set; }

        /// <summary>
        /// Meaning collection of examples
        /// </summary>
        [Required]
        public IEnumerable<ExampleParameter> Examples { get; set; }

        /// <summary>
        /// Meaning collection of synonyms
        /// </summary>
        public IEnumerable<SynonymMeaningParameter> Synonyms { get; set; }
    }
}