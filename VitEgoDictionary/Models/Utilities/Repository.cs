using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VitEgoDictionary.Models.Utilities
{
    public static class Repository
    {
        public static void SetSessionVariables(HttpSessionStateBase session, VitEgoDictionary.Models.DictionaryEntities entities)
        {
            if (session["Topics"] == null) session["Topics"] = entities.Topics.OrderBy(i => i.Name);
            if (session["SpeechParts"] == null) session["SpeechParts"] = entities.SpeechParts.OrderBy(i => i.ID);
            if (session["Formalities"] == null) session["Formalities"] = entities.Formalities.OrderBy(i => i.ID);
            if (session["Countabilities"] == null) session["Countabilities"] = entities.Countabilities.OrderBy(i => i.ID);
            if (session["Prepositions"] == null) session["Prepositions"] = entities.Prepositions.OrderBy(i => i.Name);
        }
    }
}