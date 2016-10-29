using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VitEgoDictionary.Models;
using VitEgoDictionary.Models.Utilities;

namespace VitEgoDictionary.Controllers
{
    public class DataController : DictionaryController
    {        
        [HttpGet]
        public JsonResult Words(int speechPart = (int)WordSpeechPart.Undefined)
        {
            if(speechPart == (int)WordSpeechPart.Undefined)
                return Json(_entities.Words.OrderBy(i => i.Name).
                    Select(i => new {
                        id = i.ID, 
                        name = i.Name, 
                        speechPart = i.SpeechPart.ShortName,
                        item = (int)Item.Word
                    }), JsonRequestBehavior.AllowGet);
            else 
                return Json(_entities.Words.Where(i => i.IDF_SpeechPart == speechPart).OrderBy(i => i.Name).
                    Select(i => new {
                        id = i.ID, 
                        name = i.Name, 
                        speechPart = i.SpeechPart.ShortName,
                        item = (int)Item.Word
                    }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Synonyms(int speechPart = (int)WordSpeechPart.Undefined)
        {
            IEnumerable<Synonym> synonyms;
            if (speechPart == (int)WordSpeechPart.Undefined)
            {
                synonyms = _entities.Words.OrderBy(i => i.Name).
                    Select(i => new Synonym()
                    {
                        ID = i.ID,
                        Name = i.Name,
                        SpeechPart = i.SpeechPart.ShortName,
                        Item = (int)Item.Word
                    });
            }
            else
            {
                synonyms = _entities.Words.Where(i => i.IDF_SpeechPart == speechPart).OrderBy(i => i.Name).
                    Select(i => new Synonym()
                    {
                        ID = i.ID,
                        Name = i.Name,
                        SpeechPart = i.SpeechPart.ShortName,
                        Item = (int)Item.Word
                    });

            }

            if (speechPart == (int)WordSpeechPart.Undefined || speechPart == (int)WordSpeechPart.Verb)
            {
                synonyms = synonyms.Concat(_entities.PhrasalVerbs.OrderBy(i => i.Name).
                    Select(i => new Synonym()
                    {
                        ID = i.ID,
                        Name = i.Verb.Name + " " + i.Preposition.Name,
                        SpeechPart = i.Verb.SpeechPart.ShortName,
                        Item = (int)Item.PhrasalVerb
                    }));
            }

            synonyms = synonyms.Concat(_entities.Collocations.
                Select(i => new Synonym()
                {
                    ID = i.ID,
                    Name = i.Name,
                    SpeechPart = String.Empty,
                    Item = (int)Item.Collocation
                }));

            synonyms = synonyms.Concat(_entities.Idioms.
                Select(i => new Synonym()
                {
                    ID = i.ID,
                    Name = i.Name,
                    SpeechPart = String.Empty,
                    Item = (int)Item.Idiom
                }));

            return Json(synonyms.
                    Select(i => new
                    {
                        id = i.ID,
                        name = i.Name,
                        speechPart = i.SpeechPart,
                        item = i.Item
                    }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GrammarStructures(int speechPart = (int)WordSpeechPart.Undefined)
        {
            if (speechPart == (int)WordSpeechPart.Undefined)
                return Json(_entities.GrammarStructures.Where(i => i.IsAuxiliary == false).
                    Select(i => new { 
                        id = i.ID, 
                        name = i.Name 
                    }), JsonRequestBehavior.AllowGet);
            else
                return Json(_entities.GrammarStructures.Where(i => i.IDF_SpeechPart == speechPart && i.IsAuxiliary == false).
                    Select(i => new { 
                        id = i.ID, 
                        name = i.Name 
                    }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Meanings(int id, int item)
        {
            if (id > 0 && item > 0)
            {
                switch (item)
                {
                    case (int)Item.Word:
                        return Json(_entities.WordMeanings.Where(i => i.IDF_Word == id).OrderBy(i => i.Meaning).
                             Select(i => new
                             {
                                 id = i.ID,
                                 name = i.Meaning,
                                 item = item
                             }), JsonRequestBehavior.AllowGet);
                    case (int)Item.PhrasalVerb:
                        return Json(_entities.PhrasalVerbMeanings.Where(i => i.IDF_PhrasalVerb == id).OrderBy(i => i.Meaning).
                             Select(i => new
                             {
                                 id = i.ID,
                                 name = i.Meaning,
                                 item = item
                             }), JsonRequestBehavior.AllowGet);
                    case (int)Item.Collocation:
                        return Json(_entities.CollocationMeanings.Where(i => i.IDF_Collocation == id).OrderBy(i => i.Meaning).
                             Select(i => new
                             {
                                 id = i.ID,
                                 name = i.Meaning,
                                 item = item
                             }), JsonRequestBehavior.AllowGet);
                    case (int)Item.Idiom:
                        return Json(_entities.IdiomMeanings.Where(i => i.IDF_Idiom == id).OrderBy(i => i.Meaning).
                             Select(i => new
                             {
                                 id = i.ID,
                                 name = i.Meaning,
                                 item = item
                             }), JsonRequestBehavior.AllowGet);
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        [HttpGet]
        public JsonResult Prepositions()
        {
            return Json(_entities.Prepositions.OrderBy(i => i.Name).
                Select(i => new 
                { 
                    id = i.ID, 
                    name = i.Name 
                }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult PhrasalVerbs()
        {
            return Json(_entities.PhrasalVerbs.OrderBy(i => i.Verb.Name).ThenBy(i => i.Preposition.Name).ToList().
                Select(i => new
                {
                    id = i.ID,
                    name = i.Name
                }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SpeechParts()
        {
            return Json(_entities.SpeechParts.
                Select(i => new
                {
                    id = i.ID,
                    name = i.Name
                }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Idioms()
        {
            return Json(_entities.Idioms.OrderBy(i => i.Name).
                    Select(i => new 
                    { 
                        id = i.ID, 
                        name = i.Name 
                    }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Collocations()
        {
            return Json(_entities.Collocations.OrderBy(i => i.Name).
                    Select(i => new 
                    { 
                        id = i.ID, 
                        name = i.Name 
                    }), JsonRequestBehavior.AllowGet);
        }
    }
}
