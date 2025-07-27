using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cineverse.Data;
using Cineverse.Models;
using Microsoft.AspNetCore.Authorization;

namespace Cineverse.Controllers
{
    public class FilmController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FilmController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Film
        public async Task<IActionResult> Index()
        {
            return View(await _context.Film.ToListAsync());
        }

        // GET: Film/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Film
                .FirstOrDefaultAsync(m => m.Id == id);
            if (film == null)
            {
                return NotFound();
            }

            
            var projekcije = await _context.Projekcija
                .Where(p => p.FilmId == id)
                .Select(p => new
                {
                    p.Id,
                    p.Lokacija,
                    Datum = p.Datum.ToDateTime(TimeOnly.MinValue),
                    Vrijeme = p.Vrijeme.ToTimeSpan(),
                    DvoranaNaziv = _context.Dvorana
                        .Where(d => d.Id == p.DvoranaId)
                        .Select(d => d.NazivDvorane)
                        .FirstOrDefault()
                })
                .ToListAsync();

            ViewBag.Projekcije = projekcije;


            

            return View(film);
        }

        [Authorize(Roles = "Administrator")]
        // GET: Film/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Film/Create
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Zanr,VrijemeTrajanja,Uloge,Sinopsis,Reziser,Trailer,NazivFilma,Poster")] Film film)
        {
            if (ModelState.IsValid)
            {
                _context.Add(film);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(film);
        }

        [Authorize(Roles = "Administrator")]
        // GET: Film/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Film.FindAsync(id);
            if (film == null)
            {
                return NotFound();
            }
            return View(film);
        }

        // POST: Film/Edit/5
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Zanr,VrijemeTrajanja,Uloge,Sinopsis,Reziser,Trailer,NazivFilma,Poster")] Film film)
        {
            if (id != film.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(film);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmExists(film.Id))
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
            return View(film);
        }

        [Authorize(Roles = "Administrator")]
        // GET: Film/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Film
                .FirstOrDefaultAsync(m => m.Id == id);
            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

        private bool FilmExists(int id)
        {
            return _context.Film.Any(e => e.Id == id);
        }
        [Authorize(Roles = "Administrator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    
                    var projekcijeIds = await _context.Projekcija
                        .Where(p => p.FilmId == id)
                        .Select(p => p.Id)
                        .ToListAsync();

                    var rezervacije = await _context.Rezervacija
                        .Where(r => projekcijeIds.Contains(r.ProjekcijaId))
                        .ToListAsync();

                    foreach (var rezervacija in rezervacije)
                    {
                        var karte = await _context.Karta
                            .Where(k => k.RezervacijaId == rezervacija.Id)
                            .ToListAsync();
                        _context.Karta.RemoveRange(karte);
                    }
                    _context.Rezervacija.RemoveRange(rezervacije);

                    
                    var sveProjekcije = await _context.Projekcija
                        .Where(p => p.FilmId == id || p.FilmId == null)
                        .ToListAsync();
                    _context.Projekcija.RemoveRange(sveProjekcije);

                    
                    var film = await _context.Film.FindAsync(id);
                    if (film != null)
                    {
                        _context.Film.Remove(film);
                    }

                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    
                    return RedirectToAction(nameof(Index));
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
