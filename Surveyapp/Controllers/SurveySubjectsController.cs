using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Surveyapp.Models;

namespace Surveyapp.Controllers
{
    public class SurveySubjectsController : Controller
    {
        private readonly SurveyContext _context;

        public SurveySubjectsController(SurveyContext context)
        {
            _context = context;
        }

        // GET: SurveySubjects
        [Authorize]
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.CategoryId = id;
            ViewBag.SurveyId = _context.SurveyCategory.SingleOrDefault(x=>x.Id == id)?.SurveyId;
            var surveyContext = _context.SurveySubject.Include(s => s.Category).Include(x=>x.ResponseTypes).Where(x=>x.CategoryId==id);
            return View(await surveyContext.ToListAsync());
        }

        // GET: SurveySubjects/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveySubject = await _context.SurveySubject
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (surveySubject == null)
            {
                return NotFound();
            }

            return View(surveySubject);
        }

        // GET: SurveySubjects/Create
        [Authorize]
        public IActionResult Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = id;/*new SelectList(_context.SurveyCategory, "Id", "Id");*/
            return View();
        }

        // POST: SurveySubjects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubjectName,StateCorporation,Chairpersion,AppointmentDate,EndofTerm,CategoryId")] SurveySubject surveySubject)
        {
            if (ModelState.IsValid)
            {
                _context.Add(surveySubject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index),new{ id=surveySubject.CategoryId });
            }
            ViewData["CategoryId"] = new SelectList(_context.SurveyCategory, "Id", "Id", surveySubject.CategoryId);
            return View(surveySubject);
        }

        // GET: SurveySubjects/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveySubject = await _context.SurveySubject.FindAsync(id);
            if (surveySubject == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = surveySubject.CategoryId/*new SelectList(_context.SurveyCategory, "Id", "Id", surveySubject.CategoryId)*/;
            return View(surveySubject);
        }

        // POST: SurveySubjects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SubjectName,StateCorporation,Chairpersion,AppointmentDate,EndofTerm,CategoryId")] SurveySubject surveySubject)
        {
            if (id != surveySubject.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(surveySubject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SurveySubjectExists(surveySubject.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index),new{ id=surveySubject.CategoryId });
            }
            ViewData["CategoryId"] = new SelectList(_context.SurveyCategory, "Id", "Id", surveySubject.CategoryId);
            return View(surveySubject);
        }

        // GET: SurveySubjects/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveySubject = await _context.SurveySubject
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (surveySubject == null)
            {
                return NotFound();
            }

            return View(surveySubject);
        }

        // POST: SurveySubjects/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var surveySubject = await _context.SurveySubject.FindAsync(id);
            _context.SurveySubject.Remove(surveySubject);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index),new{ id=surveySubject.CategoryId });
        }

        private bool SurveySubjectExists(int id)
        {
            return _context.SurveySubject.Any(e => e.Id == id);
        }

        public IActionResult SurveySubjects(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjects = _context.SurveySubject.Where(x => x.CategoryId == id);
            //throw new NotImplementedException();
            return View(subjects);
        }
        
    }
}
