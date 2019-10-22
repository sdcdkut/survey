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
    public class QuestionsController : Controller
    {
        private readonly SurveyContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;

        public QuestionsController(SurveyContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
        }

        // GET: Questions
        [Authorize]
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.subjectid = id;
            ViewBag.SurveyId = _context.SurveyCategory.SingleOrDefault(x=>x.SurveySubjects.Any(y=>y.Id==id))?.SurveyId;
            ViewBag.CategoryId = _context.SurveySubject.SingleOrDefault(x=>x.Id==id)?.CategoryId;
            ViewBag.SubjectName = _context.SurveySubject.SingleOrDefault(x=>x.Id==id)?.SubjectName;
            ViewBag.ResponType = _context.ResponseType.Count(x=>x.Subject.Id == id);
            var surveyContext = _context.Question.Include(q => q.ResponseType).Include(q => q.Subject).Where(x=>x.SubjectId == id);
            return View(await surveyContext.ToListAsync());
        }

        // GET: Questions/Details/5
        [NoDirectAccess]
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Question
                .Include(q => q.ResponseType)
                .Include(q => q.Subject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: Questions/Create
        [NoDirectAccess]
        [Authorize]
        [NoDirectAccess]
        public IActionResult Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.subjectid = id;
            var responseTypeId = _context.ResponseType.SingleOrDefault(x => x.SubjectId == id)?.Id;
            ViewBag.ResponseTypeId = responseTypeId;
            //ViewData["ResponseTypeId"] = new SelectList(_context.ResponseType.Where(x=>x.SubjectId == id), "Id", "ResponseName");
            /*ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id");*/
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [NoDirectAccess]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,SubjectId,ResponseTypeId,question")] Question question, int SubjectId, int ResponseTypeId,string[] quiz)
        {
            //var stns = new {question.question,question.ResponseTypeId,question.SubjectId};
            /*if (ModelState.IsValid)
            {*/
            if (quiz.Length>0)
            {
                foreach (var newquiz in quiz)
                {
                    var newQuiz = new Question()
                    {
                        SubjectId = SubjectId,
                        ResponseTypeId = ResponseTypeId,
                        question = newquiz
                    };
                    _context.Add(newQuiz);
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index),new {id=SubjectId});
            }
            //}
            ViewData["ResponseTypeId"] = new SelectList(_context.ResponseType, "Id", "Id", question.ResponseTypeId);
            ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id", question.SubjectId);
            return View(question);
        }
        
        [NoDirectAccess]
        public async Task<IActionResult> AssociateQuestion(int? subId, int? id)
        {
            if (subId != null && id != null)
            {
                var questionAssociate = _context.SurveySubject.Include(x=>x.Questions).SingleOrDefault(x => x.Id == subId);
               //var responseType = _context.ResponseType.SingleOrDefault(x => x.SubjectId == subId);
               if (questionAssociate?.Questions != null)
               {
                   foreach (var newquiz in questionAssociate.Questions)
                   {
                       var newQuiz = new Question()
                       {
                           SubjectId = (int) id,
                           ResponseTypeId = (int) newquiz.ResponseTypeId,
                           question = newquiz.question
                       };
                       _context.Add(newQuiz);
                   }
   
                   await _context.SaveChangesAsync();
                   return RedirectToAction(nameof(Index),new {id=id});
               }
                   
            }
            return RedirectToAction(nameof(Index),new {id=id});
        }

        // GET: Questions/Edit/5
        [NoDirectAccess]
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Question.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            ViewData["ResponseTypeId"] = new SelectList(_context.ResponseType.Where(x=>x.SubjectId == question.SubjectId), "Id", "ResponseName", question.ResponseTypeId);
            //ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id", question.SubjectId);
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [NoDirectAccess]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ResponseTypeId,question,SubjectId")] Question newquestion,int Id,int ResponseTypeId,string question)
        {
            if (id != Id)
            {
                return NotFound();
            }
            if (/*ModelState.IsValid*/Id !=null && ResponseTypeId != null && !string.IsNullOrEmpty(question))
            {
                var editquiz = _context.Question.SingleOrDefault(x=>x.Id == Id);
                try
                {
                    editquiz.ResponseTypeId = ResponseTypeId;
                    editquiz.question = question;
                    
                    _context.Update(editquiz);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index),new{id=editquiz.SubjectId});
            }
            ViewData["ResponseTypeId"] = new SelectList(_context.ResponseType, "Id", "ResponseName", ResponseTypeId);
            //ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id", question.SubjectId);
            return View(question);
        }

        // GET: Questions/Delete/5
        [Authorize(Roles = "Surveyor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Question
                .Include(q => q.ResponseType)
                .Include(q => q.Subject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Questions/Delete/5
        [NoDirectAccess]
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Question.FindAsync(id);
            _context.Question.Remove(question);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index),new{id=question.SubjectId});
        }
    
        private bool QuestionExists(int id)
        {
            return _context.Question.Any(e => e.Id == id);
        }

        [NoDirectAccess]
        public IActionResult SurveyQuestion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.SubjectName = _context.SurveySubject.SingleOrDefault(x=>x.Id==id)?.SubjectName;
            ViewBag.SurveyId = _context.SurveySubject.Include(x=>x.Category).SingleOrDefault(x => x.Id == id)?.CategoryId;
            var questions = _context.Question.Include(x=>x.ResponseType).Include(x=>x.Subject.Category.Survey).Where(x => x.SubjectId == id);
            //if user is logged in
            if (User.Identity.IsAuthenticated)
            {
                //if user has answered the question
                if (_context.SurveyResponse.Where(x=>x.question.Subject.Id == id).Any(x=>x.RespondantId==_usermanager.GetUserId(User)))
                {
                    questions = questions.Where(x =>
                                        x.SurveyResponses.Any(z => z.RespondantId != _usermanager.GetUserId(User)));
                }
                
            }
            return View(questions);
        }

    }
}
