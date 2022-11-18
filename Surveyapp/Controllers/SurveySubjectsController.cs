using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Surveyapp.Models;
using Surveyapp.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using Newtonsoft.Json;

namespace Surveyapp.Controllers
{
    public class SurveySubjectsController : Controller
    {
        private readonly SurveyContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IWebHostEnvironment _iHostingEnvironment;

        public SurveySubjectsController(IWebHostEnvironment hostingEnvironment, SurveyContext context, UserManager<ApplicationUser> userManager
            , IHttpClientFactory httpClientFactory, IBackgroundTaskQueue queue, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _usermanager = userManager;
            _httpClientFactory = httpClientFactory;
            _queue = queue;
            _serviceScopeFactory = serviceScopeFactory;
            _iHostingEnvironment = hostingEnvironment;
        }

        // GET: SurveySubjects 
        [Authorize( /*Roles = "Surveyor"*/)]
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.CategoryId = id;
            var surveyId = id;
            ViewBag.SurveyId = surveyId;
            var surveyContext = _context.SurveySubject.Include(s => s.Category)
                .Include(c => c.Survey).Where(x => x.SurveyId == id)
                .Where(c => !c.DynamicallyCreated);
            var useId = _usermanager.GetUserId(User);
            var survey = _context.Survey.Include(c => c.Surveyors).FirstOrDefault(c => c.Id == surveyId);
            var httpClient = _httpClientFactory.CreateClient("Workman");
            var httpResponseMessage = await httpClient.GetAsync("api/Units/Courses");
            var courses = await httpResponseMessage.Content.ReadFromJsonAsync<List<Course>>();
            if (survey?.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId) == false)
            {
                return StatusCode(403);
            }

