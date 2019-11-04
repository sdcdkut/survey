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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using MimeKit;

namespace Surveyapp.Controllers
{
    public class SurveySubjectsController : Controller
    {
        private readonly SurveyContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;
        private readonly IHostingEnvironment _IHostingEnvironment;

        public SurveySubjectsController(IHostingEnvironment IHostingEnvironment,SurveyContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
            _IHostingEnvironment = IHostingEnvironment;
        }

        // GET: SurveySubjects 
        [Authorize(Roles = "Surveyor")]
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.CategoryId = id;
            ViewBag.SurveyId = _context.SurveyCategory.SingleOrDefault(x => x.Id == id)?.SurveyId;
            var surveyContext = _context.SurveySubject.Include(s => s.Category).Include(x => x.ResponseTypes).Where(x => x.CategoryId == id);
            return View(await surveyContext.ToListAsync());
        }

        // GET: SurveySubjects/Details/5
        [NoDirectAccess]
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveySubject = await _context.SurveySubject
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (surveySubject == null)
            {
                return NotFound();
            }

            ViewBag.SurveyId = surveySubject.Category.SurveyId;
            return View(surveySubject);
        }

        // GET: SurveySubjects/Create
        [NoDirectAccess]
        [Authorize]
        public IActionResult Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.SurveyId = _context.SurveyCategory.SingleOrDefault(x => x.Id == id)?.SurveyId;
            ViewData["CategoryId"] = id;/*new SelectList(_context.SurveyCategory, "Id", "Id");*/
            return View();
        }

        // POST: SurveySubjects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [NoDirectAccess]
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubjectName,CategoryId")] SurveySubject surveySubject, string[] otherproperty)
        {
            if (ModelState.IsValid)
            {
                var propertyDictonary = new Dictionary<string, string>();
                if (otherproperty.Length>0)
                {
                    foreach (var property in otherproperty)
                    {
                        string[] ids = property.Split(new char[] { '|' });
                        string propertyName = ids[0];
                        string propertyValue = ids[1];
                        propertyDictonary.Add(propertyName, propertyValue);
                    }
                }
                surveySubject.OtherProperties = propertyDictonary;
                _context.Add(surveySubject);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"{surveySubject.SubjectName} subject created successfully";
                return RedirectToAction(nameof(Create),"ResponseTypes", new { id = surveySubject.Id });
            }
            ViewData["CategoryId"] = new SelectList(_context.SurveyCategory, "Id", "Id", surveySubject.CategoryId);
            return View(surveySubject);
        }

        // GET: SurveySubjects/Edit/5
        [NoDirectAccess]
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveySubject = await _context.SurveySubject.Include(x=>x.Category).SingleOrDefaultAsync(x=>x.Id==id);
            if (surveySubject == null)
            {
                return NotFound();
            }
            ViewBag.SurveyId = _context.SurveyCategory.SingleOrDefault(x => x.Id == id)?.SurveyId;
            ViewData["CategoryId"] = surveySubject.CategoryId/*new SelectList(_context.SurveyCategory, "Id", "Id", surveySubject.CategoryId)*/;
            return View(surveySubject);
        }

        // POST: SurveySubjects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [NoDirectAccess]
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SubjectName,CategoryId")] SurveySubject surveySubject, string[] otherproperty)
        {
            if (id != surveySubject.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var propertyDictonary = new Dictionary<string, string>();
                    if (otherproperty.Length>0)
                    {
                        foreach (var property in otherproperty)
                        {
                            string[] ids = property.Split(new char[] { '|' });
                            string propertyName = ids[0];
                            string propertyValue = ids[1];
                            propertyDictonary.Add(propertyName, propertyValue);
                        }
                    }
                    surveySubject.OtherProperties = propertyDictonary;
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
                return RedirectToAction(nameof(Index), new { id = surveySubject.CategoryId });
            }
            ViewData["CategoryId"] = new SelectList(_context.SurveyCategory, "Id", "Id", surveySubject.CategoryId);
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

            return View(surveySubject);
        }

        // POST: SurveySubjects/Delete/5
        [NoDirectAccess]
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var surveySubject = await _context.SurveySubject.FindAsync(id);
            _context.SurveySubject.Remove(surveySubject);
            await _context.SaveChangesAsync();
            TempData["FeedbackMessage"] = $"survey subject deleted successfully";
            return RedirectToAction(nameof(Index), new { id = surveySubject.CategoryId });
        }

        private bool SurveySubjectExists(int id)
        {
            return _context.SurveySubject.Any(e => e.Id == id);
        }

        [NoDirectAccess]
        public IActionResult SurveySubjects(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveyStatus = _context.Survey.SingleOrDefault(x=>x.SurveyCategorys.Any(c=>c.Id==id))?.status;
            var subjects = _context.SurveySubject.Where(x => x.CategoryId == id).Include(x=>x.Category.Survey)
                .Where(x => x.Questions.Any()).Where(x => EF.Functions.DateDiffDay(x.Category.Survey.Startdate, DateTime.Now) > 0 && EF.Functions.DateDiffDay(DateTime.Now, x.Category.Survey.EndDate) > 0);
            /*if (surveyStatus == "Open")
            {
                subjects = subjects;
            }

            if (surveyStatus == "Closed")
            {
                
            }*/
            //throw new NotImplementedException();
            return View(subjects);
        }

        public IActionResult Page_Load_file()
        {
            //create document
            Document document = new Document();
            try
            {
                //writer - have our own path!!! and see you have write permissions...
                string filefolder = Path.Combine(_IHostingEnvironment.WebRootPath, "Files");
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
                    document.Add((IElement) htmlarraylist[k]);
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


    }
}
