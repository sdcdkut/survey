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
    public class ResponseTypesController : Controller
    {
        private readonly SurveyContext _context;

        public ResponseTypesController(SurveyContext context)
        {
            _context = context;
        }

        // GET: ResponseTypes
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.SubjectId = id;
            var surveyContext = _context.ResponseType.Include(r => r.Subject).Where(x=>x.SubjectId == id);
            /*var responses =*/
            return View(await surveyContext.ToListAsync());
        }

        // GET: ResponseTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var responseType = await _context.ResponseType
                .Include(r => r.Subject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (responseType == null)
            {
                return NotFound();
            }

            return View(responseType);
        }

        // GET: ResponseTypes/Create
        public IActionResult Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.SubjectId = id;
            //ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id");
            return View();
        }

        // POST: ResponseTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ResponseName,SubjectId")] ResponseType responseType,int SubjectId,string ResponseName,string[] responseDictionary)
        {
            /*if (ModelState.IsValid)
            {*/
            var respDictonary = new Dictionary<string, string>();
            if (responseDictionary.Length>0)
            {
                for (int i = 1; i < responseDictionary.Length; i++)
                {
                    respDictonary.Add(i.ToString(), responseDictionary[i]);
                }
                /*var dictionary = responseDictionary.ToDictionary(item => item.Key,
                    item => item.Value);*/
                ResponseType responseTypes = new ResponseType()
                {
                    SubjectId = SubjectId,
                    ResponseName = ResponseName,
                    ResponseDictionary = respDictonary
                };
                
                    _context.Add(responseTypes);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index),new {id=responseTypes.SubjectId});
            }
            /*else
            {
                return View(responseType);
            }*/
            
            /*}*/
            ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id", responseType.SubjectId);
            return View(responseType);
        }

        // GET: ResponseTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var responseType = await _context.ResponseType.FindAsync(id);
            if (responseType == null)
            {
                return NotFound();
            }
            ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id", responseType.SubjectId);
            return View(responseType);
        }

        // POST: ResponseTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ResponseName")] ResponseType responseType)
        {
            if (id != responseType.Id)
            {
                return NotFound();
            }
            //ModelState.Remove("ResponseDictionary");
            if (/*ModelState.IsValid*/responseType.Id != null && !string.IsNullOrEmpty(responseType.ResponseName))
            {
                var resposUpdate = _context.ResponseType.SingleOrDefault(x=>x.Id == responseType.Id);
                try
                {
                    resposUpdate.ResponseName=responseType.ResponseName;
                    _context.Update(resposUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResponseTypeExists(responseType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index),new{id=resposUpdate.SubjectId});
            }
            ViewData["SubjectId"] = new SelectList(_context.SurveySubject, "Id", "Id", responseType.SubjectId);
            return View(responseType);
        }

        // GET: ResponseTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var responseType = await _context.ResponseType
                .Include(r => r.Subject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (responseType == null)
            {
                return NotFound();
            }

            return View(responseType);
        }

        // POST: ResponseTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var responseType = await _context.ResponseType.FindAsync(id);
            _context.ResponseType.Remove(responseType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index),new{id=responseType.SubjectId});
        }

        private bool ResponseTypeExists(int id)
        {
            return _context.ResponseType.Any(e => e.Id == id);
        }
    }
}
