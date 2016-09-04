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
    public class PhrasalVerbController : DictionaryController
    {
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Index()
        {
            ItemListParameters parameters = new ItemListParameters()
            {
                Destination = Session["PhrasalVerbSearchDestination"] == null ? 0 : (int)Session["PhrasalVerbSearchDestination"],
                Topics = null,
                Formalities = Session["PhrasalVerbSearchFormalities"] == null ? null : Session["PhrasalVerbSearchFormalities"] as IEnumerable<int>,
                SpeechParts = null,
                BaseVerb = Session["PhrasalVerbSearchBaseVerb"] == null ? -1 : (int)Session["PhrasalVerbSearchBaseVerb"],
            };
            return View(new ItemListViewModel(Models.Utilities.Item.PhrasalVerb, parameters, this.Request.IsAuthenticated, this.HttpContext.User, _entities));
        }
        
        [ChildActionOnly]
        public ActionResult SearchPanel()
        {
            Repository.SetSessionVariables(this.Session, this._entities);
            if(Session["PhrasalVerbSearchBaseVerb"] == null) return PartialView(null);
            else
            {
                int baseVerb = (int)Session["PhrasalVerbSearchBaseVerb"];
                return PartialView(_entities.Words.FirstOrDefault(i => i.ID == baseVerb));
            }
        }

        [ChildActionOnly]
        public ActionResult SearchPanelSimple() {
            return PartialView();
        }

        [HttpGet]
        public ActionResult Item(int id)
        {
            if (this.Request.IsAjaxRequest())
            {
                return Json(new { result = "redirect", url = Url.Action("Item", "PhrasalVerb", new { id = id }) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return View(new ItemViewModel(this.Request.UrlReferrer, VitEgoDictionary.Models.Utilities.Item.PhrasalVerb, id, this.Request.IsAuthenticated, this.HttpContext.User, _entities));
            }    
        }

        [HttpPost]
        public JsonResult LoadItems(ItemListParameters parameters)
        {
            //  Saves the seasrch parameters in Session 
            if (parameters != null)
            {
                Session["PhrasalVerbSearchDestination"] = parameters.Destination;
                if (parameters.BaseVerb > 0) Session["PhrasalVerbSearchBaseVerb"] = parameters.BaseVerb;
                else Session["PhrasalVerbSearchBaseVerb"] = null;
                Session["PhrasalVerbSearchFormalities"] = parameters.Formalities == null ? null : parameters.Formalities;
            }
            ItemListViewModel model = new ItemListViewModel(Models.Utilities.Item.PhrasalVerb, parameters, this.Request.IsAuthenticated, this.HttpContext.User, _entities);
            IEnumerable<PhrasalVerb> phrasalVerbs = model.Items as IEnumerable<PhrasalVerb>;
            return Json(new
            {
                page = model.Page,
                pagesToDisplay = model.PagesToDisplay,
                isFiltered = model.isFiltered,
                overallRecords = model.OverallRecords,
                overallPages = model.OverallPages,
                items = phrasalVerbs.
                    Select(i => new
                    {
                        id = i.ID,
                        name = i.Name,
                        meanings = i.Meanings.OrderBy(j => j.Meaning).Select(j => new { meaning = j.Meaning })
                    })
            });
        }

        [HttpGet]
        [Authorize(Roles = "Creator")]
        public ActionResult Create()
        {
            Repository.SetSessionVariables(this.Session, this._entities);
            return View(new UrlReferrerViewModel(this.Request.UrlReferrer, this.Request.IsAuthenticated, this.HttpContext.User, _entities));
        }

        [HttpPost]
        [Authorize(Roles = "Creator")]
        public ActionResult Create(PhrasalVerbParameter parameters)
        {
            if (ModelState.IsValid)
            {
                #region Phrasal Verb
                PhrasalVerb phrasalVerb = new PhrasalVerb()
                {
                    IDF_Verb = parameters.Verb,
                    IDF_Preposition = parameters.Preposition,
                    IDF_Formality = parameters.Formality
                };
                #endregion
                #region Meanings
                if (parameters.Meanings != null)
                {
                    foreach (var paramMeaning in parameters.Meanings)
                    {
                        PhrasalVerbMeaning meaning = new PhrasalVerbMeaning() { Meaning = paramMeaning.Meaning };
                        if (paramMeaning.Examples != null)
                        {
                            foreach (var paramMeaningExample in paramMeaning.Examples)
                            {
                                meaning.Examples.Add(new PhrasalVerbMeaningExample() { Example = paramMeaningExample.Example });
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
                                            meaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            wordSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            meaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            phrasalVerbSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            meaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            collocationSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            meaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            idiomSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                        phrasalVerb.Meanings.Add(meaning);  
                    }
                }
                #endregion
                _entities.PhrasalVerbs.AddObject(phrasalVerb);
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
                return Json(new { result = "redirect", url = Url.Action("Item", "PhrasalVerb", new { id = phrasalVerb.ID }) });
            }
            else { return Json(new { result = "error", message = "The model state is invalid" }); }     
        }

        [HttpGet]
        [Authorize(Roles="Creator")]
        public ActionResult Edit(int id)
        {
            Repository.SetSessionVariables(this.Session, this._entities);
            return View(new EditItemViewModel(this.Request.UrlReferrer, VitEgoDictionary.Models.Utilities.Item.PhrasalVerb, id, this.Request.IsAuthenticated, this.HttpContext.User, this._entities));
        }

        [HttpPost]
        [Authorize(Roles = "Creator")]
        public JsonResult Edit(PhrasalVerbParameter parameters)
        {
            if (ModelState.IsValid)
            {
                #region Phrasal Verb
                PhrasalVerb phrasalVerb = _entities.PhrasalVerbs.FirstOrDefault(i => i.ID == parameters.ID);
                if (phrasalVerb == null) { return Json(new { result = "error", message = "The phrasal verb is not found" }); }
                //  Replaces only what has been changed
                if (phrasalVerb.IDF_Verb != parameters.Verb) { phrasalVerb.IDF_Verb = parameters.Verb; }
                if (phrasalVerb.IDF_Preposition != parameters.Preposition) { phrasalVerb.IDF_Preposition = parameters.Preposition; }
                if (phrasalVerb.IDF_Formality != parameters.Formality) { phrasalVerb.IDF_Formality = parameters.Formality; }
                #endregion
                #region Meanings
                //  Removes all meanings that have been deleted by the user
                if (parameters.Meanings == null)
                {
                    foreach (var meaning in phrasalVerb.Meanings.ToList())
                    {
                        _entities.PhrasalVerbMeanings.DeleteObject(meaning);
                    }
                }
                else
                {
                    foreach (var meaning in phrasalVerb.Meanings.Where(i => !parameters.Meanings.Any(j => j.ID == i.ID)).ToList())
                    {
                        _entities.PhrasalVerbMeanings.DeleteObject(meaning);
                    }
                }
                //  Checks all meanings that should be saved
                if (parameters.Meanings != null)
                {
                    foreach (var paramMeaning in parameters.Meanings)
                    {
                        PhrasalVerbMeaning meaning = phrasalVerb.Meanings.FirstOrDefault(i => i.ID == paramMeaning.ID);
                        bool isMeaningNew = false;
                        #region New Meaning
                        if (meaning == null)
                        {
                            isMeaningNew = true;
                            meaning = new PhrasalVerbMeaning() { Meaning = paramMeaning.Meaning };
                            if (paramMeaning.Examples != null)
                            {
                                foreach (var paramMeaningExample in paramMeaning.Examples)
                                {
                                    meaning.Examples.Add(new PhrasalVerbMeaningExample() { Example = paramMeaningExample.Example });
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
                                    _entities.PhrasalVerbMeaningExamples.DeleteObject(meaningExample);
                                }
                            }
                            else
                            {
                                foreach (var meaningExample in meaning.Examples.Where(i => !paramMeaning.Examples.Any(j => j.ID == i.ID)).ToList())
                                {
                                    _entities.PhrasalVerbMeaningExamples.DeleteObject(meaningExample);
                                }
                            }
                            if (paramMeaning.Examples != null)
                            {
                                //  Checks all meaning examples that should be saved
                                foreach (var paramMeaningExample in paramMeaning.Examples)
                                {
                                    PhrasalVerbMeaningExample meaningExample = meaning.Examples.FirstOrDefault(i => i.ID == paramMeaningExample.ID);
                                    //  This is a new meaning example
                                    if (meaningExample == null)
                                    {
                                        meaningExample = new PhrasalVerbMeaningExample() { Example = paramMeaningExample.Example };
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
                                if (paramMeaning.Synonyms == null && meaning.SynonymSet != null)
                                {
                                    _entities.SynonymSets.DeleteObject(meaning.SynonymSet);
                                }
                                else
                                {
                                    foreach (var synonymMeaning in meaning.SynonymSet.WordMeanings
                                        .Where(i => !paramMeaning.Synonyms.
                                            Any(j => j.Meaning == i.ID &&
                                                j.ItemType == (int)Models.Utilities.Item.Word)).ToList())
                                    { meaning.SynonymSet.WordMeanings.Remove(synonymMeaning); }
                                    foreach (var synonymMeaning in meaning.SynonymSet.PhrasalVerbMeanings
                                        .Where(i => !paramMeaning.Synonyms.
                                            Any(j => j.Meaning == i.ID &&
                                                j.ItemType == (int)Models.Utilities.Item.PhrasalVerb) &&
                                                i.ID != meaning.ID).ToList())
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
                                            meaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            wordSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            meaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            phrasalVerbSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            meaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            collocationSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            meaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                                            idiomSynonymMeaning.SynonymSet.PhrasalVerbMeanings.Add(meaning);
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
                        if (isMeaningNew) phrasalVerb.Meanings.Add(meaning);
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
                return Json(new { result = "redirect", url = Url.Action("Item", "PhrasalVerb", new { id = phrasalVerb.ID }) });
            }
            else { return Json(new { result = "error", message = "The model state is invalid" }); }         
        }

        [HttpPost]
        [AjaxAuthorize(Roles = "Creator")]
        public JsonResult Delete(int id)
        {
            PhrasalVerb phrasalVerb = _entities.PhrasalVerbs.FirstOrDefault(i => i.ID == id);
            if (phrasalVerb == null) { return Json(new { result = "error", message = "The phrasal verb is not found" }); }
            foreach (var meaning in phrasalVerb.Meanings)
            {
                SynonymSet synonymSet = meaning.SynonymSet;
                if (synonymSet != null)
                {
                    synonymSet.PhrasalVerbMeanings.Remove(meaning);
                    if (meaning.SynonymSet.IsEmpty()) _entities.SynonymSets.DeleteObject(meaning.SynonymSet);
                }
            } 
            _entities.PhrasalVerbs.DeleteObject(phrasalVerb);
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
            return Json(new { result = "redirect", url = Url.Action("Index", "PhrasalVerb") });
        }
    }
}