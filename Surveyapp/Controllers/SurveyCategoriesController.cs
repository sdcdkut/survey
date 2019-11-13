using System;
using System.Collections.Generic;
using System.Linq;
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
    [Authorize(Roles = "Surveyor")]
    public class SurveyCategoriesController : Controller
    {
        private readonly SurveyContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;

        public SurveyCategoriesController(SurveyContext context, UserManager<ApplicationUser> usermanager)
        {
            _context = context;
            _usermanager = usermanager;
        }

        // GET: SurveyCategories
        //[NoDirectAccess]
        public async Task<IActionResult> Index(int? id)
        {
            if (id== null)
            {
                return NotFound();
            }

            ViewBag.SurveyId = id;
            var surveyContext = _context.SurveyCategory.Include(s => s.Survey).Where(x=>x.SurveyId==id);
            var survey = _context.Survey.SingleOrDefault(x => x.Id == id);
            var userId = _usermanager.GetUserId(User);
            if (survey?.SurveyerId != userId)
            {
                return StatusCode(403);
            }
            return View(await surveyContext.ToListAsync());
        }

        // GET: SurveyCategories/Details/5
        [NoDirectAccess]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveyCategory = await _context.SurveyCategory
                .Include(s => s.Survey)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (surveyCategory == null)
            {
                return NotFound();
            }

            return View(surveyCategory);
        }

        // GET: SurveyCategories/Create
        //[NoDirectAccess]
        public IActionResult Create(int? id)
        {
            if (id==null)
            {
                return NotFound();
            }
            var survey = _context.Survey.SingleOrDefault(x => x.Id == id);
            var userId = _usermanager.GetUserId(User);
            if (survey?.SurveyerId != userId)
            {
                return StatusCode(403);
            }
            ViewBag.SurveyId = id;
            //ViewData["SurveyId"] = new SelectList(_context.Survey, "Id", "Id");
            return View();
        }

        // POST: SurveyCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[NoDirectAccess]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName,SurveyId")] SurveyCategory surveyCategory)
        {
            
            if (ModelState.IsValid)
            {
                _context.Add(surveyCategory);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"survey category added successfully";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return RedirectToAction("_CreatePartial", "SurveySubjects", new { id = surveyCategory.Id });
                }
                return RedirectToAction(nameof(Create),"SurveySubjects",new {id=surveyCategory.Id});
            }
            
            ViewData["SurveyId"] = new SelectList(_context.Survey, "Id", "Id", surveyCategory.SurveyId);
            return View(surveyCategory);
        }

        // GET: SurveyCategories/Edit/5
        //[NoDirectAccess]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var surveyCategory = await _context.SurveyCategory.FindAsync(id);
            if (surveyCategory == null)
            {
                return NotFound();
            }

            var survey = await _context.Survey.FindAsync(surveyCategory.SurveyId);
            if (survey.SurveyerId != _usermanager.GetUserId(User))
            {
                return StatusCode(403);
            }
            ViewBag.SurveyId = surveyCategory.SurveyId;
            //ViewData["SurveyId"] = new SelectList(_context.Survey, "Id", "Id", surveyCategory.SurveyId);
            return View(surveyCategory);
        }

        // POST: SurveyCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[NoDirectAccess]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryName,SurveyId")] SurveyCategory surveyCategory)
        {
            if (id != surveyCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(surveyCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SurveyCategoryExists(surveyCategory.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["FeedbackMessage"] = $"survey category edited successfully";
                return RedirectToAction(nameof(Index),new {id=surveyCategory.SurveyId});
            }
            ViewData["SurveyId"] = new SelectList(_context.Survey, "Id", "Id", surveyCategory.SurveyId);
            return View(surveyCategory);
        }

        // GET: SurveyCategories/Delete/5
        //[NoDirectAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveyCategory = await _context.SurveyCategory
                .Include(s => s.Survey)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (surveyCategory == null)
            {
                return NotFound();
            }
            var survey = await _context.Survey.FindAsync(surveyCategory.SurveyId);
            if (survey.SurveyerId != _usermanager.GetUserId(User))
            {
                return StatusCode(403);
            }

            return View(surveyCategory);
        }

        // POST: SurveyCategories/Delete/5
        //[NoDirectAccess]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var surveyCategory = await _context.SurveyCategory.FindAsync(id);
            _context.SurveyCategory.Remove(surveyCategory);
            await _context.SaveChangesAsync();
            TempData["FeedbackMessage"] = $"survey category deleted successfully";
            return RedirectToAction(nameof(Index),new {id=surveyCategory.SurveyId});
        }

        private bool SurveyCategoryExists(int id)
        {
            return _context.SurveyCategory.Any(e => e.Id == id);
        }

        public IActionResult _CreateSurveyCategoryPartial(int id)
        {
            var survey = _context.Survey.SingleOrDefault(x => x.Id == id);
            var userId = _usermanager.GetUserId(User);
            if (survey?.SurveyerId != userId)
            {
                return StatusCode(403);
            }
            IQueryable<SurveyCategory> surveyCategory = _context.SurveyCategory.Where(x => x.SurveyId == id);
            ViewBag.SurveyId = id;
            return PartialView(new SurveyCategory());
        }

    }
}