            return View(await surveyContext.ToListAsync());
        }

        // GET: SurveySubjects/Details/5
        //[NoDirectAccess]
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            var surveySubject = await _context.SurveySubject
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (surveySubject == null)
            {
                return NotFound();
            }

            var surveyId = surveySubject.SurveyId;
            var useId = _usermanager.GetUserId(User);
            var survey = _context.Survey.Include(c => c.Surveyors).FirstOrDefault(c => c.Id == surveyId);
            if (survey?.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId) == false)
            {
                return StatusCode(403);
            }

            ViewBag.SurveyId = surveyId;
            return View(surveySubject);
        }

        // GET: SurveySubjects/Create
        //[NoDirectAccess]
        [Authorize]
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.SurveyId = id;
            var useId = _usermanager.GetUserId(User);
            var survey = _context.Survey.Include(c => c.Surveyors).FirstOrDefault(c => c.Id == id);
            if (survey?.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId) == false)
            {
                return StatusCode(403);
            }

            ViewData["CategoryId"] = new SelectList(_context.SurveyCategory.Where(c => c.SurveyId == id), "Id", "CategoryName");
            ViewBag.Description = survey?.Description;
            ViewBag.ResponseTypeId = _context.ResponseType.Select(v => new SelectListItem
            {
                Text = $"{v.ResponseName} ({v.DisplayOptionType})",
                Value = v.Id.ToString()
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
            return View();
        }

        // POST: SurveySubjects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[NoDirectAccess]
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,ResponseTypeId,Description,SurveyId,OtherProperties,CategoryId,AddAnotherSubjectOnSurveyTake,DynamicSubjectValue,CourseId,DepartmentId")]
            SurveySubject surveySubject,
            Dictionary<int, OtherProperties> OtherProperties,
            Dictionary<int, DynamicSubjectValueViewModel> DynamicSubjectValue)
        {
            surveySubject.OtherProperties = OtherProperties?.Select(c => new OtherProperties { Name = c.Value.Name, Value = c.Value.Value }).ToList();
            if (surveySubject.AddAnotherSubjectOnSurveyTake && DynamicSubjectValue.Any())
            {
                surveySubject.DynamicSubjectValue = DynamicSubjectValue.Select(c => new DynamicSubjectValue
                {
                    Name = c.Value.Name,
                    SelectValueOptions = c.Value.SelectValueOptions.Select(v => v.Value).ToList()
                }).ToList();
            }

            if (ModelState.IsValid)
            {
                _context.Add(surveySubject);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"{surveySubject.Name} subject created successfully";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return RedirectToAction("_CreatePartial", "ResponseTypes", new { id = surveySubject.Id });
                }

                return RedirectToAction(nameof(Create), "Questions", new { id = surveySubject.Id });
            }

            ViewData["CategoryId"] = new SelectList(_context.SurveyCategory, "Id", "CategoryName", surveySubject.CategoryId);
            ViewBag.ResponseTypeId = _context.ResponseType.Select(v => new SelectListItem
            {
                Text = $"{v.ResponseName} ({v.DisplayOptionType})",
                Value = v.Id.ToString(),
                Selected = v.Id == surveySubject.ResponseTypeId
            });
            ViewBag.courses = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == surveySubject.CourseId
            });
            ViewBag.departments = _context.Departments.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == surveySubject.DepartmentId
            });
            return View(surveySubject);
        }

        // GET: SurveySubjects/Edit/5
        //[NoDirectAccess]
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveySubject = await _context.SurveySubject.Include(x => x.Category).SingleOrDefaultAsync(x => x.Id == id);
            if (surveySubject == null)
            {
                return NotFound();
            }

            var surveyId = surveySubject.SurveyId;
            var useId = _usermanager.GetUserId(User);
            var survey = _context.Survey.Include(c => c.Surveyors).FirstOrDefault(c => c.Id == surveyId);
            if (survey?.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId) == false)
            {
                return StatusCode(403);
            }

            ViewBag.SurveyId = surveyId;
            ViewData["CategoryId"] = new SelectList(_context.SurveyCategory, "Id", "CategoryName", surveySubject.CategoryId);
            ViewBag.ResponseTypeId = _context.ResponseType.Select(v => new SelectListItem
            {
                Text = $"{v.ResponseName} ({v.DisplayOptionType})",
                Value = v.Id.ToString(),
                Selected = v.Id == surveySubject.ResponseTypeId
            });
            ViewBag.courses = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == surveySubject.CourseId
            });
            ViewBag.departments = _context.Departments.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == surveySubject.DepartmentId
            });
            return View(surveySubject);
        }

        // POST: SurveySubjects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[NoDirectAccess]
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Name,ResponseTypeId,Description,SurveyId,OtherProperties,CategoryId,AddAnotherSubjectOnSurveyTake,DynamicSubjectValue,CourseId,DepartmentId")]
            SurveySubject surveySubject,
            Dictionary<int, OtherProperties> OtherProperties,
            Dictionary<int, DynamicSubjectValueViewModel> DynamicSubjectValue)
        {
            if (id != surveySubject.Id)
            {
                return NotFound();
            }

            surveySubject.OtherProperties = OtherProperties?.Select(c => new OtherProperties { Name = c.Value.Name, Value = c.Value.Value }).ToList();
            surveySubject.DynamicSubjectValue = null;
            if (surveySubject.AddAnotherSubjectOnSurveyTake)
            {
                surveySubject.DynamicSubjectValue = DynamicSubjectValue?.Select(c => new DynamicSubjectValue
                {
                    Name = c.Value.Name,
                    SelectValueOptions = c.Value.SelectValueOptions.Select(v => v.Value).ToList()
                }).ToList();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(surveySubject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SurveySubjectExists(surveySubject.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["FeedbackMessage"] = $"survey subject edited successfully";
                return RedirectToAction(nameof(Index), new { id = surveySubject.SurveyId });
            }

            ViewData["CategoryId"] = new SelectList(_context.SurveyCategory, "Id", "CategoryName", surveySubject.CategoryId);
            ViewBag.ResponseTypeId = _context.ResponseType.Select(v => new SelectListItem
            {
                Text = $"{v.ResponseName} ({v.DisplayOptionType})",
                Value = v.Id.ToString(),
                Selected = v.Id == surveySubject.ResponseTypeId
            });
            ViewBag.courses = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == surveySubject.CourseId
            });
            ViewBag.departments = _context.Departments.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code}:{c.Name}",
                Selected = c.Id == surveySubject.DepartmentId
            });
            return View(surveySubject);
        }

        // GET: SurveySubjects/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.SurveyId = _context.SurveyCategory.SingleOrDefault(x => x.Id == id)?.SurveyId;
            var surveySubject = await _context.SurveySubject
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (surveySubject == null)
            {
                return NotFound();
            }

            var surveyId = surveySubject.SurveyId;
            var useId = _usermanager.GetUserId(User);
            var survey = _context.Survey.Include(c => c.Surveyors).FirstOrDefault(c => c.Id == surveyId);
            if (survey?.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId) == false)
            {
                return StatusCode(403);
            }

            return View(surveySubject);
        }

        // POST: SurveySubjects/Delete/5
        //[NoDirectAccess]
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var surveySubject = await _context.SurveySubject.FindAsync(id);
            _context.SurveySubject.Remove(surveySubject);
            await _context.SaveChangesAsync();
            TempData["FeedbackMessage"] = $"survey subject deleted successfully";
            return RedirectToAction(nameof(Index), new { id = surveySubject.SurveyId });
        }

        private bool SurveySubjectExists(int id)
        {
            return _context.SurveySubject.Any(e => e.Id == id);
        }

        //[NoDirectAccess]
        public async Task<IActionResult> SurveySubjects(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var survey = _context.Survey.Include(c => c.Surveyors).SingleOrDefault(x => x.SurveyCategorys.Any(c => c.Id == id));
            var surveyStatus = survey?.status;
            var subjects = _context.SurveySubject.Include(x => x.Category.Survey)
                .Include(x => x.Survey.Surveyors).Include(c => c.Questions)
                .Where(x => x.Questions.Any()).Where(x => (DateTime.Now - x.Survey.Startdate).Days >= 0 && (x.Survey.EndDate - DateTime.Now).Days >= 0);
            var surveyorIds = _context.Survey.Include(c => c.Surveyors).SingleOrDefault(x => x.Id == id)?.Surveyors;
            var useId = _usermanager.GetUserId(User);

            /*if (surveyStatus == "Closed")
            {
                
                if (!survey.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId))
                {
                    return StatusCode(403);
                }
            }*/
            //throw new NotImplementedException();
            return View(subjects);
        }

        public IActionResult _CreatePartial(int id)
        {
            var surveySubject = _context.SurveySubject.Where(x => x.CategoryId == id);
            ViewBag.CategoryId = id;
            var surveyId = _context.Survey.SingleOrDefault(x => x.SurveyCategorys.Any(c => c.Id == id))?.Id;
            var useId = _usermanager.GetUserId(User);
            var survey = _context.Survey.Include(c => c.Surveyors).FirstOrDefault(c => c.Id == surveyId);
            if (survey?.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == useId) == false)
            {
                return StatusCode(403);
            }

            return PartialView(new SurveySubject());
        }

        public async Task<IActionResult> UpdateSubjects(int subjectId)
        {
            int? surveyId = null;
            _queue.QueueBackgroundWorkItem(async token =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var httpClient = _httpClientFactory.CreateClient("Workman");
                var surveyContext = scopedServices.GetRequiredService<SurveyContext>();
                var httpResponseMessage = await httpClient.GetAsync(
                    "api/Units/lecturerunits", token);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var lecturerUnits = await httpResponseMessage.Content.ReadFromJsonAsync<List<LecturerUnits>>(cancellationToken: token);
                    var subject = surveyContext.SurveySubject.FirstOrDefault(c => c.Id == subjectId);
                    var dynamicSubjectValues = subject?.DynamicSubjectValue;
                    surveyId = subject?.SurveyId;
                    var groups = lecturerUnits?.SelectMany(c => c.semesterGroups.Select(v => new
                    {
                        course = v.programme,
                        v.semester,
                        v.school,
                        v.academicYear,
                        c.unit,
                        lecturers = c.lectures
                    }).SelectMany(d => d.lecturers.Select(v => new
                    {
                        d.course,
                        d.school,
                        d.semester,
                        d.academicYear,
                        c.unit,
                        v.lecturer
                    }))).ToList();
                    var newDynamicValues = new List<DynamicSubjectValue>();
                    if (groups != null)
                    {
                        newDynamicValues.Add(new DynamicSubjectValue
                        {
                            Name = "Course",
                            SelectValueOptions = groups.GroupBy(c => c.course).Select(v => v.Key).Distinct().ToList()
                        });
                        newDynamicValues.Add(new DynamicSubjectValue
                        {
                            Name = "School",
                            SelectValueOptions = groups.GroupBy(c => c.school).Select(v => v.Key).Distinct().ToList()
                        });
                        newDynamicValues.Add(new DynamicSubjectValue
                        {
                            Name = "Semester",
                            SelectValueOptions = groups.GroupBy(c => c.semester).Select(v => v.Key).Distinct().ToList()
                        });
                        newDynamicValues.Add(new DynamicSubjectValue
                        {
                            Name = "AcademicYear",
                            SelectValueOptions = groups.GroupBy(c => c.academicYear).Select(v => v.Key).Distinct().ToList()
                        });
                        newDynamicValues.Add(new DynamicSubjectValue
                        {
                            Name = "Lecturer",
                            SelectValueOptions = groups.GroupBy(c => c.lecturer).Select(v => v.Key).Distinct().ToList()
                        });
                        newDynamicValues.Add(new DynamicSubjectValue
                        {
                            Name = "Unit",
                            SelectValueOptions = groups.GroupBy(c => c.unit).Select(v => v.Key).Distinct().ToList()
                        });

                        if (subject != null) subject.DynamicSubjectValue = newDynamicValues;
                        await surveyContext.SaveChangesAsync(token);
                    }
                }
                else
                {
                    Console.WriteLine("{0} ({1})",
                        (int)httpResponseMessage.StatusCode,
                        httpResponseMessage.ReasonPhrase);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), token);
            });
            TempData["FeedbackMessage"] = $"Update subject In progress..";
            return RedirectToAction(nameof(Index), new { id = surveyId });
        }

        public IActionResult Page_Load_file()
        {
            //create document
            Document document = new Document();
            try
            {
                //writer - have our own path!!! and see you have write permissions...
                string filefolder = Path.Combine(_iHostingEnvironment.WebRootPath, "Files");
                PdfWriter.GetInstance(document,
                    new FileStream(Path.Combine(filefolder, "parsetest.pdf"), FileMode.Create));
                document.Open();
                //html -text - kan be from database or editor too
                String htmlText = $"<font " +
                                  " color=\"#0000FF\"><b><i>Title One</i></b></font><font " +
                                  " color=\"black\"><br><br>Some text here<br><br><br><font  " +
                                  " color=\"#0000FF\"><b><i>Another title here " +
                                  " </i></b></font><font  " +
                                  " color=\"black\"><br><br>Text1<br>Text2<br><OL><LI>hi</LI><LI>how are u</LI></OL>";

                //make an arraylist ....with STRINGREADER since its no IO reading file...
                ArrayList htmlarraylist =
                    iTextSharp.text.html.simpleparser.HtmlWorker.ParseToList(new StringReader(htmlText), null);
                //add the collection to the document
                for (int k = 0; k < htmlarraylist.Count; k++)
                {
                    document.Add((IElement)htmlarraylist[k]);
                }

                document.Add(new Paragraph("And the same with indentation...."));

                // or add the collection to an paragraph
                // if you add it to an existing non emtpy paragraph it will insert it from
                //the point youwrite -
                Paragraph mypara = new Paragraph(); //make an emtphy paragraph as "holder"
                mypara.IndentationLeft = 36;
                mypara.InsertRange(0, htmlarraylist);
                document.Add(mypara);
                document.Close();
                string filepath = Path.Combine(filefolder, "parsetest.pdf");
                return PhysicalFile(filepath, MimeTypes.GetMimeType("parsetest.pdf"), Path.GetFileName(filepath));
            }
            catch (Exception exx)
            {
                Console.Error.WriteLine(exx.StackTrace);
                Console.Error.WriteLine(exx.Message);
            }

            return Content("some staff");
        }

        public async Task<IActionResult> UpdateSubjectsFromWorkload(int id)
        {
            ViewBag.SurveySubjectId = id;
            var httpClient = _httpClientFactory.CreateClient("Workman");
            //var query = new Dictionary<string, string>{ {"active", "true"} };
            var httpResponseMessage = await httpClient.GetAsync("api/Units/AcademicYears?active=true");
            var httpResponseMessageCourses = await httpClient.GetAsync("api/Units/Courses");
            if (httpResponseMessageCourses.IsSuccessStatusCode)
            {
                var coursesString = await httpResponseMessageCourses.Content.ReadAsStringAsync();
                var courses = JsonConvert.DeserializeObject<List<Course>>(coursesString);
                ViewBag.Courses = courses?.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code}-{c.Name ?? c.couserName}"
                });
            }

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var academicYearsString = await httpResponseMessage.Content.ReadAsStringAsync();
                var academicYears = JsonConvert.DeserializeObject<List<AcademicYear>>(academicYearsString);
                ViewBag.academicYears = academicYears?.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                });
            }

            return View();
        }

        public async Task<IActionResult> GetYearSemesters(int? academicYearId, string courseIds)
        {
            var httpClient = _httpClientFactory.CreateClient("Workman");
            var courseIdsArray = courseIds?.Split(',');
            var courseIdsQuery = string.Join("", courseIdsArray?.Select(c => $"&courseIds={c}") ?? new List<string>());
            //var courseIdsQuery = courseIdsArray?.Length>0? $"&courseIds={string.Join(",", courseIds)}": "";
            var httpResponseMessage = await httpClient.GetAsync($"api/Units/CourseSemesters?academicYearId={academicYearId}{courseIdsQuery}");
            if (!httpResponseMessage.IsSuccessStatusCode) return Json(false);
            var semesters = await httpResponseMessage.Content.ReadFromJsonAsync<List<Semester>>();
            return Json(semesters?.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Startdate:MMMM}({c.Startdate:MM/dd/yyyy}) - {c.EndDate:MMMM}({c.EndDate:MM/dd/yyyy}" }).Distinct());
        }

        public async Task<IActionResult> UpdateSubjectValueOptions(int surveySubjectId, Dictionary<int, DynamicSubjectValue> selectValueOptions)
        {
            var surveySubjectValue = await _context.SurveySubject.FindAsync(surveySubjectId);
            if (surveySubjectValue != null)
            {
                surveySubjectValue.DynamicSubjectValue = selectValueOptions.Select(c => new DynamicSubjectValue
                {
                    Name = c.Value?.Name,
                    SelectValueOptions = c.Value?.SelectValueOptions
                }).ToList();
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = "Update subject value options successfully";
                return RedirectToAction(nameof(Index), new { id = surveySubjectValue.SurveyId });
            }

            TempData["FeedbackMessage"] = "Update subject value options failed";
            return RedirectToAction(nameof(UpdateSubjectsFromWorkload), new { id = surveySubjectId });
        }

        public async Task<IActionResult> GetLecturerUnits(int? academicYearId, string courseIds, string semesterIds, int? surveySubjectId)
        {
            var httpClient = _httpClientFactory.CreateClient("Workman");
            var semesterIdsArray = semesterIds?.Split(',');
            var semesterIdsQuery = string.Join("", semesterIdsArray?.Select(c => $"&semesterId={c}") ?? new List<string>());
            var courseIdsArray = courseIds?.Split(',');
            var courseIdsQuery = string.Join("", courseIdsArray?.Select(c => $"&courseIds={c}") ?? new List<string>());
            var httpResponseMessage = await httpClient.GetAsync($"api/Units/LecturerUnits?academicYearId={academicYearId}{semesterIdsQuery}{courseIdsQuery}");
            if (!httpResponseMessage.IsSuccessStatusCode) return Json(false);
            var lecUnits = await httpResponseMessage.Content.ReadFromJsonAsync<List<LecturerUnits>>();
            var surveySubject = await _context.SurveySubject.FindAsync(surveySubjectId);
            var surveySubjectDynamicSubjectValues = surveySubject?.DynamicSubjectValue? /*.Select(c => c.SelectValueOptions)*/.ToList();
            //var surveySubjectValuesList = surveySubjectValues?.SelectMany(c => c).ToList();
            //var surveySubjectValuesListDistinct = surveySubjectValuesList?.Distinct().ToList();
            var groups = lecUnits?.SelectMany(c => c.semesterGroups.Select(v => new
            {
                course = v.programme,
                v.semester,
                v.school,
                v.academicYear,
                c.unit,
                lecturers = c.lectures
            }).SelectMany(d => d.lecturers.Select(v => new
            {
                d.course,
                //d.school,
                d.semester,
                /*d.academicYear,
                c.unit,*/
                v.lecturer
            }))).Distinct().ToList();
            var semList = new List<string> { "jan-april", "may-aug", "sep-dec" };
            var yearList = new List<string> { "first year", "second year", "third year", "fourth year", "fifth year", "sixth year", "seventh year" };
            var courses = groups?.Select(c => c.course).Distinct().ToList();
            var lecturers = groups?.Select(c => c.lecturer).Distinct().ToList();
            var newDynamicValues = new List<DynamicSubjectValue>();
            if (surveySubjectDynamicSubjectValues != null)
            {
                foreach (var surveySubjectDynamicSubjectValue in surveySubjectDynamicSubjectValues)
                {
                    if (surveySubjectDynamicSubjectValue.Name.Equals("Course", StringComparison.CurrentCultureIgnoreCase))
                    {
                        groups?.Select(c => c.course).Distinct().ToList().ForEach(c =>
                        {
                            if (surveySubjectDynamicSubjectValue.SelectValueOptions.All(v => v.Equals(c, StringComparison.CurrentCultureIgnoreCase))) return;
                            surveySubjectDynamicSubjectValue.SelectValueOptions.Add(c);
                        });
                        //surveySubjectDynamicSubjectValue.SelectValueOptions = coursesList;
                    }

                    if (surveySubjectDynamicSubjectValue.Name.Equals("Lecturer", StringComparison.CurrentCultureIgnoreCase))
                    {
                        groups?.Select(c => c.lecturer).Distinct().ToList().ForEach(c =>
                        {
                            if (surveySubjectDynamicSubjectValue.SelectValueOptions.All(v => v.Equals(c, StringComparison.CurrentCultureIgnoreCase))) return;
                            surveySubjectDynamicSubjectValue.SelectValueOptions.Add(c);
                        });
                        /*var lecturersList = groups?.Select(c => c.lecturer).Distinct().Select(c =>
                            surveySubjectDynamicSubjectValue.SelectValueOptions?.Contains(c) == false && !string.IsNullOrEmpty(c) ? c : string.Empty).ToList();
                        surveySubjectDynamicSubjectValue.SelectValueOptions = lecturersList;*/
                    }

                    if (surveySubjectDynamicSubjectValue.Name.Equals("semester", StringComparison.CurrentCultureIgnoreCase))
                    {
                        surveySubjectDynamicSubjectValue.SelectValueOptions = semList;
                    }

                    if (surveySubjectDynamicSubjectValue.Name.Equals("year of study", StringComparison.CurrentCultureIgnoreCase))
                    {
                        surveySubjectDynamicSubjectValue.SelectValueOptions = yearList;
                    }

                    newDynamicValues.Add(surveySubjectDynamicSubjectValue);
                }

                if (!surveySubjectDynamicSubjectValues.Any(c => c.Name.Equals("Course", StringComparison.CurrentCultureIgnoreCase)))
                    newDynamicValues.Add(new DynamicSubjectValue
                    {
                        Name = "Course",
                        SelectValueOptions = courses
                    });
                if (!surveySubjectDynamicSubjectValues.Any(c => c.Name.Equals("Lecturer", StringComparison.CurrentCultureIgnoreCase)))
                    newDynamicValues.Add(new DynamicSubjectValue
                    {
                        Name = "Lecturer",
                        SelectValueOptions = lecturers
                    });
                if (!surveySubjectDynamicSubjectValues.Any(c => c.Name.Equals("semester", StringComparison.CurrentCultureIgnoreCase)))
                    newDynamicValues.Add(new DynamicSubjectValue
                    {
                        Name = "semester",
                        SelectValueOptions = semList
                    });
                if (!surveySubjectDynamicSubjectValues.Any(c => c.Name.Equals("year of study", StringComparison.CurrentCultureIgnoreCase)))
                    newDynamicValues.Add(new DynamicSubjectValue
                    {
                        Name = "year of study",
                        SelectValueOptions = yearList
                    });
            }
            else
            {
                newDynamicValues.Add(new DynamicSubjectValue
                {
                    Name = "Course",
                    SelectValueOptions = courses
                });
                newDynamicValues.Add(new DynamicSubjectValue
                {
                    Name = "Lecturer",
                    SelectValueOptions = lecturers
                });
                newDynamicValues.Add(new DynamicSubjectValue
                {
                    Name = "semester",
                    SelectValueOptions = semList
                });
                newDynamicValues.Add(new DynamicSubjectValue
                {
                    Name = "year of study",
                    SelectValueOptions = yearList
                });
            }

            return Json(newDynamicValues);
        }

        public class DynamicSubjectValueViewModel
        {
            public string Name { get; set; }
            public Dictionary<int, string> SelectValueOptions { get; set; }
        }

        public class LecturerUnits
        {
            public string unit { get; set; }
            public List<Lecturers> lectures { get; set; }
            public List<SemesterGroups> semesterGroups { get; set; }
        }

        public class SemesterGroups
        {
            public string semester { get; set; }
            public int semesterId { get; set; }
            public int academicYearId { get; set; }
            public string academicYear { get; set; }
            public string programme { get; set; }
            public int programmeId { get; set; }
            public string department { get; set; }
            public int departmentId { get; set; }
            public string school { get; set; }
            public int schoolId { get; set; }
        }

        public class Lecturers
        {
            public string lecturer { get; set; }
            public Guid lecturerId { get; set; }
        }

        public class AcademicYear
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int? DepartmentId { get; set; }

            public DateTime? StartDate { get; set; }

            public DateTime? EndDate { get; set; }

            //public PlanningDates PlanningDates { get; set; } = new PlanningDates();

            public string OpenStatus { get; set; }
        }

        public enum AcademicyearStatus
        {
            Pending,
            Open,
            Closed,
            [Display(Name = "Planning Stage")] PlaningStage
        }
    }

    public class Semester
    {
        public int Id { get; set; }
        public string SemesterTerm { get; set; }
        public string Name { get; set; }
        public int AcademicyearId { get; set; }
        public string OpenStatus { get; set; }
        public DateTime? Startdate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; } = DateTime.UtcNow;
        public string ReferenceName => $"{SemesterTerm} ({Startdate:MMMM}({Startdate:MM/dd/yyyy}) - {EndDate:MMMM}({EndDate:MM/dd/yyyy}) )";
    }
}