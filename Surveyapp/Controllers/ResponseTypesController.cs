﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Surveyapp.Models;
using Surveyapp.Services;

namespace Surveyapp.Controllers
{
    [Authorize]
    public class ResponseTypesController : Controller
    {
        private readonly SurveyContext _context;

        public ResponseTypesController(SurveyContext context)
        {
            _context = context;
        }

        // GET: ResponseTypes
        [NoDirectAccess]
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryId = _context.SurveySubject.SingleOrDefault(z => z.Id == id)?.CategoryId;
            ViewBag.SubjectId = id;
            ViewBag.SurveyId = _context.SurveyCategory.SingleOrDefault(x=>x.SurveySubjects.Any(y=>y.Id==id))?.SurveyId;
            ViewBag.CategoryId = _context.SurveySubject.SingleOrDefault(x=>x.Id==id)?.CategoryId;
            ViewBag.SubjectName = _context.SurveySubject.SingleOrDefault(x=>x.Id==id)?.SubjectName;
            var otherSubjectOption = _context.ResponseType.Any(x => x.Subject.CategoryId == categoryId);
            ViewBag.otherSubjectOption = otherSubjectOption;
            ViewData["Subjects"] =  new SelectList(_context.SurveySubject.Where(x=>x.CategoryId == categoryId),"Id", "SubjectName");
            var surveyContext = _context.ResponseType.Include(r => r.Subject.Category).Where(x=>x.SubjectId == id);
            /*var responses =*/
            return View(await surveyContext.ToListAsync());
        }

        // GET: ResponseTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var responseType = await _context.ResponseType
                .Include(r => r.Subject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (responseType == null)
            {
                return NotFound();
            }

            return View(responseType);
        }

        // GET: ResponseTypes/Create
        [NoDirectAccess]
        public IActionResult Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Subject = _context.SurveySubject.Include(x=>x.Category).SingleOrDefault(x => x.Id == id);
            ViewBag.SurveyId = Subject.Category.SurveyId;
            ViewBag.CategoryId =Subject.Category.Id;
            ViewBag.SubjectId = id;
            //ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id");
            return View();
        }

        // POST: ResponseTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [NoDirectAccess]
        public async Task<IActionResult> Create([Bind("Id,ResponseName,SubjectId")] ResponseType responseType,int SubjectId,string ResponseName,string[] responseDictionary,string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            /*if (ModelState.IsValid)
            {*/
            var respDictonary = new Dictionary<string, string>();
            if (responseDictionary.Length>0)
            {
                for (int i = 0; i < responseDictionary.Length; i++)
                {
                    if (responseDictionary[i] != null)
                    {
                        respDictonary.Add((i+1).ToString(), responseDictionary[i]);
                    }
                }
                /*var dictionary = responseDictionary.ToDictionary(item => item.Key,
                    item => item.Value);*/
                ResponseType responseTypes = new ResponseType()
                {
                    SubjectId = SubjectId,
                    ResponseName = ResponseName,
                    ResponseDictionary = respDictonary
                };
                
                    _context.Add(responseTypes);
                    await _context.SaveChangesAsync();
                    TempData["FeedbackMessage"] = $"survey responses added successfully";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return RedirectToAction("_CreatePartial", "Questions", new { id = responseTypes.Id,subjectId = responseTypes.SubjectId });
                    }
                    return RedirectToAction(nameof(Create),"Questions",new {id=responseTypes.SubjectId});
            }
            /*else
            {
                return View(responseType);
            }*/
            
            /*}*/
            ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id", responseType.SubjectId);
            return View(responseType);
        }
        
        [NoDirectAccess]
        public async Task<IActionResult> AssociateResponse(int? subId, int? id)
        {
            if (subId != null && id != null)
            {
                var subjectAssociate = _context.ResponseType.SingleOrDefault(x => x.SubjectId == subId);
                ResponseType responseTypes = new ResponseType()
                {
                    SubjectId = (int)id,
                    ResponseName = subjectAssociate?.ResponseName,
                    ResponseDictionary = subjectAssociate?.ResponseDictionary
                };
                
                _context.Add(responseTypes);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"survey responses added successfully";
                return RedirectToAction(nameof(Index),new {id=responseTypes.SubjectId});
            }
            return RedirectToAction(nameof(Index),new {id=subId});
        } 

        // GET: ResponseTypes/Edit/5
        [NoDirectAccess]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var responseType = await _context.ResponseType.Include(x=>x.Subject.Category).SingleOrDefaultAsync(x=>x.Id == id);
            if (responseType == null)
            {
                return NotFound();
            }

            ViewBag.SurveyId = responseType.Subject.Category.SurveyId;
            ViewBag.CategoryId = responseType.Subject.CategoryId;
            ViewBag.typeDictonary = responseType.ResponseDictionary;
            ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id", responseType.SubjectId);
            return View(responseType);
        }

        // POST: ResponseTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [NoDirectAccess]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ResponseName")] ResponseType responseType, string[] responseDictionary, string ResponseName)
        {
            if (id != responseType.Id)
            {
                return NotFound();
            }
            //ModelState.Remove("ResponseDictionary");
            if (responseDictionary.Any())
            {
                /*var resposUpdate = _context.ResponseType.SingleOrDefault(x=>x.Id == responseType.Id);
                try
                {
                    resposUpdate.ResponseName=responseType.ResponseName;
                    _context.Update(resposUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResponseTypeExists(responseType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }*/
                var respDictonary = new Dictionary<string, string>();
                    for (int i = 0; i < responseDictionary.Length; i++)
                    {
                        respDictonary.Add((i+1).ToString(), responseDictionary[i]);
                    }
                    ResponseType responseTypes = await _context.ResponseType.FindAsync(id);

                    responseTypes.ResponseName = ResponseName;
                    responseTypes.ResponseDictionary = respDictonary;

                    _context.Update(responseTypes);
                    await _context.SaveChangesAsync();
                    TempData["FeedbackMessage"] = $"survey responses edited successfully";
                return RedirectToAction(nameof(Index),new{id=responseTypes.SubjectId});
            }
            ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id", responseType.SubjectId);
            return View(responseType);
        }

        // GET: ResponseTypes/Delete/5
        [NoDirectAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var responseType = await _context.ResponseType
                .Include(r => r.Subject.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (responseType == null)
            {
                return NotFound();
            }
            ViewBag.SurveyId = responseType.Subject.Category.SurveyId;
            ViewBag.CategoryId = responseType.Subject.CategoryId;
            return View(responseType);
        }

        // POST: ResponseTypes/Delete/5
        [NoDirectAccess]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var responseType = await _context.ResponseType.FindAsync(id);
            _context.ResponseType.Remove(responseType);
            await _context.SaveChangesAsync();
            TempData["FeedbackMessage"] = $"survey responses deleted successfully";
            return RedirectToAction(nameof(Index),new{id=responseType.SubjectId});
        }
        [NoDirectAccess]
        private bool ResponseTypeExists(int id)
        {
            return _context.ResponseType.Any(e => e.Id == id);
        }

        public IActionResult _CreatePartial(int id)
        {
            ViewBag.SubjectId = id;
            return PartialView(new ResponseType());
        }
    }
}
