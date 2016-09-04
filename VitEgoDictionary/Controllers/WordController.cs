using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VitEgoDictionary.Models;
using VitEgoDictionary.Models.ViewModels;
using VitEgoDictionary.Models.Parameters;
using VitEgoDictionary.Models.Extensions;
using VitEgoDictionary.Models.Utilities;

namespace VitEgoDictionary.Controllers
{
    public class WordController : DictionaryController
    {
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Index()
        {
            ItemListParameters parameters = new ItemListParameters()
            {
                Destination = Session["WordSearchDestination"] == null ? 0 : (int)Session["WordSearchDestination"],
                Topics = Session["WordSearchTopics"] == null ? null : Session["WordSearchTopics"] as IEnumerable<int>,
                Formalities = Session["WordSearchFormalities"] == null ? null : Session["WordSearchFormalities"] as IEnumerable<int>,
                SpeechParts = Session["WordSearchSpeechParts"] == null ? null : Session["WordSearchSpeechParts"] as IEnumerable<int>,
                BaseVerb = -1
            };
            return View(new ItemListViewModel(Models.Utilities.Item.Word, parameters, this.Request.IsAuthenticated, this.HttpContext.User, _entities));
        }

        [ChildActionOnly]
        public ActionResult SearchPanel()
        {
            Repository.SetSessionVariables(this.Session, this._entities);
            return PartialView();
        }

        [ChildActionOnly]
        public ActionResult SearchPanelSimple() {
            return PartialView();
        }

        [HttpPost]
        public JsonResult LoadItems(ItemListParameters parameters)
        {
            //  Saves the seasrch parameters in Session 
            if(parameters != null)
            {
                Session["WordSearchDestination"] = parameters.Destination;
                Session["WordSearchTopics"] = parameters.Topics == null ? null : parameters.Topics;
                Session["WordSearchFormalities"] = parameters.Formalities == null ? null : parameters.Formalities;
                Session["WordSearchSpeechParts"] = parameters.SpeechParts == null ? null : parameters.SpeechParts;
            }
            ItemListViewModel model = new ItemListViewModel(Models.Utilities.Item.Word, parameters, this.Request.IsAuthenticated, this.HttpContext.User, _entities);
            IEnumerable<Word> words = model.Items as IEnumerable<Word>;
            return Json(new
            {
                page = model.Page,
                pagesToDisplay = model.PagesToDisplay,
                isFiltered = model.isFiltered,
                overallRecords = model.OverallRecords,
                overallPages = model.OverallPages,
                items = words.
                    Select(i => new
                    {
                        id = i.ID,
                        name = i.Name,
                        topic = i.Topic.Name,
                        speechPart = i.SpeechPart.ShortName,
                        meanings = i.Meanings.OrderBy(j => j.Meaning).Select(j => new
                        {
                            meaning = j.Meaning
                        })
                    })
            });
        }

