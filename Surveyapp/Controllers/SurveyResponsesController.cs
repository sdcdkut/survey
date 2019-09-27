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
        public async Task<IActionResult> Index()
        {
            var surveyContext = _context.SurveyResponse.Include(s => s.Respondant).Include(s => s.question);
            return View(await surveyContext.ToListAsync());
        }

        // GET: SurveyResponses/Details/5
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
        public IActionResult Create()
        {
            ViewData["RespondantId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["QuestionId"] = new SelectList(_context.Question, "Id", "Id");
            return View();
        }

        // POST: SurveyResponses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RespondantId,QuestionId,Response")] SurveyResponse surveyResponse, string[] quizresponse)
        {
            if (/*ModelState.IsValid*/quizresponse.Length>0)
            {
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
                    _context.Add(newResponse);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RespondantId"] = new SelectList(_context.Users, "Id", "Id", surveyResponse.RespondantId);
            ViewData["QuestionId"] = new SelectList(_context.Question, "Id", "Id", surveyResponse.QuestionId);
            return View(surveyResponse);
        }

        // GET: SurveyResponses/Edit/5
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
        }

        // GET: SurveyResponses/Delete/5
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
        public IActionResult SurveyResults(int? id)
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
