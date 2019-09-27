using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Surveyapp.Models;

namespace Surveyapp.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly SurveyContext _context;

        public QuestionsController(SurveyContext context)
        {
            _context = context;
        }

        // GET: Questions
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.subjectid = id;
            var surveyContext = _context.Question.Include(q => q.ResponseType).Include(q => q.Subject).Where(x=>x.SubjectId == id);
            return View(await surveyContext.ToListAsync());
        }

        // GET: Questions/Details/5
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
        public IActionResult Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.subjectid = id;
            var responseTypeId = _context.SurveySubject.SingleOrDefault(x => x.Id == id)?.Id;
            ViewBag.responseTypeId = responseTypeId;
            ViewData["ResponseTypeId"] = new SelectList(_context.ResponseType.Where(x=>x.SubjectId == id), "Id", "ResponseName");
            /*ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id");*/
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Questions/Edit/5
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
            ViewData["ResponseTypeId"] = new SelectList(_context.ResponseType, "Id", "ResponseName", question.ResponseTypeId);
            //ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id", question.SubjectId);
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        public IActionResult SurveyQuestion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questions = _context.Question.Include(x=>x.ResponseType).Where(x => x.SubjectId == id);
            //throw new NotImplementedException();
            return View(questions);
        }
    }
}