        [HttpGet]
        public ActionResult Item(int id)
        {
            if (this.Request.IsAjaxRequest())
            {
                return Json(new { result = "redirect", url = Url.Action("Item", "Word", new { id = id }) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return View(new ItemViewModel(this.Request.UrlReferrer, VitEgoDictionary.Models.Utilities.Item.Word, id, this.Request.IsAuthenticated, this.HttpContext.User, this._entities));
            }
        }

        [HttpGet]
        [Authorize(Roles = "Creator")]
        public ActionResult Create()
        {
            Repository.SetSessionVariables(this.Session, this._entities);
            return View(new UrlReferrerViewModel(this.Request.UrlReferrer, this.Request.IsAuthenticated, this.HttpContext.User, this._entities));
        }

        [HttpPost]
        [Authorize(Roles = "Creator")]
        public ActionResult Create(WordParameter parameters)
        {
            if (ModelState.IsValid)
            {
                #region Word
                Word word = new Word()
                {
                    Name = parameters.Name,
                    IDF_SpeechPart = parameters.SpeechPart,
                    IDF_Topic = parameters.Topic,
                    IDF_Formality = parameters.Formality,
                    IDF_Countability = parameters.Coutability,
                    IDF_GenericWord = parameters.GenericWord > 0 ? parameters.GenericWord : null
                };
                #endregion
                #region Variations
                if (parameters.Variations != null)
                {
                    //  Gets rid of NULL values
                    parameters.Variations = parameters.Variations.Except(parameters.Variations.Where(i => i == null));
                    foreach (var paramVariation in parameters.Variations)
                    {
                        Variation variation = new Variation()
                        {
                            IDF_GrammarStructure = paramVariation.GrammarStructure,
                            Name = paramVariation.Name
                        };
                        word.Variations.Add(variation);
                    }
                }
                #endregion
                #region Meanings
                if (parameters.Meanings != null)
                {
                    foreach (var paramMeaning in parameters.Meanings)
                    {
                        WordMeaning meaning = new WordMeaning() { Meaning = paramMeaning.Meaning };
                        if (paramMeaning.Examples != null)
                        {
                            foreach (var paramMeaningExample in paramMeaning.Examples)
                            {
                                meaning.Examples.Add(new WordMeaningExample() { Example = paramMeaningExample.Example });
                            }
                        }
                        #region Synonyms
                        if (paramMeaning.Synonyms != null)
                        {
                            //  Checks all word synonyms that should be saved
                            foreach (var paramSynonym in paramMeaning.Synonyms)
                            {
                                switch (paramSynonym.ItemType)
                                {
                                    #region Word
                                    case (int)Models.Utilities.Item.Word:
                                        WordMeaning wordSynonymMeaning = _entities.WordMeanings.FirstOrDefault(i => i.ID == paramSynonym.Meaning);
                                        //  Case 1: Both the current meaning and the synonym meaning have no SynonymSets
                                        if (meaning.SynonymSet == null && wordSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet = new SynonymSet();
                                            meaning.SynonymSet.WordMeanings.Add(meaning);
                                            meaning.SynonymSet.WordMeanings.Add(wordSynonymMeaning);
                                        }
                                        //  Case 2: The current meaning has SynonymSet, but the synonym meaning doesn't
                                        else if (meaning.SynonymSet != null && wordSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet.WordMeanings.Add(wordSynonymMeaning);
                                        }
                                        //  Case 3: The current meaning doesn't have SynonymSet, but the synonym meaning does
                                        else if (meaning.SynonymSet == null && wordSynonymMeaning.SynonymSet != null)
                                        {
                                            wordSynonymMeaning.SynonymSet.WordMeanings.Add(meaning);
                                        }
                                        //  Case 4: Both the current meaning and the synonym meaning have SynonymSets
                                        else if (meaning.SynonymSet != null && wordSynonymMeaning.SynonymSet != null)
                                        {
                                            if (meaning.SynonymSet != wordSynonymMeaning.SynonymSet)
                                            {
                                                foreach (var item in meaning.SynonymSet.WordMeanings.ToList())
                                                {
                                                    wordSynonymMeaning.SynonymSet.WordMeanings.Add(item);
                                                }
                                                foreach (var item in meaning.SynonymSet.PhrasalVerbMeanings.ToList())
                                                {
                                                    wordSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(item);
                                                }
                                                foreach (var item in meaning.SynonymSet.CollocationMeanings.ToList())
                                                {
                                                    wordSynonymMeaning.SynonymSet.CollocationMeanings.Add(item);
                                                }
                                                foreach (var item in meaning.SynonymSet.IdiomMeanings.ToList())
                                                {
                                                    wordSynonymMeaning.SynonymSet.IdiomMeanings.Add(item);
                                                }
                                                meaning.SynonymSet = wordSynonymMeaning.SynonymSet;
                                            }
                                        }
                                        break;
                                    #endregion
                                    #region Phrasal Verb
                                    case (int)Models.Utilities.Item.PhrasalVerb:
                                        PhrasalVerbMeaning phrasalVerbSynonymMeaning = _entities.PhrasalVerbMeanings.FirstOrDefault(i => i.ID == paramSynonym.Meaning);
                                        //  Case 1: Both the current meaning and the synonym meaning have no SynonymSets
                                        if (meaning.SynonymSet == null && phrasalVerbSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet = new SynonymSet();
                                            meaning.SynonymSet.WordMeanings.Add(meaning);
                                            meaning.SynonymSet.PhrasalVerbMeanings.Add(phrasalVerbSynonymMeaning);
                                        }
                                        //  Case 2: The current meaning has SynonymSet, but the synonym meaning doesn't
                                        else if (meaning.SynonymSet != null && phrasalVerbSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet.PhrasalVerbMeanings.Add(phrasalVerbSynonymMeaning);
                                        }
                                        //  Case 3: The current meaning doesn't have SynonymSet, but the synonym meaning does
                                        else if (meaning.SynonymSet == null && phrasalVerbSynonymMeaning.SynonymSet != null)
                                        {
                                            phrasalVerbSynonymMeaning.SynonymSet.WordMeanings.Add(meaning);
                                        }
                                        //  Case 4: Both the current meaning and the synonym meaning have SynonymSets
                                        else if (meaning.SynonymSet != null && phrasalVerbSynonymMeaning.SynonymSet != null)
                                        {
                                            foreach (var item in meaning.SynonymSet.WordMeanings.ToList())
                                            {
                                                phrasalVerbSynonymMeaning.SynonymSet.WordMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.PhrasalVerbMeanings.ToList())
                                            {
                                                phrasalVerbSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.CollocationMeanings.ToList())
                                            {
                                                phrasalVerbSynonymMeaning.SynonymSet.CollocationMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.IdiomMeanings.ToList())
                                            {
                                                phrasalVerbSynonymMeaning.SynonymSet.IdiomMeanings.Add(item);
                                            }
                                            meaning.SynonymSet = phrasalVerbSynonymMeaning.SynonymSet;

                                        }
                                        break;
                                    #endregion
                                    #region Collocation
                                    case (int)Models.Utilities.Item.Collocation:
                                        CollocationMeaning collocationSynonymMeaning = _entities.CollocationMeanings.FirstOrDefault(i => i.ID == paramSynonym.Meaning);
                                        //  Case 1: Both the current meaning and the synonym meaning have no SynonymSets
                                        if (meaning.SynonymSet == null && collocationSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet = new SynonymSet();
                                            meaning.SynonymSet.WordMeanings.Add(meaning);
                                            meaning.SynonymSet.CollocationMeanings.Add(collocationSynonymMeaning);
                                        }
                                        //  Case 2: The current meaning has SynonymSet, but the synonym meaning doesn't
                                        else if (meaning.SynonymSet != null && collocationSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet.CollocationMeanings.Add(collocationSynonymMeaning);
                                        }
                                        //  Case 3: The current meaning doesn't have SynonymSet, but the synonym meaning does
                                        else if (meaning.SynonymSet == null && collocationSynonymMeaning.SynonymSet != null)
                                        {
                                            collocationSynonymMeaning.SynonymSet.WordMeanings.Add(meaning);
                                        }
                                        //  Case 4: Both the current meaning and the synonym meaning have SynonymSets
                                        else if (meaning.SynonymSet != null && collocationSynonymMeaning.SynonymSet != null)
                                        {
                                            foreach (var item in meaning.SynonymSet.WordMeanings.ToList())
                                            {
                                                collocationSynonymMeaning.SynonymSet.WordMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.PhrasalVerbMeanings.ToList())
                                            {
                                                collocationSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.CollocationMeanings.ToList())
                                            {
                                                collocationSynonymMeaning.SynonymSet.CollocationMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.IdiomMeanings.ToList())
                                            {
                                                collocationSynonymMeaning.SynonymSet.IdiomMeanings.Add(item);
                                            }
                                            meaning.SynonymSet = collocationSynonymMeaning.SynonymSet;

                                        }
                                        break;
                                    #endregion
                                    #region Idiom
                                    case (int)Models.Utilities.Item.Idiom:
                                        IdiomMeaning idiomSynonymMeaning = _entities.IdiomMeanings.FirstOrDefault(i => i.ID == paramSynonym.Meaning);
                                        //  Case 1: Both the current meaning and the synonym meaning have no SynonymSets
                                        if (meaning.SynonymSet == null && idiomSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet = new SynonymSet();
                                            meaning.SynonymSet.WordMeanings.Add(meaning);
                                            meaning.SynonymSet.IdiomMeanings.Add(idiomSynonymMeaning);
                                        }
                                        //  Case 2: The current meaning has SynonymSet, but the synonym meaning doesn't
                                        else if (meaning.SynonymSet != null && idiomSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet.IdiomMeanings.Add(idiomSynonymMeaning);
                                        }
                                        //  Case 3: The current meaning doesn't have SynonymSet, but the synonym meaning does
                                        else if (meaning.SynonymSet == null && idiomSynonymMeaning.SynonymSet != null)
                                        {
                                            idiomSynonymMeaning.SynonymSet.WordMeanings.Add(meaning);
                                        }
                                        //  Case 4: Both the current meaning and the synonym meaning have SynonymSets
                                        else if (meaning.SynonymSet != null && idiomSynonymMeaning.SynonymSet != null)
                                        {
                                            foreach (var item in meaning.SynonymSet.WordMeanings.ToList())
                                            {
                                                idiomSynonymMeaning.SynonymSet.WordMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.PhrasalVerbMeanings.ToList())
                                            {
                                                idiomSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.CollocationMeanings.ToList())
                                            {
                                                idiomSynonymMeaning.SynonymSet.CollocationMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.IdiomMeanings.ToList())
                                            {
                                                idiomSynonymMeaning.SynonymSet.IdiomMeanings.Add(item);
                                            }
                                            meaning.SynonymSet = idiomSynonymMeaning.SynonymSet;
                                        }
                                        break;
                                    #endregion
                                }
                            }
                        }
                        #endregion
                        word.Meanings.Add(meaning);  
                    }
                }
                #endregion
                #region Prepositions
                if (parameters.PrepositionMix != null)
                {
                    foreach (var paramPrepositionMix in parameters.PrepositionMix)
                    {
                        PrepositionMix prepositionMix = new PrepositionMix() { IDF_Preposition = paramPrepositionMix.Preposition };
                        foreach (var prepositionMixExample in paramPrepositionMix.Examples)
                        {
                            prepositionMix.Examples.Add(new PrepositionMixExample() { Example = prepositionMixExample.Example });
                        }
                        word.PrepositionMixes.Add(prepositionMix);
                    }
                }
                #endregion
                _entities.Words.AddObject(word);
                if (_entities.ObjectStateManager.GetObjectStateEntries(
                    System.Data.EntityState.Modified |
                    System.Data.EntityState.Added |
                    System.Data.EntityState.Deleted).Count() > 0)
                {
                    try { _entities.SaveChanges(); }
                    catch (Exception ex)
                    {
                        return Json(new
                        {
                            result = "error",
                            message = ex.Message,
                            innerMessage = ex.InnerException == null ? null : ex.InnerException.Message
                        });
                    }
                }
                return Json(new { result = "redirect", url = Url.Action("Item", "Word", new { id = word.ID }) });
            }
            else { return Json(new { result = "error", message = "The model state is invalid" }); }     
        }

        [HttpGet]
        [Authorize(Roles="Creator")]
        public ActionResult Edit(int id)
        {
            Repository.SetSessionVariables(this.Session, this._entities);
            return View(new EditItemViewModel(this.Request.UrlReferrer, VitEgoDictionary.Models.Utilities.Item.Word, id, this.Request.IsAuthenticated, this.HttpContext.User, this._entities));
        }

        [HttpPost]
        [Authorize(Roles = "Creator")]
        public JsonResult Edit(WordParameter parameters)
        {
            if (ModelState.IsValid)
            {
                #region Word
                Word word = _entities.Words.FirstOrDefault(i => i.ID == parameters.ID);
                if (word == null) { return Json(new { result = "error", message = "The word is not found" }); }
                //  Replaces only what has been changed
                if (word.Name != parameters.Name) { word.Name = parameters.Name; }
                if (word.IDF_SpeechPart != parameters.SpeechPart) { word.IDF_SpeechPart = parameters.SpeechPart; }
                if (word.IDF_Topic != parameters.Topic) { word.IDF_Topic = parameters.Topic; }
                if (word.IDF_Formality != parameters.Formality) { word.IDF_Formality = parameters.Formality; }
                if (parameters.SpeechPart != (int)WordSpeechPart.Noun) word.IDF_Countability = null;
                else if (word.IDF_Countability != parameters.Coutability) { word.IDF_Countability = parameters.Coutability; }
                if (word.IDF_GenericWord != parameters.GenericWord) { word.IDF_GenericWord = parameters.GenericWord > 0 ? parameters.GenericWord : null; }
                #endregion
                #region Variations
                if (parameters.Variations == null)
                {
                    foreach (var variation in word.Variations.ToList())
                    {
                        word.Variations.Remove(variation);
                        _entities.Variations.DeleteObject(variation);
                    }
                }
                else
                {
                    //  Gets rid of NULL values
                    parameters.Variations = parameters.Variations.Except(parameters.Variations.Where(i => i == null));
                    //  Removes all variations that have been deleted by the user
                    foreach (var variation in word.Variations.Where(i => !parameters.Variations.Any(j => j.GrammarStructure == i.IDF_GrammarStructure)).ToList())
                    {
                        word.Variations.Remove(variation);
                        _entities.Variations.DeleteObject(variation);
                    }
                }
                //  Checks all variations that should be saved
                if (parameters.Variations != null)
                {
                    foreach (var paramVariation in parameters.Variations)
                    {
                        Variation variation = word.Variations.FirstOrDefault(i => i.IDF_GrammarStructure == paramVariation.GrammarStructure);
                        //  This is a new variation
                        if (variation == null)
                        {
                            variation = new Variation()
                            {
                                IDF_GrammarStructure = paramVariation.GrammarStructure,
                                Name = paramVariation.Name
                            };
                            word.Variations.Add(variation);
                        }
                        //This is an existent variation
                        else if (variation.Name != paramVariation.Name) { variation.Name = paramVariation.Name; } 
                    }
                }
                #endregion
                #region Meanings
                //  Removes all meanings that have been deleted by the user
                if (parameters.Meanings == null)
                {
                    foreach (var meaning in word.Meanings.ToList())
                    {
                        _entities.WordMeanings.DeleteObject(meaning);
                    }
                }
                else
                {
                    foreach (var meaning in word.Meanings.Where(i => !parameters.Meanings.Any(j => j.ID == i.ID)).ToList())
                    {
                        _entities.WordMeanings.DeleteObject(meaning);
                    }
                }
                //  Checks all meanings that should be saved
                if (parameters.Meanings != null)
                {
                    foreach (var paramMeaning in parameters.Meanings)
                    {
                        WordMeaning meaning = word.Meanings.FirstOrDefault(i => i.ID == paramMeaning.ID);
                        bool isMeaningNew = false;
                        #region New Meaning
                        if (meaning == null)
                        {
                            isMeaningNew = true;
                            meaning = new WordMeaning() { Meaning = paramMeaning.Meaning };
                            if (paramMeaning.Examples != null)
                            {
                                foreach (var paramMeaningExample in paramMeaning.Examples)
                                {
                                    meaning.Examples.Add(new WordMeaningExample() { Example = paramMeaningExample.Example });
                                }
                            }
                        }
                        #endregion
                        #region Existing Meaning
                        else
                        {
                            if (meaning.Meaning != paramMeaning.Meaning)
                            {
                                meaning.Meaning = paramMeaning.Meaning;
                            }
                            //  Removes all meaning examples that have been deleted by the user
                            if (paramMeaning.Examples == null)
                            {
                                foreach (var meaningExample in meaning.Examples.ToList())
                                {
                                    _entities.WordMeaningExamples.DeleteObject(meaningExample);
                                }
                            }
                            else
                            {
                                foreach (var meaningExample in meaning.Examples.Where(i => !paramMeaning.Examples.Any(j => j.ID == i.ID)).ToList())
                                {
                                    _entities.WordMeaningExamples.DeleteObject(meaningExample);
                                }
                            }
                            if (paramMeaning.Examples != null)
                            {
                                //  Checks all meaning examples that should be saved
                                foreach (var paramMeaningExample in paramMeaning.Examples)
                                {
                                    WordMeaningExample meaningExample = meaning.Examples.FirstOrDefault(i => i.ID == paramMeaningExample.ID);
                                    //  This is a new meaning example
                                    if (meaningExample == null)
                                    {
                                        meaningExample = new WordMeaningExample() { Example = paramMeaningExample.Example };
                                        meaning.Examples.Add(meaningExample);
                                    }
                                    //This is an existent meaning example
                                    else
                                    {
                                        if (meaningExample.Example != paramMeaningExample.Example) { meaningExample.Example = paramMeaningExample.Example; }
                                    }
                                }
                            }
                            //  Removes all synonyms that have been deleted by the user
                            if (meaning.SynonymSet != null)
                            {
                                if (paramMeaning.Synonyms == null)
                                {
                                    _entities.SynonymSets.DeleteObject(meaning.SynonymSet);
                                }
                                else
                                {
                                    foreach (var synonymMeaning in meaning.SynonymSet.WordMeanings
                                        .Where(i => !paramMeaning.Synonyms.
                                            Any(j => j.Meaning == i.ID &&
                                                j.ItemType == (int)Models.Utilities.Item.Word) &&
                                                i.ID != meaning.ID).ToList())
                                    { meaning.SynonymSet.WordMeanings.Remove(synonymMeaning); }
                                    foreach (var synonymMeaning in meaning.SynonymSet.PhrasalVerbMeanings
                                        .Where(i => !paramMeaning.Synonyms.
                                            Any(j => j.Meaning == i.ID &&
                                                j.ItemType == (int)Models.Utilities.Item.PhrasalVerb)).ToList())
                                    { meaning.SynonymSet.PhrasalVerbMeanings.Remove(synonymMeaning); }
                                    foreach (var synonymMeaning in meaning.SynonymSet.CollocationMeanings
                                       .Where(i => !paramMeaning.Synonyms.
                                           Any(j => j.Meaning == i.ID &&
                                               j.ItemType == (int)Models.Utilities.Item.Collocation)).ToList())
                                    { meaning.SynonymSet.CollocationMeanings.Remove(synonymMeaning); }
                                    foreach (var synonymMeaning in meaning.SynonymSet.IdiomMeanings
                                        .Where(i => !paramMeaning.Synonyms.
                                            Any(j => j.Meaning == i.ID &&
                                                j.ItemType == (int)Models.Utilities.Item.Idiom)).ToList())
                                    { meaning.SynonymSet.IdiomMeanings.Remove(synonymMeaning); }
                                }
                            }
                        }
                        #endregion
                        #region Synonyms
                        if (paramMeaning.Synonyms != null)
                        {
                            //  Checks all word synonyms that should be saved
                            foreach (var paramSynonym in paramMeaning.Synonyms)
                            {
                                switch (paramSynonym.ItemType)
                                {
                                    #region Word
                                    case (int)Models.Utilities.Item.Word:
                                        WordMeaning wordSynonymMeaning = _entities.WordMeanings.FirstOrDefault(i => i.ID == paramSynonym.Meaning);
                                        //  Case 1: Both the current meaning and the synonym meaning have no SynonymSets
                                        if (meaning.SynonymSet == null && wordSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet = new SynonymSet();
                                            meaning.SynonymSet.WordMeanings.Add(meaning);
                                            meaning.SynonymSet.WordMeanings.Add(wordSynonymMeaning);
                                        }
                                        //  Case 2: The current meaning has SynonymSet, but the synonym meaning doesn't
                                        else if (meaning.SynonymSet != null && wordSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet.WordMeanings.Add(wordSynonymMeaning);
                                        }
                                        //  Case 3: The current meaning doesn't have SynonymSet, but the synonym meaning does
                                        else if (meaning.SynonymSet == null && wordSynonymMeaning.SynonymSet != null)
                                        {
                                            wordSynonymMeaning.SynonymSet.WordMeanings.Add(meaning);
                                        }
                                        //  Case 4: Both the current meaning and the synonym meaning have SynonymSets
                                        else if (meaning.SynonymSet != null && wordSynonymMeaning.SynonymSet != null)
                                        {
                                            if (meaning.SynonymSet != wordSynonymMeaning.SynonymSet)
                                            {
                                                foreach (var item in meaning.SynonymSet.WordMeanings.ToList())
                                                {
                                                    wordSynonymMeaning.SynonymSet.WordMeanings.Add(item);
                                                }
                                                foreach (var item in meaning.SynonymSet.PhrasalVerbMeanings.ToList())
                                                {
                                                    wordSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(item);
                                                }
                                                foreach (var item in meaning.SynonymSet.CollocationMeanings.ToList())
                                                {
                                                    wordSynonymMeaning.SynonymSet.CollocationMeanings.Add(item);
                                                }
                                                foreach (var item in meaning.SynonymSet.IdiomMeanings.ToList())
                                                {
                                                    wordSynonymMeaning.SynonymSet.IdiomMeanings.Add(item);
                                                }
                                                meaning.SynonymSet = wordSynonymMeaning.SynonymSet;
                                            }
                                        }
                                        break;
                                    #endregion
                                    #region Phrasal Verb
                                    case (int)Models.Utilities.Item.PhrasalVerb:
                                        PhrasalVerbMeaning phrasalVerbSynonymMeaning = _entities.PhrasalVerbMeanings.FirstOrDefault(i => i.ID == paramSynonym.Meaning);
                                        //  Case 1: Both the current meaning and the synonym meaning have no SynonymSets
                                        if (meaning.SynonymSet == null && phrasalVerbSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet = new SynonymSet();
                                            meaning.SynonymSet.WordMeanings.Add(meaning);
                                            meaning.SynonymSet.PhrasalVerbMeanings.Add(phrasalVerbSynonymMeaning);
                                        }
                                        //  Case 2: The current meaning has SynonymSet, but the synonym meaning doesn't
                                        else if (meaning.SynonymSet != null && phrasalVerbSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet.PhrasalVerbMeanings.Add(phrasalVerbSynonymMeaning);
                                        }
                                        //  Case 3: The current meaning doesn't have SynonymSet, but the synonym meaning does
                                        else if (meaning.SynonymSet == null && phrasalVerbSynonymMeaning.SynonymSet != null)
                                        {
                                            phrasalVerbSynonymMeaning.SynonymSet.WordMeanings.Add(meaning);
                                        }
                                        //  Case 4: Both the current meaning and the synonym meaning have SynonymSets
                                        else if (meaning.SynonymSet != null && phrasalVerbSynonymMeaning.SynonymSet != null)
                                        {
                                            foreach (var item in meaning.SynonymSet.WordMeanings.ToList())
                                            {
                                                phrasalVerbSynonymMeaning.SynonymSet.WordMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.PhrasalVerbMeanings.ToList())
                                            {
                                                phrasalVerbSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.CollocationMeanings.ToList())
                                            {
                                                phrasalVerbSynonymMeaning.SynonymSet.CollocationMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.IdiomMeanings.ToList())
                                            {
                                                phrasalVerbSynonymMeaning.SynonymSet.IdiomMeanings.Add(item);
                                            }
                                            meaning.SynonymSet = phrasalVerbSynonymMeaning.SynonymSet;

                                        }
                                        break;
                                    #endregion
                                    #region Collocation
                                    case (int)Models.Utilities.Item.Collocation:
                                        CollocationMeaning collocationSynonymMeaning = _entities.CollocationMeanings.FirstOrDefault(i => i.ID == paramSynonym.Meaning);
                                        //  Case 1: Both the current meaning and the synonym meaning have no SynonymSets
                                        if (meaning.SynonymSet == null && collocationSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet = new SynonymSet();
                                            meaning.SynonymSet.WordMeanings.Add(meaning);
                                            meaning.SynonymSet.CollocationMeanings.Add(collocationSynonymMeaning);
                                        }
                                        //  Case 2: The current meaning has SynonymSet, but the synonym meaning doesn't
                                        else if (meaning.SynonymSet != null && collocationSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet.CollocationMeanings.Add(collocationSynonymMeaning);
                                        }
                                        //  Case 3: The current meaning doesn't have SynonymSet, but the synonym meaning does
                                        else if (meaning.SynonymSet == null && collocationSynonymMeaning.SynonymSet != null)
                                        {
                                            collocationSynonymMeaning.SynonymSet.WordMeanings.Add(meaning);
                                        }
                                        //  Case 4: Both the current meaning and the synonym meaning have SynonymSets
                                        else if (meaning.SynonymSet != null && collocationSynonymMeaning.SynonymSet != null)
                                        {
                                            foreach (var item in meaning.SynonymSet.WordMeanings.ToList())
                                            {
                                                collocationSynonymMeaning.SynonymSet.WordMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.PhrasalVerbMeanings.ToList())
                                            {
                                                collocationSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.CollocationMeanings.ToList())
                                            {
                                                collocationSynonymMeaning.SynonymSet.CollocationMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.IdiomMeanings.ToList())
                                            {
                                                collocationSynonymMeaning.SynonymSet.IdiomMeanings.Add(item);
                                            }
                                            meaning.SynonymSet = collocationSynonymMeaning.SynonymSet;

                                        }
                                        break;
                                    #endregion
                                    #region Idiom
                                    case (int)Models.Utilities.Item.Idiom:
                                        IdiomMeaning idiomSynonymMeaning = _entities.IdiomMeanings.FirstOrDefault(i => i.ID == paramSynonym.Meaning);
                                        //  Case 1: Both the current meaning and the synonym meaning have no SynonymSets
                                        if (meaning.SynonymSet == null && idiomSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet = new SynonymSet();
                                            meaning.SynonymSet.WordMeanings.Add(meaning);
                                            meaning.SynonymSet.IdiomMeanings.Add(idiomSynonymMeaning);
                                        }
                                        //  Case 2: The current meaning has SynonymSet, but the synonym meaning doesn't
                                        else if (meaning.SynonymSet != null && idiomSynonymMeaning.SynonymSet == null)
                                        {
                                            meaning.SynonymSet.IdiomMeanings.Add(idiomSynonymMeaning);
                                        }
                                        //  Case 3: The current meaning doesn't have SynonymSet, but the synonym meaning does
                                        else if (meaning.SynonymSet == null && idiomSynonymMeaning.SynonymSet != null)
                                        {
                                            idiomSynonymMeaning.SynonymSet.WordMeanings.Add(meaning);
                                        }
                                        //  Case 4: Both the current meaning and the synonym meaning have SynonymSets
                                        else if (meaning.SynonymSet != null && idiomSynonymMeaning.SynonymSet != null)
                                        {
                                            foreach (var item in meaning.SynonymSet.WordMeanings.ToList())
                                            {
                                                idiomSynonymMeaning.SynonymSet.WordMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.PhrasalVerbMeanings.ToList())
                                            {
                                                idiomSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.CollocationMeanings.ToList())
                                            {
                                                idiomSynonymMeaning.SynonymSet.CollocationMeanings.Add(item);
                                            }
                                            foreach (var item in meaning.SynonymSet.IdiomMeanings.ToList())
                                            {
                                                idiomSynonymMeaning.SynonymSet.IdiomMeanings.Add(item);
                                            }
                                            meaning.SynonymSet = idiomSynonymMeaning.SynonymSet;
                                        }
                                        break;
                                    #endregion
                                }
                            }
                        }
                        #endregion
                        if (isMeaningNew) word.Meanings.Add(meaning);
                    }
                }
                #endregion
                #region Prepositions
                //  Removes all prepositionMixes that have been deleted by the user
                if (parameters.PrepositionMix == null)
                {
                    foreach (var prepositionMix in word.PrepositionMixes.ToList())
                    {
                        word.PrepositionMixes.Remove(prepositionMix);
                        _entities.PrepositionMixes.DeleteObject(prepositionMix);
                    }
                }
                else
                {
                    foreach (var prepositionMix in word.PrepositionMixes.Where(i => !parameters.PrepositionMix.Any(j => j.ID == i.ID)).ToList())
                    {
                        word.PrepositionMixes.Remove(prepositionMix);
                        _entities.PrepositionMixes.DeleteObject(prepositionMix);
                    }
                }
                //  Checks all prepositionMixes that should be saved
                if (parameters.PrepositionMix != null)
                {
                    foreach (var paramPrepositionMix in parameters.PrepositionMix)
                    {
                        PrepositionMix prepositionMix = word.PrepositionMixes.FirstOrDefault(i => i.ID == paramPrepositionMix.ID);
                        //  This is a new prepositionMix
                        if (prepositionMix == null)
                        {
                            prepositionMix = new PrepositionMix() { IDF_Preposition = paramPrepositionMix.Preposition };
                            foreach (var prepositionMixExample in paramPrepositionMix.Examples)
                            {
                                prepositionMix.Examples.Add(new PrepositionMixExample() { Example = prepositionMixExample.Example });
                            }
                            word.PrepositionMixes.Add(prepositionMix);
                        }
                        //This is an existent prepositionMix
                        else
                        {
                            if (prepositionMix.IDF_Preposition != paramPrepositionMix.Preposition) { prepositionMix.IDF_Preposition = paramPrepositionMix.Preposition; }
                            //  Removes all prepositionMix examples that have been deleted by the user
                            foreach (var prepositionMixExample in prepositionMix.Examples.Where(i => !paramPrepositionMix.Examples.Any(j => j.ID == i.ID)).ToList())
                            {
                                prepositionMix.Examples.Remove(prepositionMixExample);
                                _entities.PrepositionMixExamples.DeleteObject(prepositionMixExample);
                            }
                            foreach (var paramPrepositionMixExample in paramPrepositionMix.Examples)
                            {
                                PrepositionMixExample prepositionMixExample = prepositionMix.Examples.FirstOrDefault(i => i.ID == paramPrepositionMixExample.ID);
                                //  This is a new prepositionMix example
                                if (prepositionMixExample == null)
                                {
                                    prepositionMixExample = new PrepositionMixExample() { Example = paramPrepositionMixExample.Example };
                                    prepositionMix.Examples.Add(prepositionMixExample);
                                }
                                //This is an existent prepositionMix example
                                else
                                {
                                    if (prepositionMixExample.Example != paramPrepositionMixExample.Example) { prepositionMixExample.Example = paramPrepositionMixExample.Example; }
                                }
                            }
                        }
                    }
                }
                #endregion
                if (_entities.ObjectStateManager.GetObjectStateEntries(
                    System.Data.EntityState.Modified |
                    System.Data.EntityState.Added |
                    System.Data.EntityState.Deleted).Count() > 0)
                {
                    try { _entities.SaveChanges(); }
                    catch (Exception ex)
                    {
                        return Json(new
                        {
                            result = "error",
                            message = ex.Message,
                            innerMessage = ex.InnerException == null ? null : ex.InnerException.Message
                        });
                    }
                }
                return Json(new { result = "redirect", url = Url.Action("Item", "Word", new { id = word.ID }) });
            }
            else { return Json(new { result = "error", message = "The model state is invalid" }); }         
        }

        [HttpPost]
        [AjaxAuthorize(Roles = "Creator")]
        public JsonResult Delete(int id)
        {
            Word word = _entities.Words.FirstOrDefault(i => i.ID == id);
            if (word == null) { return Json(new { result = "error", message = "The word is not found" }); }
            foreach (var meaning in word.Meanings)
            {
                SynonymSet synonymSet = meaning.SynonymSet;
                if(synonymSet != null)
                {
                    synonymSet.WordMeanings.Remove(meaning);
                    if (meaning.SynonymSet.IsEmpty()) _entities.SynonymSets.DeleteObject(meaning.SynonymSet);
                }
            }     
            _entities.Words.DeleteObject(word);
            if (_entities.ObjectStateManager.GetObjectStateEntries(
                     System.Data.EntityState.Modified |
                     System.Data.EntityState.Added |
                     System.Data.EntityState.Deleted).Count() > 0)
            {
                try { _entities.SaveChanges(); }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        result = "error",
                        message = ex.Message,
                        innerMessage = ex.InnerException == null ? null : ex.InnerException.Message
                    });
                }
            }
            return Json(new { result = "redirect", url = Url.Action("Index", "Word") });
        }
    }
}