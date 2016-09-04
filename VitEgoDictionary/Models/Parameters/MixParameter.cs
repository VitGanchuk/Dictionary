using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Passes Idiom/Collocation mix data from Create/Edit views to appropriate controller methods.
    /// </summary>
    public class MixParameter
    {
        /// <summary>
        /// Idiom/Collocation mix ID
        /// </summary>
        [Required]
        public int ID { get; set; }

        /// <summary>
        /// Idiom/Collocation associated word ID
        /// </summary>
        [Required]
        public int Word { get; set; }
    }
}