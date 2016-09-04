using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Passes example data from Create/Edit Word views to appropriate controller methods.
    /// </summary>
    public class ExampleParameter
    {
        /// <summary>
        /// Example ID
        /// </summary>
        [Required]
        public int ID { get; set; }

        /// <summary>
        /// Example sentense.
        /// </summary>
        [Required]
        public string Example { get; set; }
    }
}