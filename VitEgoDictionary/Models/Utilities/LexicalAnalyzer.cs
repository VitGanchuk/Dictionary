using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VitEgoDictionary.Models.Utilities;

namespace VitEgoDictionary.Models
{
    public static class LexicalAnalyzer
    {
        static char[] punctuation = { '.', ',', ';', ':', '?', '!', '\'', '`', '(', ')', '[', ']', '{', '}', '-' };
        
        /// <summary>
        /// Splits a string into a list of lexical items separated by whitespaces and/or any punctuation characters; 
        /// unlike the ordinary String.Split() method, those punctuation characters are also included to the returned list;
        /// </summary>
        /// <param name="exampleSentense">The input string</param>
        /// <returns>The list of lexical items separated by whitespaces and/or any punctuation characters; 
        /// unlike the ordinary String.Split() method, those punctuation characters are also included to the returned list</returns>
        public static List<LexicalItem> Split(string exampleSentense)
        {
            List<LexicalItem> lexicalItems = new List<LexicalItem>();
            if (String.IsNullOrEmpty(exampleSentense)) return lexicalItems;
            //For every word in the sentense
            foreach (var word in exampleSentense.Split(' ')) {
                //If the word contains any punctuation
                if (word.Any(i => punctuation.Contains(i))) {
                    string subword = String.Empty;
                    //For every character in the word
                    foreach (var character in word.ToCharArray()) {
                        //If the character is a punctuation
                        if (punctuation.Contains(character)) {
                            //If the subword isn't empty, it's added to the returned list
                            if (!String.IsNullOrEmpty(subword)) {
                                lexicalItems.Add(new LexicalItem(subword));
                                subword = String.Empty;
                            }
                            //The character, which is a punctuation, is added to the returned list
                            lexicalItems.Add(new LexicalItem(character.ToString()));
                        }
                            //If the character is a letter, it's added to the subword
                        else subword += character;
                    }
                    //If the subword isn't empty at the end of the loop, the subbword is added to the returned list
                    if (!String.IsNullOrEmpty(subword)) {
                        lexicalItems.Add(new LexicalItem(subword));
                        subword = String.Empty;
                    }
                }
                //If the word doesn't contain any punctuation, it's added to the returned list
                else lexicalItems.Add(new LexicalItem(word));
            }
            return lexicalItems;
        }

        /// <summary>
        /// Sets particular items in the list of lexicals items highlighted
        /// </summary>
        /// <param name="lexicalItems">A list of lexixal items</param>
        /// <param name="word">The current word from the dictionary to be highlighted</param>
        /// <param name="preposition">The preposition linked with the current word to be highlighted</param>
        /// <returns>The list of lexical items with some of them highlighted</returns>
        /// <remarks>As a rule, a verb is followed by prepositions but not vice versa.
        /// So, the needed preposition to be highlighted is the first after the verb</remarks>
        public static List<LexicalItem> Highlight(string exampleSentense, Word word)
        {
            List<LexicalItem> lexicalItems = LexicalAnalyzer.Split(exampleSentense);
            foreach (var item in lexicalItems) {
                if (word.Name == item.Text.ToLower() || word.Variations.Any(i => i.Name == item.Text.ToLower())) {
                    item.SetHighlighted();
                }
            }
            return lexicalItems;
        }

        /// <summary>
        /// Sets particular items in the list of lexicals items highlighted
        /// </summary>
        /// <param name="lexicalItems">A list of lexixal items</param>
        /// <param name="prepositionMix">The combination of a word and a preposition to be highlighted</param>
        /// <returns>The list of lexical items with some of them highlighted</returns>
        /// <remarks>As a rule, a verb is followed by prepositions, but not vice versa.
        ///         So, the needed preposition to be highlighted is the first one after the verb</remarks>
        public static List<LexicalItem> Highlight(string exampleSentense, PrepositionMix prepositionMix)
        {
            List<LexicalItem> lexicalItems = LexicalAnalyzer.Split(exampleSentense);
           
            bool canHighlightPreposition = false;
            foreach (var item in lexicalItems)
            {
                if (prepositionMix.Word.Name == item.Text.ToLower() ||
                    prepositionMix.Word.Variations.Any(i => i.Name == item.Text.ToLower()))
                {
                    item.SetHighlighted();
                    canHighlightPreposition = true;
                }
                if (canHighlightPreposition)
                {
                    if (prepositionMix.Preposition.Name == item.Text.ToLower())
                    {
                        item.SetHighlighted();
                        canHighlightPreposition = false;
                    }
                }
            }
            return lexicalItems;
        }

        /// <summary>
        /// Sets particular items in the list of lexicals items highlighted
        /// </summary>
        /// <param name="lexicalItems">A list of lexixal items</param>
        /// <param name="phrasalVerb">The combination of a word and one/two prepositions to be highlighted</param>
        /// <returns>The list of lexical items with some of them highlighted</returns>
        /// <remarks>As a rule, a verb is followed by prepositions, but not vice versa.
        ///         So, the needed preposition to be highlighted is the first one after the verb.</remarks>
        public static List<LexicalItem> Highlight(string exampleSentense, PhrasalVerb phrasalVerb)
        {
            List<LexicalItem> lexicalItems = LexicalAnalyzer.Split(exampleSentense);
            
            bool canHighlightPreposition = false;
            foreach (var item in lexicalItems)
            {
                if (phrasalVerb.Verb.Name == item.Text.ToLower() ||
                    phrasalVerb.Verb.Variations.Any(i => i.Name == item.Text.ToLower()))
                {
                    item.SetHighlighted();
                    canHighlightPreposition = true;
                }
                if (canHighlightPreposition)
                {
                    if (phrasalVerb.Preposition.Name == item.Text.ToLower())
                    {
                        item.SetHighlighted();
                        canHighlightPreposition = false;
                    }
                }
            }
            return lexicalItems;
        }

        /// <summary>
        /// Sets particular items in the list of lexicals items highlighted
        /// </summary>
        /// <param name="lexicalItems">A list of lexixal items</param>
        /// <param name="phrase">The phrase to be highlighted</param>
        /// <returns>The list of lexical items with some of them highlighted</returns>
        public static List<LexicalItem> Highlight(string exampleSentense, string phrase)
        {
            List<LexicalItem> lexicalItems = LexicalAnalyzer.Split(exampleSentense);
           
            string[] wordsToBeHighlighted = phrase.Split(' ');
            
            foreach (var item in lexicalItems)
            {
                DictionaryEntities entities = new DictionaryEntities();
                IEnumerable<Word> words = entities.Words.Where(i => i.Name == item.Text.ToLower() || i.Variations.Any(j => j.Name == item.Text.ToLower()));
                if (words.Count() > 0) {
                    foreach (var word in words) {
                        if (wordsToBeHighlighted.Contains(word.Name)) {
                            item.SetHighlighted();
                        } else {
                            foreach (var variation in word.Variations) {
                                if (wordsToBeHighlighted.Contains(variation.Name)) {
                                    item.SetHighlighted();
                                }
                            }
                        }                   
                    }
                } else {
                    if (wordsToBeHighlighted.Contains(item.Text.ToLower())) {
                        item.SetHighlighted();
                    }
                }
            }
            return lexicalItems;
        }
    }
}

