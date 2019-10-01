using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Surveyapp.Models;

namespace Surveyapp.Controllers
{
    public class SurveysController : Controller
    {
        private readonly SurveyContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;

        public SurveysController(SurveyContext context,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
        }

        // GET: Surveys
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var surveyContext = _context.Survey.Include(s => s.Surveyer).Where(x=>x.SurveyerId == _usermanager.GetUserId(User));
            return View(await surveyContext.ToListAsync());
        }

        // GET: Surveys/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var survey = await _context.Survey
                .Include(s => s.Surveyer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (survey == null)
            {
                return NotFound();
            }

            return View(survey);
        }

        // GET: Surveys/Create
        public IActionResult Create()
        {
            ViewData["SurveyerId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Surveys/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,name,Startdate,EndDate,status")] Survey survey)
        {
            survey.SurveyerId = _usermanager.GetUserId(User);
            //survey.status = survey.status.ToString();
            /*if (ModelState.IsValid)
            {*/
                _context.Add(survey);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            /*}*/
            ViewData["SurveyerId"] = new SelectList(_context.Users, "Id", "Id", survey.SurveyerId);
            return View(survey);
        }

        // GET: Surveys/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var survey = await _context.Survey.FindAsync(id);
            if (survey == null)
            {
                return NotFound();
            }
            ViewData["SurveyerId"] = new SelectList(_context.Users, "Id", "Id", survey.SurveyerId);
            return View(survey);
        }

        // POST: Surveys/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,Startdate,EndDate,status,SurveyerId")] Survey survey)
        {
            if (id != survey.Id)
            {
                return NotFound();
            }
            //ModelState.Remove<Survey>(x => x.SurveyerId);
            /*survey.SurveyerId = _usermanager.GetUserId(User);*/
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(survey);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SurveyExists(survey.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SurveyerId"] = new SelectList(_context.Users, "Id", "Id", survey.SurveyerId);
            return View(survey);
        }

        // GET: Surveys/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var survey = await _context.Survey
                .Include(s => s.Surveyer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (survey == null)
            {
                return NotFound();
            }

            return View(survey);
        }

        // POST: Surveys/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var survey = await _context.Survey.FindAsync(id);
            _context.Survey.Remove(survey);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SurveyExists(int id)
        {
            return _context.Survey.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Surveys()
        {
            IQueryable<Survey> surveyContext = _context.Survey.Include(s => s.Surveyer).Include(x=>x.SurveyCategorys);
            if (User.Identity.IsAuthenticated)
            {
                //display surveys not created by current logged in user
                surveyContext = surveyContext.Where(x=>x.SurveyerId != _usermanager.GetUserId(User));
            }
            return View(await surveyContext.ToListAsync());
            //throw new NotImplementedException();
        }

        public IActionResult TakeSurvey(int? id)
        {
            throw new NotImplementedException();
        }
    }
}
