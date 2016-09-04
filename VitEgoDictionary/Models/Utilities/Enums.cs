using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models.Utilities
{
    public enum Item
    {
        Undefined = 0,
        Word = 1,
        PhrasalVerb = 2,
        Collocation = 3,
        Idiom = 4
    }

    public enum WordSpeechPart
    {
        Undefined,
        Noun = 1,
        Verb = 2,
        Adjective = 3,
        Adverb = 4,
        Others = 5
    }
}