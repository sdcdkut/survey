using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SurveyCategoriesController : Controller
    {
        private readonly SurveyContext _context;

        public SurveyCategoriesController(SurveyContext context)
        {
            _context = context;
        }

        // GET: SurveyCategories
        [NoDirectAccess]
        public async Task<IActionResult> Index(int? id)
        {
            if (id== null)
            {
                return NotFound();
            }

            ViewBag.SurveyId = id;
            var surveyContext = _context.SurveyCategory.Include(s => s.Survey).Where(x=>x.SurveyId==id);
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
        [NoDirectAccess]
        public IActionResult Create(int? id)
        {
            if (id==null)
            {
                return NotFound();
            }

            ViewBag.SurveyId = id;
            //ViewData["SurveyId"] = new SelectList(_context.Survey, "Id", "Id");
            return View();
        }

        // POST: SurveyCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [NoDirectAccess]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName,SurveyId")] SurveyCategory surveyCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(surveyCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index),new {id=surveyCategory.SurveyId});
            }
            ViewData["SurveyId"] = new SelectList(_context.Survey, "Id", "Id", surveyCategory.SurveyId);
            return View(surveyCategory);
        }

        // GET: SurveyCategories/Edit/5
        [NoDirectAccess]
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
            ViewBag.SurveyId = surveyCategory.SurveyId;
            //ViewData["SurveyId"] = new SelectList(_context.Survey, "Id", "Id", surveyCategory.SurveyId);
            return View(surveyCategory);
        }

        // POST: SurveyCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [NoDirectAccess]
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
                return RedirectToAction(nameof(Index),new {id=surveyCategory.SurveyId});
            }
            ViewData["SurveyId"] = new SelectList(_context.Survey, "Id", "Id", surveyCategory.SurveyId);
            return View(surveyCategory);
        }

        // GET: SurveyCategories/Delete/5
        [NoDirectAccess]
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

            return View(surveyCategory);
        }

        // POST: SurveyCategories/Delete/5
        [NoDirectAccess]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var surveyCategory = await _context.SurveyCategory.FindAsync(id);
            _context.SurveyCategory.Remove(surveyCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index),new {id=surveyCategory.SurveyId});
        }

        private bool SurveyCategoryExists(int id)
        {
            return _context.SurveyCategory.Any(e => e.Id == id);
        }
    }
}
