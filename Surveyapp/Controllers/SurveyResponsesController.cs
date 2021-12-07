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

namespace Surveyapp.Controllers
{
    public class SurveyResponsesController : Controller
    {
        private readonly SurveyContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;


        public SurveyResponsesController(SurveyContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
        }

        // GET: SurveyResponses
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var surveyContext = _context.SurveyResponse.Include(s => s.Respondant).Include(s => s.question);
            return View(await surveyContext.ToListAsync());
        }

        // GET: SurveyResponses/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveyResponse = await _context.SurveyResponse
                .Include(s => s.Respondant)
                .Include(s => s.question)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (surveyResponse == null)
            {
                return NotFound();
            }

            return View(surveyResponse);
        }

        // GET: SurveyResponses/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["RespondantId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["QuestionId"] = new SelectList(_context.Question, "Id", "Id");
            return View();
        }

        // POST: SurveyResponses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( /*[Bind("Id,RespondantId,QuestionId,Response")] SurveyResponse surveyResponse, string[] quizresponse*/
            Dictionary<int, SurveyResponse> quizResponse)
        {
            if ( /*ModelState.IsValid*/quizResponse.Any())
            {
                //int? categoryId = null;
                foreach (var quizAndResponse in quizResponse)
                {
                    string respondantId = null;
                    if (User.Identity!.IsAuthenticated)
                    {
                        respondantId = _usermanager.GetUserId(User);
                    }

                    var newResponse = new SurveyResponse()
                    {
                        QuestionId = quizAndResponse.Value.QuestionId,
                        Response = quizAndResponse.Value.Response,
                        RespondantId = respondantId,
                        ResponseText = quizAndResponse.Value?.ResponseText
                    };
                    /*foreach (var id in ids)
                {
                    var sd = id;
                }*/
                    //categoryId = _context.SurveySubject.SingleOrDefault(x => x.Questions.Any(y => y.Id == newResponse.QuestionId))?.CategoryId;
                    _context.SurveyResponse.Add(newResponse);
                }

                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"Survey Taken successfully";
                /*return RedirectToAction("SurveySubjects", "SurveySubjects", new { id = categoryId });*/
                return RedirectToAction("Surveys", "Surveys");
            }

            /*ViewData["RespondantId"] = new SelectList(_context.Users, "Id", "Id", surveyResponse.RespondantId);
            ViewData["QuestionId"] = new SelectList(_context.Question, "Id", "Id", surveyResponse.QuestionId);*/
            return RedirectToAction("Surveys", "Surveys");
        }

        // GET: SurveyResponses/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveyResponse = await _context.SurveyResponse.FindAsync(id);
            if (surveyResponse == null)
            {
                return NotFound();
            }

            ViewData["RespondantId"] = new SelectList(_context.Users, "Id", "Id", surveyResponse.RespondantId);
            ViewData["QuestionId"] = new SelectList(_context.Question, "Id", "Id", surveyResponse.QuestionId);
            return View(surveyResponse);
        }

        // POST: SurveyResponses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /*[Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RespondantId,QuestionId,Response")] SurveyResponse surveyResponse)
        {
            if (id != surveyResponse.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(surveyResponse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SurveyResponseExists(surveyResponse.Id))
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
            ViewData["RespondantId"] = new SelectList(_context.Users, "Id", "Id", surveyResponse.RespondantId);
            ViewData["QuestionId"] = new SelectList(_context.Question, "Id", "Id", surveyResponse.QuestionId);
            return View(surveyResponse);
        }*/

        // GET: SurveyResponses/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveyResponse = await _context.SurveyResponse
                .Include(s => s.Respondant)
                .Include(s => s.question)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (surveyResponse == null)
            {
                return NotFound();
            }

            return View(surveyResponse);
        }

        // POST: SurveyResponses/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var surveyResponse = await _context.SurveyResponse.FindAsync(id);
            _context.SurveyResponse.Remove(surveyResponse);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SurveyResponseExists(int id)
        {
            return _context.SurveyResponse.Any(e => e.Id == id);
        }

        [Authorize]
        public async Task<IActionResult> SurveyResults(int? id)
        {
            var subjects = _context.Survey.Include(c => c.SurveySubjects)
                .ThenInclude(c => c.Questions).ThenInclude(v => v.SurveyResponses).Include(c=>c.Surveyors).ToList()
                .Where(x => x.SurveySubjects.Any(z => z.Questions.Any(c => c.SurveyResponses.Any())));
            if (User.Identity!.IsAuthenticated)
            {
                subjects = subjects.Where(x => x.Surveyors.Any(c=>c.ActiveStatus && c.SurveyorId == _usermanager.GetUserId(User)));
            }

            return View(subjects);
        }

        [Authorize]
        public async Task<IActionResult> SubjectsResult(int? id)
        {
            if (id == null)
            {
                return Content("Subject category not specified");
            }

            ViewBag.SurveyName = _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor).SingleOrDefault(x => x.Id == id)?.Name;
            var questionResults = _context.SurveySubject.Include(x => x.Questions).ThenInclude(c => c.SurveyResponses)
                .Include(c => c.Category.Survey.Surveyors).ThenInclude(c=>c.Surveyor)
                .Include(x => x.Survey.Surveyors).ToList()
                .Where(x => x.Questions.Any(z => z.SurveyResponses.Any())).Where(z => z.SurveyId == id)
                .Where(x => x.Survey.Surveyors.Any(c=>c.ActiveStatus && c.SurveyorId == _usermanager.GetUserId(User)));

            return View(questionResults.ToList());
        }

        [Authorize]
        public async Task<IActionResult> QuestionResults(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scoreResponse = _context.SurveyResponse.Include(c=>c.question.ResponseType).Include(c=>c.question.QuestionGroup)
                .Where(a => a.question.SubjectId == id).ToList().Sum(x =>
            {
                if (x?.Response != null) return x.Response;
                return 0;
            });
            var questionResponsesCount = _context.Question.Where(a => a.SubjectId == id).ToList().Take(1).SelectMany(x => x.SurveyResponses)?.Count();
            var totalResponse = _context.SurveyResponse.Where(a => a.question.SubjectId == id).Count(a => a.question.SubjectId == id);
            var avgScore = Math.Round(scoreResponse / (decimal)totalResponse, 0);
            var surveySubject = _context.SurveySubject.SingleOrDefault(z => z.Id == id);
            ViewBag.Time = (DateTime.Now - DateTime.Now).Minutes;
            ViewBag.CategoryId = surveySubject?.CategoryId;
            ViewBag.SubjectName = surveySubject?.Name;
            ViewBag.SurveyId = surveySubject?.SurveyId;
            ViewBag.avgScore = avgScore;
            ViewBag.SubjectResult = _context.ResponseType.ToList().SelectMany(x => x.ResponseDictionary).FirstOrDefault( x => x.Value == avgScore)?.Value;
            var questionResults = _context.Question.Include(c=>c.SurveyResponses).Where(z => z.SubjectId == id).Where(x => x.SurveyResponses.Any());
            return View(await questionResults.ToListAsync());
        }
    }
}