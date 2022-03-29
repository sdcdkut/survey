using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Surveyapp.Models;

namespace Surveyapp.Controllers
{
    public class SchoolOrInstitutionsController : Controller
    {
        private readonly SurveyContext _context;

        public SchoolOrInstitutionsController(SurveyContext context)
        {
            _context = context;
        }

        // GET: SchoolOrInstitutions
        public async Task<IActionResult> Index()
        {
            var surveyContext = _context.SchoolOrInstitutions.Include(s => s.Campus);
            return View(await surveyContext.ToListAsync());
        }

        // GET: SchoolOrInstitutions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolOrInstitution = await _context.SchoolOrInstitutions
                .Include(s => s.Campus)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schoolOrInstitution == null)
            {
                return NotFound();
            }

            return View(schoolOrInstitution);
        }

        // GET: SchoolOrInstitutions/Create
        public IActionResult Create()
        {
            ViewData["CampusId"] = new SelectList(_context.Campus, "Id", "Id");
            return View();
        }

        // POST: SchoolOrInstitutions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,Name,CampusId")] SchoolOrInstitution schoolOrInstitution)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schoolOrInstitution);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CampusId"] = new SelectList(_context.Campus, "Id", "Id", schoolOrInstitution.CampusId);
            return View(schoolOrInstitution);
        }

        // GET: SchoolOrInstitutions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolOrInstitution = await _context.SchoolOrInstitutions.FindAsync(id);
            if (schoolOrInstitution == null)
            {
                return NotFound();
            }
            ViewData["CampusId"] = new SelectList(_context.Campus, "Id", "Id", schoolOrInstitution.CampusId);
            return View(schoolOrInstitution);
        }

        // POST: SchoolOrInstitutions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,CampusId")] SchoolOrInstitution schoolOrInstitution)
        {
            if (id != schoolOrInstitution.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schoolOrInstitution);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SchoolOrInstitutionExists(schoolOrInstitution.Id))
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
            ViewData["CampusId"] = new SelectList(_context.Campus, "Id", "Id", schoolOrInstitution.CampusId);
            return View(schoolOrInstitution);
        }

        // GET: SchoolOrInstitutions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schoolOrInstitution = await _context.SchoolOrInstitutions
                .Include(s => s.Campus)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schoolOrInstitution == null)
            {
                return NotFound();
            }

            return View(schoolOrInstitution);
        }

        // POST: SchoolOrInstitutions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schoolOrInstitution = await _context.SchoolOrInstitutions.FindAsync(id);
            _context.SchoolOrInstitutions.Remove(schoolOrInstitution);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SchoolOrInstitutionExists(int id)
        {
            return _context.SchoolOrInstitutions.Any(e => e.Id == id);
        }
    }
}
