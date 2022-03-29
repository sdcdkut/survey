using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Surveyapp.Models;

namespace Surveyapp.Controllers
{
    public class QuestionGroupsController : Controller
    {
        private readonly SurveyContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;

        public QuestionGroupsController(SurveyContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
        }

        // GET
        [Authorize(/*Roles = "Surveyor"*/)]
        public async Task<IActionResult> Index(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            ViewBag.SubjectId = id;
            ViewBag.SurveyId = _context.SurveySubject.SingleOrDefault(x => x.Id == id)?.SurveyId;
            ViewBag.CategoryId = _context.SurveySubject.SingleOrDefault(x => x.Id == id)?.CategoryId;
            var questionGroupsContext = _context.QuestionGroups.Include(s => s.SurveySubject.Survey)
                .Where(x => x.SubjectId == id);
            return View(await questionGroupsContext.ToListAsync());
        }

        // GET: QuestionGroups/Details/5
        //[NoDirectAccess]
        public async Task<IActionResult> Details(int? id)
        {
            var userId = _usermanager.GetUserId(User);
            var survey = await _context.QuestionGroups
                .Include(s => s.SurveySubject.Survey)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (survey == null)
            {
                return NotFound();
            }

            return View(survey);
        }

        // GET: QuestionGroups/Create
        public async Task<IActionResult> Create(int id)
        {
            ViewData["SurveySubjectId"] = id;
            ViewData["SurveyId"] = (await _context.SurveySubject.FindAsync(id))?.SurveyId;
            return View();
        }

        // POST: QuestionGroups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(/*Roles = "Surveyor"*/)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Dictionary<int, QuestionGroup> QuestionGroup, int subjectId)
        {
            if (QuestionGroup.Any())
            {
                var counter = 0;
                foreach (var questionGroup in QuestionGroup)
                {
                    if (questionGroup.Value != null)
                        _context.QuestionGroups.Add(new QuestionGroup
                        {
                            Name = questionGroup.Value?.Name,
                            SubjectId = questionGroup.Value.SubjectId
                        });
                    counter++;
                }
                
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"added {counter} question groups  successfully";
                return RedirectToAction(nameof(Index), new {id = subjectId });
            }
            ViewData["SurveySubjectId"] = subjectId;
            ViewData["SurveyId"] = (await _context.SurveySubject.FindAsync(subjectId))?.SurveyId;
            return View();
        }

        // GET: QuestionGroups/Edit/5
        //[NoDirectAccess]
        [Authorize(/*Roles = "Surveyor"*/)]
        public async Task<IActionResult> Edit(int? id)
        {
            var questionGroup = await _context.QuestionGroups.FindAsync(id);
            if (questionGroup == null)
            {
                return NotFound();
            }

            return View(questionGroup);
        }

        // POST: QuestionGroups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[NoDirectAccess]
        [Authorize(/*Roles = "Surveyor"*/)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SubjectId")] QuestionGroup questionGroup)
        {
            if (id != questionGroup.Id)
            {
                return NotFound();
            }

            //ModelState.Remove<Survey>(x => x.SurveyerId);
            /*survey.SurveyerId = _usermanager.GetUserId(User);*/
            if (ModelState.IsValid)
            {
                try
                {
                    _context.QuestionGroups.Update(questionGroup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionQroupExists(questionGroup.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["FeedbackMessage"] = $"{questionGroup.Name}  edited successfully";
                return RedirectToAction(nameof(Index), new { id = questionGroup.SubjectId });
            }

            return View(questionGroup);
        }

        // GET: QuestionGroups/Delete/5
        //[NoDirectAccess]
        [Authorize(/*Roles = "Surveyor"*/)]
        public async Task<IActionResult> Delete(int? id)
        {
            var questionGroup = await _context.QuestionGroups
                .Include(s => s.SurveySubject.Survey)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionGroup == null)
            {
                return NotFound();
            }

            return View(questionGroup);
        }

        // POST: QuestionGroups/Delete/5
        //[NoDirectAccess]
        [Authorize(/*Roles = "Surveyor"*/)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var questionGroup = await _context.QuestionGroups.FindAsync(id);
            if (questionGroup != null) _context.QuestionGroups.Remove(questionGroup);
            await _context.SaveChangesAsync();
            TempData["FeedbackMessage"] = $"{questionGroup?.Name} deleted successfully";
            return RedirectToAction(nameof(Index), new  {id = questionGroup?.SubjectId});
        }

        private bool QuestionQroupExists(int id)
        {
            return _context.Survey.Any(e => e.Id == id);
        }
    }
}