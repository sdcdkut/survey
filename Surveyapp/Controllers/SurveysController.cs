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
using Surveyapp.Services;

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
        [Authorize(Roles = "Surveyor")]
        public async Task<IActionResult> Index()
        {
            var surveyContext = _context.Survey.Include(s => s.Surveyer).Where(x=>x.SurveyerId == _usermanager.GetUserId(User));
            return View(await surveyContext.ToListAsync());
        }

        // GET: Surveys/Details/5
        [NoDirectAccess]
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
        [Authorize(Roles = "Surveyor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,name,Startdate,EndDate,status")] Survey survey)
        {
            survey.SurveyerId = _usermanager.GetUserId(User);
            survey.approvalStatus = "NotApproved";
            //survey.status = survey.status.ToString();
            if (survey != null)
            {
                _context.Add(survey);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"survey added {survey.name} successfully";
                var ajax = Request.Headers["X-Requested-With"];
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return RedirectToAction("_CreateSurveyCategoryPartial", "SurveyCategories", new { id = survey.Id });
                }
                return RedirectToAction(nameof(Create),"SurveyCategories",new {id = survey.Id });
            }
            return View(survey);
        }

        // GET: Surveys/Edit/5
        [NoDirectAccess]
        [Authorize(Roles = "Surveyor")]
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
        [NoDirectAccess]
        [Authorize(Roles = "Surveyor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,Startdate,EndDate,status,SurveyerId,approvalStatus")] Survey survey)
        {
            if (true)
            {
                
            }
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
                TempData["FeedbackMessage"] = $"{survey.name}  edited successfully";
                return RedirectToAction(nameof(Index));
            }
            ViewData["SurveyerId"] = new SelectList(_context.Users, "Id", "Id", survey.SurveyerId);
            return View(survey);
        }

        // GET: Surveys/Delete/5
        [NoDirectAccess]
        [Authorize(Roles = "Surveyor")]
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
        [NoDirectAccess]
        [Authorize(Roles = "Surveyor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var survey = await _context.Survey.FindAsync(id);
            _context.Survey.Remove(survey);
            await _context.SaveChangesAsync();
            TempData["FeedbackMessage"] = $"{survey.name} deleted successfully";
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
            surveyContext = surveyContext.Where(x=>x.SurveyCategorys.Any(a=>a.SurveySubjects.Any(z=>z.Questions.Any()))).Where(x=>x.approvalStatus=="Approved");
            return View(await surveyContext.ToListAsync());
        }
        [NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveSurveys()
        {
            var surveyContext = _context.Survey.Include(s => s.Surveyer);
            return View(await surveyContext.ToListAsync());
        }
        
        [NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeApproval(int? id, string Approvalstate)
        {
            if (id == null) return NotFound();
            var surveyEdit = _context.Survey.SingleOrDefault(x => x.Id == id);
            if (surveyEdit != null)
            {
                surveyEdit.approvalStatus = Approvalstate;
                _context.Survey.Update(surveyEdit );
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"survey {Approvalstate} successfully";
                return RedirectToAction(nameof(ApproveSurveys));
            }

            return RedirectToAction(nameof(ApproveSurveys));
        }
        public IActionResult TakeSurvey(int? id)
        {
            throw new NotImplementedException();
        }

        public IActionResult CreatePartal()
        {
            return PartialView("_Createsurvey", new Survey());
        }
    }
}
