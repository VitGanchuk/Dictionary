using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models.Utilities
{
    /// <summary>
    /// The lexical item to determine whether or not its text should be highlighted
    /// </summary>
    public class LexicalItem
    {
        /// <summary>
        /// The text to be shown
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// The flag whether or not the text should be highlighted
        /// </summary>
        public bool IsHighlighted { get; private set; }

        /// <summary>
        /// Initializes a new LexicalItem object
        /// </summary>
        /// <param name="text">The text to be shown</param>
        public LexicalItem(string text)
        {
            Text = text;
            IsHighlighted = false;
        }

        /// <summary>
        /// Set the item highlighted
        /// </summary>
        public void SetHighlighted()
        {
            IsHighlighted = true;
        }
    }
}