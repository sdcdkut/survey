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
            var surveySubject = _context.SurveySubject.SingleOrDefault(x => x.Id == id);
            ViewBag.SurveyId = surveySubject?.SurveyId;
            ViewBag.CategoryId = surveySubject?.CategoryId;
            ViewBag.SubjectName = surveySubject?.Name;
            //ViewBag.ResponType = _context.ResponseType.Count(x=>x.Subject.Id == id);
            var surveyContext = _context.Question.Include(q => q.ResponseType).Include(c => c.QuestionGroup)
                .Include(q => q.Subject.Survey).Where(x => x.SubjectId == id);
            var useId = _usermanager.GetUserId(User);
            var survey = await _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor).FirstOrDefaultAsync(c => c.Id == surveySubject.SurveyId);
            if (survey != null && !survey.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId))
            {
                return StatusCode(403);
            }

            return View(await surveyContext.ToListAsync());
        }

        // GET: Questions/Details/5
        //[NoDirectAccess]
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
        [Authorize]
        //[NoDirectAccess]
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.subjectid = id;

            // ViewBag.ResponseTypeId = responseTypeId;
            var subject = _context.SurveySubject.Include(x => x.Category).SingleOrDefault(x => x.Id == id);
            ViewBag.SurveyId = subject?.SurveyId;
            ViewBag.CategoryId = subject?.CategoryId;
            ViewBag.SubjectId = id;
            ViewBag.responseTypeId = _context.ResponseType.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.ResponseName} ({c.DisplayOptionType})",
                Selected = c.Id == subject.ResponseTypeId
            });
            ViewBag.QuestionGroupId = _context.QuestionGroups.Where(c => c.SubjectId == id).Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Name}",
            });
            var useId = _usermanager.GetUserId(User);
            var survey = await _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor).FirstOrDefaultAsync(c => c.Id == subject.SurveyId);
            if (survey != null && !survey.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId))
            {
                return StatusCode(403);
            }

            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[NoDirectAccess]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Dictionary<int, Question> Question, int subjectId)
        {
            //var stns = new {question.question,question.ResponseTypeId,question.SubjectId};
            /*if (ModelState.IsValid)
            {*/
            if (Question.Any())
            {
                foreach (var newQuiz in Question.Select(newquiz => new Question
                {
                    SubjectId = newquiz.Value.SubjectId,
                    ResponseTypeId = newquiz.Value.ResponseTypeId,
                    question = newquiz.Value?.question,
                    QuestionGroupId = newquiz.Value?.QuestionGroupId,
                    AnswerRequired = newquiz.Value.AnswerRequired
                }))
                {
                    _context.Add(newQuiz);
                }

                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"Question(s) added successfully";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return RedirectToAction("_SuccessSurveySetupComplete", new { id = subjectId });
                }

                return RedirectToAction(nameof(Index), new { id = subjectId });
            }

            //}
            var surveySubject = await _context.SurveySubject.FindAsync(subjectId);
            ViewBag.subjectid = subjectId;
            ViewBag.responseTypeId = _context.ResponseType.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.ResponseName} ({c.DisplayOptionType})",
                Selected = c.Id == surveySubject.ResponseTypeId
            });
            ViewBag.QuestionGroupId = _context.QuestionGroups.Where(c => c.SubjectId == subjectId).Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Name}",
            });
            return View();
        }

        //[NoDirectAccess]
        [Authorize]
        public async Task<IActionResult> AssociateQuestion(int? subId, int? id)
        {
            if (subId != null && id != null)
            {
                var useId = _usermanager.GetUserId(User);
                var subject = await _context.SurveySubject.Include(x => x.Category).SingleOrDefaultAsync(x => x.Id == id);
                var survey = await _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor).FirstOrDefaultAsync(c => c.Id == subject.SurveyId);
                if (survey != null && !survey.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId))
                {
                    return StatusCode(403);
                }

                var questionAssociate = _context.SurveySubject.Include(x => x.Questions).ThenInclude(c => c.QuestionGroup).SingleOrDefault(x => x.Id == subId);
                //var responseType = _context.ResponseType.SingleOrDefault(x => x.SubjectId == subId);
                if (questionAssociate?.Questions != null)
                {
                    var counter = 0;
                    foreach (var newquiz in questionAssociate.Questions)
                    {
                        if (_context.Question.Any(c => c.question == newquiz.question && c.SubjectId == (int)id)) continue;
                        var newQuiz = new Question
                        {
                            SubjectId = (int)id,
                            ResponseTypeId = newquiz.ResponseTypeId,
                            question = newquiz.question,
                            QuestionGroupId = newquiz.QuestionGroupId
                        };
                        await _context.Question.AddAsync(newQuiz);
                        counter++;
                    }

                    await _context.SaveChangesAsync();
                    TempData["FeedbackMessage"] = $"{counter} Question(s) added successfully";
                    return RedirectToAction(nameof(Index), new { id });
                }
            }

            return RedirectToAction(nameof(Index), new { id = id });
        }

        // GET: Questions/Edit/5
        //[NoDirectAccess]
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Question.Include(x => x.Subject.Category).SingleOrDefaultAsync(x => x.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            var useId = _usermanager.GetUserId(User);
            var survey = await _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor).FirstOrDefaultAsync(c => c.Id == question.Subject.SurveyId);
            if (!survey.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId))
            {
                return StatusCode(403);
            }

            ViewBag.SurveyId = question.Subject.SurveyId;
            ViewBag.CategoryId = question.Subject?.CategoryId;
            ViewBag.responseTypeId = _context.ResponseType.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.ResponseName} ({c.DisplayOptionType})",
                Selected = c.Id == question.ResponseTypeId
            });
            ViewBag.QuestionGroupId = _context.QuestionGroups.Where(c => c.SubjectId == question.SubjectId).Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Name}",
                Selected = question.QuestionGroupId == c.Id
            });
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        //[NoDirectAccess]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ResponseTypeId,question,QuestionGroupId,SubjectId,AnswerRequired")] Question newquestion)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(newquestion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["FeedbackMessage"] = $"Question edited successfully";
                return RedirectToAction(nameof(Index), new { id = newquestion.SubjectId });
            }

            ViewBag.responseTypeId = _context.ResponseType.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.ResponseName} ({c.DisplayOptionType})",
                Selected = c.Id == newquestion.ResponseTypeId
            });
            ViewBag.QuestionGroupId = _context.QuestionGroups.Where(c => c.SubjectId == newquestion.SubjectId).Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Name}",
                Selected = newquestion.QuestionGroupId == c.Id
            });
            return View(newquestion);
        }

        // GET: Questions/Delete/5
        [Authorize( /*Roles = "Surveyor"*/)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Question
                .Include(q => q.ResponseType)
                .Include(q => q.Subject.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            var useId = _usermanager.GetUserId(User);
            var survey = await _context.Survey.Include(c => c.Surveyors).FirstOrDefaultAsync(c => c.Id == question.Subject.SurveyId);
            if (survey?.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId) == false)
            {
                return StatusCode(403);
            }

            ViewBag.SurveyId = question.Subject.SurveyId;
            ViewBag.CategoryId = question.Subject?.CategoryId;
            return View(question);
        }

        // POST: Questions/Delete/5
        //[NoDirectAccess]
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Question.FindAsync(id);
            _context.Question.Remove(question);
            await _context.SaveChangesAsync();
            TempData["FeedbackMessage"] = $"Question deleted successfully";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return RedirectToAction("_CreatePartial", "Questions");
            }

            return RedirectToAction(nameof(Index), new { id = question.SubjectId });
        }

        private bool QuestionExists(int id)
        {
            return _context.Question.Any(e => e.Id == id);
        }

        //[NoDirectAccess]
        public IActionResult SurveyQuestion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveySubject = _context.SurveySubject.Include(x => x.Category).SingleOrDefault(x => x.Id == id);
            var survey = _context.Survey.Include(c => c.SurveyParticipants).ToList().FirstOrDefault(c => c.Id == surveySubject?.SurveyId);
            var currentUser = _context.Users.FirstOrDefault(c => c.Id == _usermanager.GetUserId(User));
            var canTakeSurvey = false;

            if (survey is not null)
            {
                switch (survey.status)
                {
                    case "Open":
                        canTakeSurvey = true;
                        if (survey.ForStudents)
                        {
                            canTakeSurvey = currentUser?.UserType == UserType.Student;
                        }

                        if (survey?.CourseId is not null)
                        {
                            canTakeSurvey = currentUser?.CourseId == survey?.CourseId;
                        }

                        break;
                    case "Closed":
                        canTakeSurvey = User.Identity?.IsAuthenticated is true;
                        if (survey.ForStudents)
                        {
                            canTakeSurvey = currentUser?.UserType == UserType.Student;
                        }

                        if (survey?.CourseId is not null)
                        {
                            canTakeSurvey = currentUser?.CourseId == survey?.CourseId;
                        }

                        break;
                    case "SelectiveParticipants":
                        canTakeSurvey = survey.SurveyParticipants.Any(c => c.ParticipantId == currentUser?.Id);
                        if (survey.ForStudents)
                        {
                            canTakeSurvey = currentUser?.UserType == UserType.Student;
                        }

                        if (survey?.CourseId is not null)
                        {
                            canTakeSurvey = currentUser?.CourseId == survey?.CourseId;
                        }

                        break;
                }
            }

            if (canTakeSurvey == false)
            {
                return Content("SORRY, You are not allowed to take the survey");
            }

            ViewBag.SubjectName = _context.SurveySubject.SingleOrDefault(x => x.Id == id)?.Name;
            ViewBag.SurveyId = surveySubject?.SurveyId;
            var questions = _context.Question.Include(x => x.ResponseType).Include(c => c.SurveyResponses).Include(c => c.QuestionGroup)
                .Include(x => x.Subject.Category.Survey.SurveyParticipants)
                .Include(x => x.Subject.Survey.SurveyParticipants).Where(x => x.SubjectId == id).ToList();
            //if user is logged in
            if (User.Identity!.IsAuthenticated)
            {
                //if user has answered the question
                if (_context.SurveyResponse.Where(x => x.question.Subject.Id == id).Any(x => x.RespondantId == _usermanager.GetUserId(User)))
                {
                    questions = questions.Where(x =>
                        x.SurveyResponses.Any(z => z.RespondantId != _usermanager.GetUserId(User))).ToList();
                }
            }

            return View(questions);
        }

        public async Task<IActionResult> _CreatePartial(int? id, int? subjectid)
        {
            if (id == null || subjectid == null)
            {
                return NotFound();
            }

            var subject = await _context.SurveySubject.Include(x => x.Category).SingleOrDefaultAsync(x => x.Id == subjectid);
            var useId = _usermanager.GetUserId(User);
            var survey = await _context.Survey.Include(c => c.Surveyors).FirstOrDefaultAsync(c => c.Id == subject.SurveyId);
            if (survey?.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId) == false)
            {
                return StatusCode(403);
            }

            ViewBag.subjectid = subjectid;
            ViewBag.ResponseTypeId = id;
            return PartialView(new Question());
        }

        public IActionResult _SuccessSurveySetupComplete(int id)
        {
            ViewBag.subjectid = id;
            ViewBag.SurveyId = _context.SurveySubject.SingleOrDefault(x => x.Id == id)?.SurveyId;
            ViewBag.CategoryId = _context.SurveySubject.SingleOrDefault(x => x.Id == id)?.CategoryId;
            ViewBag.SubjectName = _context.SurveySubject.SingleOrDefault(x => x.Id == id)?.Name;
            ViewBag.ResponType = _context.ResponseType.Count();
            var surveyContext = _context.Question.Include(q => q.ResponseType).Include(q => q.Subject).Where(x => x.SubjectId == id);
            return PartialView("_SuccessSurveySetupComplete", surveyContext);
        }
    }
}