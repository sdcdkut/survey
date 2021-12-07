using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _usermanager;

        public ResponseTypesController(SurveyContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
        }

        // GET: ResponseTypes
        //[NoDirectAccess]
        public async Task<IActionResult> Index()
        {
            return View(await _context.ResponseType.Include(c=>c.Creator).ToListAsync());
        }

        // GET: ResponseTypes/Details/5
        // [NoDirectAccess]
        public async Task<IActionResult> Details(int? id)
        {
            var responseType = await _context.ResponseType.Include(c => c.Creator).FirstOrDefaultAsync(c=>c.Id == id);
            if (responseType == null)
            {
                return NotFound();
            }

            return View(responseType);
        }

        // GET: ResponseTypes/Create
        //[NoDirectAccess]
        public async Task<IActionResult> Create(int? id)
        {
            //ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id");
            return View();
        }

        // POST: ResponseTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        //[NoDirectAccess]
        public async Task<IActionResult> Create([Bind("Id,ResponseName,DisplayOptionType,CreatorId,ResponseDictionary")] ResponseType responseType,
            Dictionary<int, ResponseDictionary> ResponseDictionary)
        {
            responseType.ResponseDictionary = ResponseDictionary.Select(c => new ResponseDictionary { Name = c.Value?.Name, Value = c.Value!.Value }).ToList();
            responseType.CreatorId = _usermanager.GetUserId(User);
            if (ModelState.IsValid)
            {
                _context.ResponseType.Add(responseType);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"survey responses added successfully";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return RedirectToAction("_CreatePartial", "Questions", new { id = responseType.Id });
                }

                return RedirectToAction(nameof(Index));
            }
            /*else
            {
                return View(responseType);
            }*/

            /*}*/
            //ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id");
            return View(responseType);
        }

        //[NoDirectAccess]
        public async Task<IActionResult> AssociateResponse(int? subId, int? id)
        {
            if (subId != null && id != null)
            {
                var useId = _usermanager.GetUserId(User);
                var subject = await _context.SurveySubject.Include(x => x.Category).SingleOrDefaultAsync(x => x.Id == id);
                var survey = await _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor).FirstOrDefaultAsync(c => c.Id == subject.SurveyId);
                if (!survey.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId))
                {
                    return StatusCode(403);
                }

                var subjectAssociate = _context.ResponseType.SingleOrDefault( /*x => x.SubjectId == subId*/);
                ResponseType responseTypes = new ResponseType()
                {
                    //SubjectId = (int)id,
                    ResponseName = subjectAssociate?.ResponseName,
                    ResponseDictionary = subjectAssociate?.ResponseDictionary
                };

                _context.Add(responseTypes);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"survey responses added successfully";
                return RedirectToAction(nameof(Index) /*,new {id=responseTypes.SubjectId}*/);
            }

            return RedirectToAction(nameof(Index), new { id = subId });
        }

        // GET: ResponseTypes/Edit/5
        //[NoDirectAccess]
        public async Task<IActionResult> Edit(int? id)
        {
            var responseType = await _context.ResponseType.SingleOrDefaultAsync(x => x.Id == id);
            if (responseType == null)
            {
                return NotFound();
            }

            if (_usermanager.GetUserId(User) != responseType.CreatorId)
            {
                TempData["FeedbackMessage"] = $"Response type can only be edited by the creator, contact the creator for change";
                return RedirectToAction(nameof(Index));
            }

            //ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id");
            return View(responseType);
        }

        // POST: ResponseTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[NoDirectAccess]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ResponseName,CreatorId,DisplayOptionType,ResponseDictionary")] ResponseType responseType,
            Dictionary<int, ResponseDictionary> ResponseDictionary)
        {
            if (id != responseType.Id)
            {
                return NotFound();
            }

            if (_usermanager.GetUserId(User) != responseType.CreatorId)
            {
                TempData["FeedbackMessage"] = $"Response type can only be edited by the creator, contact the creator for change";
                return RedirectToAction(nameof(Index));
            }

            responseType.ResponseDictionary = ResponseDictionary.Select(c => new ResponseDictionary { Name = c.Value?.Name, Value = c.Value!.Value }).ToList();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(responseType);
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
                }

                TempData["FeedbackMessage"] = $"survey responses edited successfully";
                return RedirectToAction(nameof(Index));
            }
            //ModelState.Remove("ResponseDictionary");
            //ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id");
            return View(responseType);
        }

        // GET: ResponseTypes/Delete/5
        //[NoDirectAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            

            var responseType = await _context.ResponseType
                .FirstOrDefaultAsync(m => m.Id == id);
            if (responseType == null)
            {
                return NotFound();
            }

            if (_usermanager.GetUserId(User) != responseType.CreatorId)
            {
                TempData["FeedbackMessage"] = $"Response type can only be edited by the creator, contact the creator for change";
                return RedirectToAction(nameof(Index));
            }

            var useId = _usermanager.GetUserId(User);
            /*var survey = await _context.Survey.FindAsync(responseType.Subject.Category.SurveyId);
            if (survey.SurveyerId != useId)
            {
                return StatusCode(403);
            }
            ViewBag.SurveyId = responseType.Subject.Category.SurveyId;
            ViewBag.CategoryId = responseType.Subject.CategoryId;*/
            return View(responseType);
        }

        // POST: ResponseTypes/Delete/5
        //[NoDirectAccess]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var responseType = await _context.ResponseType.FindAsync(id);
            if (_usermanager.GetUserId(User) != responseType?.CreatorId)
            {
                TempData["FeedbackMessage"] = $"Response type can only be edited by the creator, contact the creator for change";
                return RedirectToAction(nameof(Index));
            }
            _context.ResponseType.Remove(responseType);
            await _context.SaveChangesAsync();
            TempData["FeedbackMessage"] = $"survey responses deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        //[NoDirectAccess]
        private bool ResponseTypeExists(int id)
        {
            return _context.ResponseType.Any(e => e.Id == id);
        }

        public async Task<IActionResult> _CreatePartial(int id)
        {
            var subject = await _context.SurveySubject.Include(x => x.Category).SingleOrDefaultAsync(x => x.Id == id);
            var useId = _usermanager.GetUserId(User);
            var survey = await _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor).FirstOrDefaultAsync(c => c.Id == subject.SurveyId);
            if (!survey.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId))
            {
                return StatusCode(403);
            }

            ViewBag.SubjectId = id;
            return PartialView(new ResponseType());
        }
    }
}