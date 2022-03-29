using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Surveyapp.Models;

namespace Surveyapp.Controllers
{
    [Authorize( /*Roles = "Surveyor"*/)]
    public class SurveyCategoriesController : Controller
    {
        private readonly SurveyContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;

        public SurveyCategoriesController(SurveyContext context, UserManager<ApplicationUser> usermanager)
        {
            _context = context;
            _usermanager = usermanager;
        }

        // GET: SurveyCategories
        //[NoDirectAccess]
        [Authorize]
        public async Task<IActionResult> Index(int id)
        {
            var user = await _usermanager.GetUserAsync(User);
            ViewBag.SurveyId = id;
            var surveyContext = _context.SurveyCategory.Include(s => s.Survey.Surveyors)
                .Where(x => x.SurveyId == id && x.Survey.Surveyors.Any(v => v.ActiveStatus && v.SurveyorId == user.Id));
            return View(await surveyContext.ToListAsync());
        }

        // GET: SurveyCategories/Details/5
        //[NoDirectAccess]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveyCategory = await _context.SurveyCategory
                .Include(s => s.Survey)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (surveyCategory == null)
            {
                return NotFound();
            }

            return View(surveyCategory);
        }

        // GET: SurveyCategories/Create
        //[NoDirectAccess]
        public IActionResult Create(int id)
        {
            var userId = _usermanager.GetUserId(User);
            ViewBag.SurveyId = id;
            //ViewData["SurveyId"] = new SelectList(_context.Survey.Include(c => c.Surveyors).Where(c => c.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == userId)), "Id", "name");
            return View();
        }

        // POST: SurveyCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[NoDirectAccess]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName,SurveyId")] SurveyCategory surveyCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(surveyCategory);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = $"survey category added successfully";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return RedirectToAction("_CreatePartial", "SurveySubjects", new { id = surveyCategory.SurveyId });
                }

                return RedirectToAction(nameof(Create), "SurveySubjects", new { id = surveyCategory.SurveyId });
            }

            var userId = _usermanager.GetUserId(User);
            ViewBag.SurveyId = surveyCategory.SurveyId;
            //ViewData["SurveyId"] = new SelectList(_context.Survey.Include(c => c.Surveyors).Where(c => c.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == userId)), "Id", "name",
            //    surveyCategory.SurveyId);
            return View(surveyCategory);
        }

        // GET: SurveyCategories/Edit/5
        //[NoDirectAccess]
        public async Task<IActionResult> Edit(int? id)
        {
            var surveyCategory = await _context.SurveyCategory.FindAsync(id);
            if (surveyCategory == null)
            {
                return NotFound();
            }

            var userId = _usermanager.GetUserId(User);
            ViewBag.SurveyId = surveyCategory.SurveyId;
            //ViewData["SurveyId"] = new SelectList(_context.Survey.Include(c => c.Surveyors).Where(c => c.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == userId)), "Id", "name",
            //    surveyCategory.SurveyId);
            return View(surveyCategory);
        }

        // POST: SurveyCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[NoDirectAccess]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryName,SurveyId")] SurveyCategory surveyCategory)
        {
            if (id != surveyCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(surveyCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SurveyCategoryExists(surveyCategory.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["FeedbackMessage"] = $"survey category edited successfully";
                return RedirectToAction(nameof(Index), new { id = surveyCategory.SurveyId });
            }

            var userId = _usermanager.GetUserId(User);
            ViewBag.SurveyId = surveyCategory.SurveyId;
            //ViewData["SurveyId"] = new SelectList(_context.Survey.Include(c => c.Surveyors).Where(c => c.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == userId)), "Id", "name",
            //    surveyCategory.SurveyId);
            return View(surveyCategory);
        }

        // GET: SurveyCategories/Delete/5
        //[NoDirectAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var surveyCategory = await _context.SurveyCategory
                .Include(s => s.Survey)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (surveyCategory == null)
            {
                return NotFound();
            }

            var survey = await _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor).FirstOrDefaultAsync(c => c.Id == id);
            if (!survey.Surveyors.Any(c => c.ActiveStatus && c.SurveyorId == _usermanager.GetUserId(User)))
            {
                return StatusCode(403);
            }
            ViewBag.SurveyId = surveyCategory.SurveyId;

            return View(surveyCategory);
        }

        // POST: SurveyCategories/Delete/5
        //[NoDirectAccess]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var surveyCategory = await _context.SurveyCategory.FindAsync(id);
            _context.SurveyCategory.Remove(surveyCategory);
            await _context.SaveChangesAsync();
            TempData["FeedbackMessage"] = $"survey category deleted successfully";
            return RedirectToAction(nameof(Index), new { id = surveyCategory.SurveyId });
        }

        private bool SurveyCategoryExists(int id)
        {
            return _context.SurveyCategory.Any(e => e.Id == id);
        }

        public IActionResult _CreateSurveyCategoryPartial(int id)
        {
            var survey = _context.Survey.Include(c => c.Surveyors).ThenInclude(c => c.Surveyor).SingleOrDefault(x => x.Id == id);
            var userId = _usermanager.GetUserId(User);
            var surveyCategory = _context.SurveyCategory.Where(x => x.SurveyId == id);
            //ViewData["SurveyId"] = new SelectList(_context.Survey.Where(c => c.SurveyerId == userId), "Id", "name", survey.Id);
            return PartialView(new SurveyCategory());
        }
    }
}