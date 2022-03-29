using System;
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
    public class SurveysController : Controller
    {
        private readonly SurveyContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;

        public SurveysController(SurveyContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
        }

        // GET: Surveys
        [Authorize( /*Roles = "Surveyor"*/)]
        public async Task<IActionResult> Index()
        {
            var surveyContext = _context.Survey.Include(s => s.Surveyors).ThenInclude(c => c.Surveyor)
                .Include(c => c.SurveyParticipants)
                .Where(x => x.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == _usermanager.GetUserId(User)));
            return View(await surveyContext.ToListAsync());
        }

        // GET: Surveys/Details/5
        //[NoDirectAccess]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _usermanager.GetUserId(User);
            var survey = await _context.Survey
                .Include(s => s.Surveyors).ThenInclude(c => c.Surveyor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (survey == null)
            {
                return NotFound();
            }

            if (!survey.Surveyors.Any(x => x.ActiveStatus && x.SurveyorId == userId))
            {
                return StatusCode(403);
            }

            return View(survey);
        }

        // GET: Surveys/Create
        public IActionResult Create()
        {
            ViewData["SurveyerId"] = new SelectList(_context.Users, "Id", "Id");
            ViewBag.Participants = _context.Users.Select(c => new SelectListItem
            {
                Value = c.Id,
                Text = c.UserName
            });
            ViewBag.courses = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}"
            });
            ViewBag.departments = _context.Departments.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}"
            });
            return View();
        }

        // POST: Surveys/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize( /*Roles = "Surveyor"*/)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Startdate,Description,EndDate,status,ListedOnSurveyListPage,CourseId,DepartmentId,ForStudents,ForStaff")] Survey survey,
            string[] participants)
        {
            //survey.SurveyerId = _usermanager.GetUserId(User);
            survey.approvalStatus = "Approved";
            //survey.status = survey.status.ToString();
            if (survey != null)
            {
                _context.Survey.Add(survey);
                await _context.SaveChangesAsync();
                _context.Surveyors.Add(new Surveyors
                {
                    Permission = SurveyPermission.AllPermissions,
                    SurveyorId = _usermanager.GetUserId(User),
                    ActiveStatus = true,
                    SurveyId = survey.Id
                });
                await _context.SaveChangesAsync();
                if (participants.Any())
                {
                    foreach (var participant in participants)
                    {
                        var newParticipants = new SurveyParticipants
                        {
                            ParticipantId = participant,
                            SurveyId = survey.Id
                        };
                        await _context.SurveyParticipants.AddAsync(newParticipants);
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["FeedbackMessage"] = $"survey added {survey.Name} successfully";
                var ajax = Request.Headers["X-Requested-With"];
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return RedirectToAction("_CreateSurveyCategoryPartial", "SurveyCategories", new { id = survey.Id });
                }

                ViewBag.Participants = _context.Users.Select(c => new SelectListItem
                {
                    Value = c.Id,
                    Text = c.UserName,
                    Selected = participants.Contains(c.Id)
                });
                ViewBag.courses = _context.Courses.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code}:{c.Name}",
                    Selected = c.Id == survey.CourseId
                });
                ViewBag.departments = _context.Departments.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code}:{c.Name}",
                    Selected = c.Id == survey.DepartmentId
                });
                return RedirectToAction(nameof(Create), "SurveySubjects", new { id = survey.Id });
            }

            ViewBag.Participants = _context.Users.Select(c => new SelectListItem
            {
                Value = c.Id,
                Text = c.UserName
            });
            ViewBag.courses = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == survey.CourseId
            });
            ViewBag.departments = _context.Departments.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == survey.DepartmentId
            });
            return View(survey);
        }

        // GET: Surveys/Edit/5
        //[NoDirectAccess]
        [Authorize( /*Roles = "Surveyor"*/)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var survey = await _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor)
                .Include(c => c.SurveyParticipants).FirstOrDefaultAsync(c => c.Id == id);
            if (survey == null)
            {
                return NotFound();
            }

            var userId = _usermanager.GetUserId(User);
            if (!survey.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == userId))
            {
                return StatusCode(403);
            }

            ViewData["SurveyerId"] = _context.Users.ToList().Select(c => new SelectListItem
            {
                Value = c.Id,
                Text = c.UserName,
                Selected = survey.Surveyors.Any(x => x.SurveyorId == c.Id)
            }).ToList();
            ViewBag.Participants = _context.Users.ToList().Select(c => new SelectListItem
            {
                Value = c.Id,
                Text = c.UserName,
                Selected = survey.SurveyParticipants.Any(v => v.ParticipantId == c.Id)
            });
            ViewBag.courses = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == survey.CourseId
            });
            ViewBag.departments = _context.Departments.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == survey.DepartmentId
            });
            return View(survey);
        }

        // POST: Surveys/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[NoDirectAccess]
        [Authorize( /*Roles = "Surveyor"*/)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Name,Startdate,EndDate,status,Description,SurveyerId,approvalStatus,ListedOnSurveyListPage,SurveyParticipants,CourseId,DepartmentId,ForStudents,ForStaff")]
            Survey survey, string[] SurveyParticipants)
        {
            var surveyEdit = await _context.Survey.Include(c => c.SurveyParticipants).FirstOrDefaultAsync(c => c.Id == id);
            if (surveyEdit == null)
            {
                return NotFound();
            }

            //ModelState.Remove<Survey>(x => x.SurveyerId);
            /*survey.SurveyerId = _usermanager.GetUserId(User);*/
            /*if (survey.Surveyors.Any(c=>c.ActiveStatus && c.SurveyorId ==_usermanager.GetUserId(User)))
            {*/
            //survey.SurveyParticipants = SurveyParticipants.Select(c => new SurveyParticipants { ParticipantId = c, SurveyId = survey.Id}).ToList();
            if (ModelState.IsValid)
            {
                try
                {
                    surveyEdit.Name = survey.Name;
                    surveyEdit.Startdate = survey.Startdate;
                    surveyEdit.EndDate = survey.EndDate;
                    surveyEdit.status = survey.status;
                    surveyEdit.Description = survey.Description;
                    surveyEdit.approvalStatus = survey.approvalStatus;
                    surveyEdit.ListedOnSurveyListPage = survey.ListedOnSurveyListPage;
                    surveyEdit.SurveyParticipants = SurveyParticipants?.Select(c => new SurveyParticipants { ParticipantId = c, SurveyId = survey.Id }).ToList();
                    surveyEdit.CourseId = survey?.CourseId;
                    surveyEdit.DepartmentId = survey?.DepartmentId;
                    surveyEdit.ForStudents = survey?.ForStudents ?? false;
                    surveyEdit.ForStaff = survey?.ForStaff ?? false;
                    _context.Update(surveyEdit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SurveyExists(survey.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                TempData["FeedbackMessage"] = $"{survey.Name} edited successfully";
                return RedirectToAction(nameof(Index));
            }
            /*}
            else
            {
                return StatusCode(403);
            }*/

            ViewData["SurveyerId"] = _context.Users.ToList().Select(c => new SelectListItem
            {
                Value = c.Id,
                Text = c.UserName,
                Selected = survey.Surveyors.Any(x => x.SurveyorId == c.Id)
            }).ToList();
            ViewBag.Participants = _context.Users.ToList().Select(c => new SelectListItem
            {
                Value = c.Id,
                Text = c.UserName,
                Selected = survey.SurveyParticipants.Any(v => v.ParticipantId == c.Id)
            });
            ViewBag.courses = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == survey.CourseId
            });
            ViewBag.departments = _context.Departments.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == survey.DepartmentId
            });
            return View(survey);
        }

        // GET: Surveys/Delete/5
        //[NoDirectAccess]
        [Authorize( /*Roles = "Surveyor"*/)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var survey = await _context.Survey
                .Include(s => s.Surveyors).ThenInclude(c => c.Surveyor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (survey == null)
            {
                return NotFound();
            }

            var userId = _usermanager.GetUserId(User);
            if (!survey.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == userId))
            {
                return StatusCode(403);
            }

            return View(survey);
        }

        // POST: Surveys/Delete/5
        //[NoDirectAccess]
        [Authorize( /*Roles = "Surveyor"*/)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var survey = await _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor).FirstOrDefaultAsync(c => c.Id == id);
            if (survey.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == _usermanager.GetUserId(User)))
            {
                _context.Survey.Remove(survey);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"{survey.Name} deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return StatusCode(403);
            }
        }

        private bool SurveyExists(int id)
        {
            return _context.Survey.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Surveys()
        {
            var surveyContext = await _context.SurveySubject.Include(s => s.Survey.Surveyors).ThenInclude(c => c.Surveyor)
                .Include(x => x.Category).Include(c => c.Questions).Include(c => c.Survey.SurveyParticipants).ToListAsync();
            if (User.Identity!.IsAuthenticated)
            {
                //display surveys not created by current logged in user
                surveyContext = surveyContext.Where(x => x.Survey.Surveyors.All(c => c.ActiveStatus && c.SurveyorId != _usermanager.GetUserId(User))).ToList();
            }

            surveyContext = surveyContext.Where(x => x.Questions.Any()).Where(x => x.Survey.approvalStatus == "Approved" && !x.DynamicallyCreated && x.Survey.ListedOnSurveyListPage)
                .ToList();
            return View(surveyContext);
        }

        //[NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveSurveys()
        {
            var surveyContext = _context.Survey.Include(s => s.Surveyors).ThenInclude(c => c.Surveyor);
            return View(await surveyContext.ToListAsync());
        }

        //[NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeApproval(int? id, string Approvalstate)
        {
            if (id == null) return NotFound();
            var surveyEdit = _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor).SingleOrDefault(x => x.Id == id);
            if (surveyEdit != null)
            {
                surveyEdit.approvalStatus = Approvalstate;
                _context.Survey.Update(surveyEdit);
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

        [Authorize]
        public async Task<IActionResult> SurveyUsersAccess(int id)
        {
            var surveyorsList = await _context.Surveyors.Include(c => c.Surveyor).Include(c => c.Survey)
                .Where(c => c.SurveyId == id).ToListAsync();
            ViewBag.SurveyId = id;
            return View(surveyorsList);
        }

        [Authorize]
        public async Task<IActionResult> DeleteUserAccess(Guid id)
        {
            var surveyor = await _context.Surveyors.FindAsync(id);
            if (surveyor != null)
            {
                _context.Surveyors.Remove(surveyor);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"survey removed user access";
                return RedirectToAction(nameof(SurveyUsersAccess), new { id = surveyor?.SurveyId });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreateUserAccess(int id)
        {
            var survey = await _context.Survey.Include(c => c.Surveyors).SingleOrDefaultAsync(c => c.Id == id);
            var surveyorsIds = survey?.Surveyors.Select(c => c.SurveyorId).ToList();
            ViewBag.SurveyorId = _context.Users.ToList().Where(c => surveyorsIds != null && !surveyorsIds.Contains(c.Id)).Select(x =>
                new SelectListItem
                {
                    Text = x.UserName,
                    Value = x.Id
                }).ToList();
            ViewBag.SurveyId = survey?.Id;
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateUserAccess([Bind("SurveyorId,SurveyId,ActiveStatus,Permission,Owner")] Surveyors surveyors)
        {
            surveyors.Owner = false;
            if (ModelState.IsValid)
            {
                _context.Surveyors.Add(surveyors);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"survey user access saved";
                return RedirectToAction(nameof(SurveyUsersAccess), new { id = surveyors.SurveyId });
            }

            var survey = await _context.Survey.Include(c => c.Surveyors).SingleOrDefaultAsync(c => c.Id == surveyors.SurveyId);
            var surveyorsIds = survey?.Surveyors.Select(c => c.SurveyorId).ToList();
            ViewBag.SurveyorId = _context.Users.ToList().Where(c => surveyorsIds != null && !surveyorsIds.Contains(c.Id)).Select(x =>
                new SelectListItem
                {
                    Text = x.UserName,
                    Value = x.Id
                }).ToList();
            ViewBag.SurveyId = survey?.Id;
            return View(surveyors);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditUserAccess(Guid id)
        {
            var surveyor = await _context.Surveyors.FindAsync(id);
            return View(surveyor);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditUserAccess(Surveyors surveyors)
        {
            var surveyor = await _context.Surveyors.FindAsync(surveyors.Id);
            if (surveyor != null)
            {
                surveyor.Permission = surveyors.Permission;
                surveyor.ActiveStatus = surveyors.ActiveStatus;
                _context.Surveyors.Update(surveyor);
                TempData["FeedbackMessage"] = $"survey user access saved";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(SurveyUsersAccess), new { id = surveyors.SurveyId });
            }

            return View(surveyors);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ReOpenSurvey(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var survey = _context.Survey.Include(c => c.Surveyors).Include(c => c.SurveyParticipants).SingleOrDefault(c => c.Id == id);
            var surveySubjects = _context.SurveySubject.Where(c => c.SurveyId == id).ToList();
            ViewBag.surveySubjects = surveySubjects.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = true
            }).ToList();
            if (surveySubjects.Any(c => c.AddAnotherSubjectOnSurveyTake))
            {
                ViewBag.surveySubjects = surveySubjects.Where(c => c.AddAnotherSubjectOnSurveyTake).Take(1).Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = true
                }).ToList();
            }

            ViewBag.Participants = _context.Users.ToList().Select(c => new SelectListItem
            {
                Value = c.Id,
                Text = c.UserName,
                Selected = survey?.SurveyParticipants.Any(v => v.ParticipantId == c.Id) == true
            });
            ViewBag.courses = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == survey.CourseId
            });
            ViewBag.departments = _context.Departments.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == survey.DepartmentId
            });
            return View(survey);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ReOpenSurvey(int? id, string status, DateTime Startdate, DateTime EndDate, int[] surveySubjects, string[] participants)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = _context.SurveySubject.SingleOrDefault(c => c.Id == id);
            var survey = _context.Survey.Include(c => c.SurveyCategorys).ThenInclude(c => c.SurveySubjects).ThenInclude(c => c.Questions)
                .Include(c => c.SurveySubjects).ThenInclude(c => c.Questions).ThenInclude(c => c.QuestionGroup)
                .Include(c => c.Surveyors).Include(c => c.SurveyParticipants).FirstOrDefault(c => c.Id == id);
            var newSurvey = new Survey
            {
                status = status,
                approvalStatus = "Approved",
                Description = survey?.Description,
                Startdate = Startdate,
                EndDate = EndDate,
                Name = survey?.Name,
                ListedOnSurveyListPage = survey?.ListedOnSurveyListPage ?? true,
                CourseId = survey?.CourseId,
                DepartmentId = survey?.DepartmentId,
                ForStudents = survey?.ForStudents ?? false,
                ForStaff = survey?.ForStaff ?? false
            };
            try
            {
                await _context.Survey.AddAsync(newSurvey);
                await _context.SaveChangesAsync();
                survey?.Surveyors?.ToList().ForEach(async surveyors =>
                {
                    await _context.Surveyors.AddAsync(new Surveyors
                    {
                        Owner = surveyors.Owner,
                        Permission = surveyors.Permission,
                        ActiveStatus = surveyors.ActiveStatus,
                        SurveyId = newSurvey.Id,
                        SurveyorId = surveyors.SurveyorId,
                    });
                });
                participants?.ToList().ForEach(async surveyParticipants =>
                {
                    await _context.SurveyParticipants.AddAsync(new SurveyParticipants
                    {
                        ParticipantId = surveyParticipants,
                        SurveyId = newSurvey.Id
                    });
                });
                await _context.SaveChangesAsync();
                if (survey?.SurveyCategorys?.Any() == true)
                {
                    foreach (var category in survey.SurveyCategorys.ToList())
                    {
                        var newSurveyCategory = new SurveyCategory
                        {
                            CategoryName = category.CategoryName,
                            SurveyId = newSurvey.Id
                        };
                        await _context.SurveyCategory.AddAsync(newSurveyCategory);
                        await _context.SaveChangesAsync();
                        if (category.SurveySubjects?.Any() != true) continue;
                        foreach (var surveySubject in category.SurveySubjects.Where(c => surveySubjects.Contains(c.Id)).ToList())
                        {
                            var newSurveySubject = new SurveySubject
                            {
                                Description = surveySubject.Description,
                                Name = surveySubject.Name,
                                CategoryId = newSurveyCategory.Id,
                                OtherProperties = surveySubject.OtherProperties,
                                ResponseTypeId = surveySubject.ResponseTypeId,
                                SurveyId = newSurvey.Id,
                                DynamicallyCreated = surveySubject.DynamicallyCreated,
                                DynamicSubjectValue = surveySubject.DynamicSubjectValue,
                                AddAnotherSubjectOnSurveyTake = surveySubject.AddAnotherSubjectOnSurveyTake,
                                CourseId = surveySubject?.CourseId,
                                DepartmentId = surveySubject?.DepartmentId
                            };
                            if (_context.SurveySubject.Any(c => c == newSurveySubject)) continue;
                            await _context.SurveySubject.AddAsync(newSurveySubject);
                            await _context.SaveChangesAsync();
                            if (surveySubject.QuestionGroups.Any())
                            {
                                foreach (var group in surveySubject.QuestionGroups.ToList())
                                {
                                    var newQuestionGroup = new QuestionGroup
                                    {
                                        SubjectId = newSurveySubject.Id,
                                        Name = group.Name
                                    };
                                    await _context.QuestionGroups.AddAsync(newQuestionGroup);
                                    await _context.SaveChangesAsync();
                                    if (!@group.Questions.Any()) continue;
                                    foreach (var newQuestion in @group.Questions.ToList().Select(question => new Question
                                    {
                                        question = question.question,
                                        ResponseTypeId = question.ResponseTypeId,
                                        QuestionGroupId = newQuestionGroup.Id,
                                        SubjectId = newSurveySubject.Id,
                                        AnswerRequired = question.AnswerRequired
                                    }))
                                    {
                                        if (_context.Question.Any(c => c == newQuestion)) continue;
                                        await _context.Question.AddAsync(newQuestion);
                                        await _context.SaveChangesAsync();
                                    }
                                }
                            }
                            else
                            {
                                foreach (var newQuestion in surveySubject.Questions.ToList().Select(question => new Question
                                {
                                    question = question.question,
                                    ResponseTypeId = question.ResponseTypeId,
                                    SubjectId = surveySubject.Id,
                                    AnswerRequired = question.AnswerRequired
                                }))
                                {
                                    if (_context.Question.Any(c => c == newQuestion)) continue;
                                    await _context.Question.AddAsync(newQuestion);
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }

                if (survey?.SurveySubjects != null)
                    foreach (var surveySubject in survey.SurveySubjects.Where(c => surveySubjects.Contains(c.Id)).ToList())
                    {
                        var newSurveySubject = new SurveySubject
                        {
                            Description = surveySubject.Description,
                            Name = surveySubject.Name,
                            OtherProperties = surveySubject.OtherProperties,
                            ResponseTypeId = surveySubject.ResponseTypeId,
                            SurveyId = newSurvey.Id,
                            DynamicallyCreated = surveySubject.DynamicallyCreated,
                            DynamicSubjectValue = surveySubject.DynamicSubjectValue,
                            AddAnotherSubjectOnSurveyTake = surveySubject.AddAnotherSubjectOnSurveyTake,
                            CourseId = surveySubject?.CourseId,
                            DepartmentId = surveySubject?.DepartmentId
                        };
                        if (_context.SurveySubject.Any(c => c == newSurveySubject)) continue;
                        await _context.SurveySubject.AddAsync(newSurveySubject);
                        await _context.SaveChangesAsync();
                        if (surveySubject.QuestionGroups.Any())
                        {
                            foreach (var group in surveySubject.QuestionGroups.ToList())
                            {
                                var newQuestionGroup = new QuestionGroup
                                {
                                    SubjectId = newSurveySubject.Id,
                                    Name = group.Name
                                };
                                await _context.QuestionGroups.AddAsync(newQuestionGroup);
                                await _context.SaveChangesAsync();
                                if (!group.Questions.Any()) continue;
                                foreach (var newQuestion in group.Questions.ToList().Select(question => new Question
                                {
                                    question = question.question,
                                    ResponseTypeId = question.ResponseTypeId,
                                    QuestionGroupId = newQuestionGroup.Id,
                                    SubjectId = newSurveySubject.Id,
                                    AnswerRequired = question.AnswerRequired
                                }).Where(newQuestion => !_context.Question.Any(c => c == newQuestion)))
                                {
                                    await _context.Question.AddAsync(newQuestion);
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }
                        else
                        {
                            foreach (var newQuestion in surveySubject.Questions.ToList().Select(question => new Question
                            {
                                question = question.question,
                                ResponseTypeId = question.ResponseTypeId,
                                SubjectId = newSurveySubject.Id,
                                AnswerRequired = question.AnswerRequired
                            }).Where(newQuestion => !_context.Question.Any(c => c == newQuestion)))
                            {
                                await _context.Question.AddAsync(newQuestion);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }

                TempData["FeedbackMessage"] = $"Survey re-opened successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                TempData["FeedbackMessage"] = $"Error : {e.InnerException?.Message} will reopening survey";
                var surveySubjectList1 = _context.SurveySubject.Include(c => c.Survey.Surveyors).ToList();
                ViewBag.surveySubjects = surveySubjectList1.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = true
                }).ToList();
                ViewBag.Participants = _context.Users.ToList().Select(c => new SelectListItem
                {
                    Value = c.Id,
                    Text = c.UserName,
                    Selected = survey?.SurveyParticipants.Any(v => v.ParticipantId == c.Id) == true
                });
                ViewBag.courses = _context.Courses.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code}:{c.Name}",
                    Selected = c.Id == survey.CourseId
                });
                ViewBag.departments = _context.Departments.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code}:{c.Name}",
                    Selected = c.Id == survey.DepartmentId
                });
                return View(survey);
            }
        }
    }
}