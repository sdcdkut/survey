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


        public SurveyResponsesController(SurveyContext context, UserManager<ApplicationUser>userManager)
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
        public async Task<IActionResult> Create([Bind("Id,RespondantId,QuestionId,Response")] SurveyResponse surveyResponse, string[] quizresponse)
        {
            if (/*ModelState.IsValid*/quizresponse.Length>0)
            {
                int? categoryId= null;
                foreach (var quizeAndResponse in quizresponse)
                {
                    string[] ids = quizeAndResponse.Split(new char[] { '|' });
                    string response = ids[0];
                    int quizId = int.Parse(ids[1]);
                    string RespondantId = null;
                    if (User.Identity.IsAuthenticated)
                    {
                        RespondantId = _usermanager.GetUserId(User);
                    }
                    SurveyResponse newResponse = new SurveyResponse()
                    {
                        QuestionId=quizId,
                        Response = response,
                        RespondantId = RespondantId
                    };
                    /*foreach (var id in ids)
                    {
                        var sd = id;
                    }*/
                    categoryId = _context.SurveySubject.SingleOrDefault(x => x.Questions.Any(y=>y.Id == newResponse.QuestionId))?.CategoryId;
                    _context.Add(newResponse);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("SurveySubjects","SurveySubjects", new{id = categoryId});
            }
            ViewData["RespondantId"] = new SelectList(_context.Users, "Id", "Id", surveyResponse.RespondantId);
            ViewData["QuestionId"] = new SelectList(_context.Question, "Id", "Id", surveyResponse.QuestionId);
            return View(surveyResponse);
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
        public async Task<IActionResult> SurveyResults (int? id)
        {
            IQueryable<Survey> subjects = _context.Survey.Include(x=>x.SurveyCategorys)
                .Where(x=>x.SurveyCategorys.Any(a=>a.SurveySubjects.Any(z=>z.Questions.Any(c=>c.SurveyResponses.Any()))));
            if (User.Identity.IsAuthenticated)
            {
                subjects = subjects.Where(x => x.SurveyerId == _usermanager.GetUserId(User));
            }
            return  View(subjects);
        }
        [Authorize]
        public async Task<IActionResult> SubjectsResult(int? id)
        {
            if (id == null)
            {
                return Content("Subject category not specified");
            }
            var questionResults = _context.SurveySubject.Include(x=>x.Questions).Include(x=>x.ResponseTypes)
                                    .Where(x=>x.Questions.Any(z=>z.SurveyResponses.Any())).Where(z=>z.CategoryId==id)
                                    .Where(x=>x.Category.Survey.SurveyerId == _usermanager.GetUserId(User));
           
            return View(questionResults);
        }
        [Authorize]
        public async Task<IActionResult> QuestionResults(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.Time = EF.Functions.DateDiffMinute(DateTime.Now, DateTime.Now);
            ViewBag.CategoryId = _context.SurveySubject.SingleOrDefault(z=>z.Id == id)?.CategoryId;
            var questionResults = _context.Question.Where(z=>z.SubjectId==id).Where(x=>x.SurveyResponses.Any());
            return View(questionResults);
        }
    }
}
