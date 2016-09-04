using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VitEgoDictionary.Models.Parameters
{
    /// <summary>
    /// Passes data from Create/Edit Collocation/Idiom views to appropriate controller methods.
    /// </summary>
    public class CollocationIdiomParameter : ItemParameter
    {
        /// <summary>
        /// Collocation or Idiom name.
        /// </summary>
        [Required]
        public string Name { get; set; }
        
        /// <summary>
        /// Collocation or Idiom collection of associated words.
        /// </summary>
        public IEnumerable<MixParameter> Mixes { get; set; }
    }
}