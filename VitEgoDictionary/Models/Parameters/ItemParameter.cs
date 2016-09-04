using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Abstact base class to pass data from Create/Edit views to appropriate controller methods.
    /// </summary>
    public abstract class ItemParameter
    {
        /// <summary>
        /// Item ID.
        /// </summary>
        [Required]
        public int ID { get; set; }

        /// <summary>
        /// Item topic ID.
        /// </summary>
        [Required]
        public int Topic { get; set; }

        /// <summary>
        /// Item formality ID.
        /// </summary>
        [Required]
        public int Formality { get; set; }

        /// <summary>
        /// Item meaning collection.
        /// </summary>
        public IEnumerable<MeaningParameter> Meanings { get; set; }
    }
}