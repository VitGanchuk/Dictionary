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
    public class CollocationController : DictionaryController
    {
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Index()
        {
            ItemListParameters parameters = new ItemListParameters()
            {
                Destination = Session["CollocationSearchDestination"] == null ? 0 : (int)Session["CollocationSearchDestination"],
                Topics = Session["CollocationSearchTopics"] == null ? null : Session["CollocationSearchTopics"] as IEnumerable<int>,
                Formalities = Session["CollocationSearchFormalities"] == null ? null : Session["CollocationSearchFormalities"] as IEnumerable<int>,
                SpeechParts = null,
                BaseVerb = -1
            };
            return View(new ItemListViewModel(Models.Utilities.Item.Collocation, parameters, this.Request.IsAuthenticated, this.HttpContext.User, this._entities));
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

        [HttpGet]
        public ActionResult Item(int id)
        {
            if (this.Request.IsAjaxRequest())
            {
                return Json(new { result = "redirect", url = Url.Action("Item", "Collocation", new { id = id }) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return View(new ItemViewModel(this.Request.UrlReferrer, VitEgoDictionary.Models.Utilities.Item.Collocation, id, this.Request.IsAuthenticated, this.HttpContext.User, this._entities));
            }
        }

        [HttpPost]
        public JsonResult LoadItems(ItemListParameters parameters)
        {
            //  Saves the seasrch parameters in Session 
            if (parameters != null)
            {
                Session["CollocationSearchDestination"] = parameters.Destination;
                Session["CollocationSearchTopics"] = parameters.Topics == null ? null : parameters.Topics;
                Session["CollocationSearchFormalities"] = parameters.Formalities == null ? null : parameters.Formalities;
            }
            ItemListViewModel model = new ItemListViewModel(Models.Utilities.Item.Collocation, parameters, this.Request.IsAuthenticated, this.HttpContext.User, this._entities);
            IEnumerable<Collocation> collocations = model.Items as IEnumerable<Collocation>;
            return Json(new
            {
                page = model.Page,
                pagesToDisplay = model.PagesToDisplay,
                isFiltered = model.isFiltered,
                overallRecords = model.OverallRecords,
                overallPages = model.OverallPages,
                items = collocations.
                    Select(i => new
                    {
                        id = i.ID,
                        name = i.Name,
                        topic = i.Topic.Name,
                        meanings = i.Meanings.OrderBy(j => j.Meaning).Select(j => new { meaning = j.Meaning })
                    })
            });
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
        public ActionResult Create(CollocationIdiomParameter parameters)
        {
            if (ModelState.IsValid)
            {
                #region Collocation
                Collocation collocation = new Collocation()
                {
                    Name = parameters.Name,
                    IDF_Topic = parameters.Topic,
                    IDF_Formality = parameters.Formality
                };
                #endregion
                #region Meanings
                if (parameters.Meanings != null)
                {
                    foreach (var paramMeaning in parameters.Meanings)
                    {
                        CollocationMeaning meaning = new CollocationMeaning() { Meaning = paramMeaning.Meaning };
                        if (paramMeaning.Examples != null)
                        {
                            foreach (var paramMeaningExample in paramMeaning.Examples)
                            {
                                meaning.Examples.Add(new CollocationMeaningExample() { Example = paramMeaningExample.Example });
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
                                            meaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            wordSynonymMeaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            meaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            phrasalVerbSynonymMeaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            meaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            collocationSynonymMeaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            meaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            idiomSynonymMeaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                        collocation.Meanings.Add(meaning);
                    }
                }
                #endregion
                #region AssociatedWords
                if (parameters.Mixes != null)
                {
                    foreach (var paramMix in parameters.Mixes)
                    {
                        Word word = _entities.Words.FirstOrDefault(i => i.ID == paramMix.Word);
                        collocation.CollocationMixes.Add(new CollocationMix() { Collocation = collocation, Word = word });
                    }
                }
                #endregion
                _entities.Collocations.AddObject(collocation);
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
                return Json(new { result = "redirect", url = Url.Action("Item", "Collocation", new { id = collocation.ID }) });
            }
            else { return Json(new { result = "error", message = "The model state is invalid" }); }
        }

        [HttpGet]
        [Authorize(Roles = "Creator")]
        public ActionResult Edit(int id)
        {
            Repository.SetSessionVariables(this.Session, this._entities);
            return View(new EditItemViewModel(this.Request.UrlReferrer, VitEgoDictionary.Models.Utilities.Item.Collocation, id, this.Request.IsAuthenticated, this.HttpContext.User, this._entities));
        }

        [HttpPost]
        [Authorize(Roles = "Creator")]
        public JsonResult Edit(CollocationIdiomParameter parameters)
        {
            if (ModelState.IsValid)
            {
                #region Collocation
                Collocation collocation = _entities.Collocations.FirstOrDefault(i => i.ID == parameters.ID);
                if (collocation == null) { return Json(new { result = "error", message = "The collocation is not found" }); }
                //  Replaces only what has been changed
                if (collocation.Name != parameters.Name) { collocation.Name = parameters.Name; }
                if (collocation.IDF_Topic != parameters.Topic) { collocation.IDF_Topic = parameters.Topic; }
                if (collocation.IDF_Formality != parameters.Formality) { collocation.IDF_Formality = parameters.Formality; }
                #endregion
                #region Meanings
                //  Removes all meanings that have been deleted by the user
                if (parameters.Meanings == null)
                {
                    foreach (var meaning in collocation.Meanings.ToList())
                    {
                        _entities.CollocationMeanings.DeleteObject(meaning);
                    }
                }
                else
                {
                    foreach (var meaning in collocation.Meanings.Where(i => !parameters.Meanings.Any(j => j.ID == i.ID)).ToList())
                    {
                        _entities.CollocationMeanings.DeleteObject(meaning);
                    }
                }
                //  Checks all meanings that should be saved
                if (parameters.Meanings != null)
                {
                    foreach (var paramMeaning in parameters.Meanings)
                    {
                        CollocationMeaning meaning = collocation.Meanings.FirstOrDefault(i => i.ID == paramMeaning.ID);
                        bool isMeaningNew = false;
                        #region New Meaning
                        if (meaning == null)
                        {
                            isMeaningNew = true;
                            meaning = new CollocationMeaning() { Meaning = paramMeaning.Meaning };
                            if (paramMeaning.Examples != null)
                            {
                                foreach (var paramMeaningExample in paramMeaning.Examples)
                                {
                                    meaning.Examples.Add(new CollocationMeaningExample() { Example = paramMeaningExample.Example });
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
                                    _entities.CollocationMeaningExamples.DeleteObject(meaningExample);
                                }
                            }
                            else
                            {
                                foreach (var meaningExample in meaning.Examples.Where(i => !paramMeaning.Examples.Any(j => j.ID == i.ID)).ToList())
                                {
                                    _entities.CollocationMeaningExamples.DeleteObject(meaningExample);
                                }
                            }
                            if (paramMeaning.Examples != null)
                            {
                                //  Checks all meaning examples that should be saved
                                foreach (var paramMeaningExample in paramMeaning.Examples)
                                {
                                    CollocationMeaningExample meaningExample = meaning.Examples.FirstOrDefault(i => i.ID == paramMeaningExample.ID);
                                    //  This is a new meaning example
                                    if (meaningExample == null)
                                    {
                                        meaningExample = new CollocationMeaningExample() { Example = paramMeaningExample.Example };
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
                                                j.ItemType == (int)Models.Utilities.Item.Word)).ToList())
                                    { meaning.SynonymSet.WordMeanings.Remove(synonymMeaning); }
                                    foreach (var synonymMeaning in meaning.SynonymSet.PhrasalVerbMeanings
                                        .Where(i => !paramMeaning.Synonyms.
                                            Any(j => j.Meaning == i.ID &&
                                                j.ItemType == (int)Models.Utilities.Item.PhrasalVerb)).ToList())
                                    { meaning.SynonymSet.PhrasalVerbMeanings.Remove(synonymMeaning); }
                                    foreach (var synonymMeaning in meaning.SynonymSet.CollocationMeanings
                                       .Where(i => !paramMeaning.Synonyms.
                                           Any(j => j.Meaning == i.ID &&
                                               j.ItemType == (int)Models.Utilities.Item.Collocation) &&
                                                i.ID != meaning.ID).ToList())
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
                                            meaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            wordSynonymMeaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            meaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            phrasalVerbSynonymMeaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            meaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            collocationSynonymMeaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            meaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                                            idiomSynonymMeaning.SynonymSet.CollocationMeanings.Add(meaning);
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
                        if (isMeaningNew) collocation.Meanings.Add(meaning);
                    }
                }
                #endregion
                #region Associated Words
                //  Removes all associated word that have been deleted by the user
                if (parameters.Mixes == null)
                {
                    foreach (var mix in collocation.CollocationMixes.ToList())
                    {
                        _entities.CollocationMixes.DeleteObject(mix);
                    }
                }
                else
                {
                    foreach (var mix in collocation.CollocationMixes.Where(i => !parameters.Mixes.Any(j => j.ID == i.ID)).ToList())
                    {
                        _entities.CollocationMixes.DeleteObject(mix);
                    }
                }
                if (parameters.Mixes != null)
                {
                    //  Checks all associated words that should be saved
                    foreach (var paramMix in parameters.Mixes)
                    {
                        CollocationMix mix = collocation.CollocationMixes.FirstOrDefault(i => i.ID == paramMix.ID);
                        Word word = _entities.Words.FirstOrDefault(i => i.ID == paramMix.Word);
                        //  This is a new idiom mix
                        if (mix == null)
                        {
                            mix = new CollocationMix() { Collocation = collocation, Word = word };
                            collocation.CollocationMixes.Add(mix);
                        }
                        //This is an existent idiom mix
                        else
                        {
                            if (mix.IDF_Word != paramMix.Word) { mix.Word = word; }
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
                return Json(new { result = "redirect", url = Url.Action("Item", "Collocation", new { id = collocation.ID }) });
            }
            else { return Json(new { result = "error", message = "The model state is invalid" }); }
        }

        [HttpPost]
        [AjaxAuthorize(Roles = "Creator")]
        public JsonResult Delete(int id)
        {
            Collocation collocation = _entities.Collocations.FirstOrDefault(i => i.ID == id);
            if (collocation == null) { return Json(new { result = "error", message = "The collocation is not found" }); }
            foreach (var meaning in collocation.Meanings)
            {
                SynonymSet synonymSet = meaning.SynonymSet;
                if (synonymSet != null)
                {
                    synonymSet.CollocationMeanings.Remove(meaning);
                    if (meaning.SynonymSet.IsEmpty()) _entities.SynonymSets.DeleteObject(meaning.SynonymSet);
                }
            }
            _entities.Collocations.DeleteObject(collocation);
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
            return Json(new { result = "redirect", url = Url.Action("Index", "Collocation") });
        }
    }
}
