using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryKeyApp.Models;
using X.PagedList;
using Microsoft.AspNetCore.Http;

namespace LibraryKeyApp.Controllers
{
    public class ChaveController : Controller
    {
        private readonly ClassContext _context;
        private const int ITENSPAGINA = 20;
        private const string AUTHSESSION = "Administrador";
        private const string AUTHPERMISSION = "Permissao";

        public ChaveController(ClassContext context)
        {
            _context = context;
        }

        public bool Permissao()
        {
            try
            {
                var auth = HttpContext.Session.GetString(AUTHSESSION);
                if (auth != null)
                    if (auth == AUTHPERMISSION)
                        return true;
            }
            catch (Exception) { }
            return false;
        }


        // GET: Chave
        public async Task<IActionResult> Index(int? pagina)
        {
            int numeroPagina = (pagina ?? 1);
            if (!Permissao())
                return Redirect("~/Administrador/Login");

            return View(await _context.Chaves.ToPagedListAsync(numeroPagina, ITENSPAGINA));
        }

        // GET: Chave/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!Permissao())
                return Redirect("~/Administrador/Login");

            if (id == null)
            {
                return NotFound();
            }

            var chave = await _context.Chaves
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chave == null)
            {
                return NotFound();
            }

            return View(chave);
        }

        // GET: Chave/Create
        public IActionResult Create()
        {
            if (!Permissao())
                return Redirect("~/Administrador/Login");
            return View();
        }

        // POST: Chave/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NumeroChave,Disponivel")] Chave chave)
        {
            //DISPONIBILIDADE
            chave.Disponivel = true;

            if (ModelState.IsValid)
            {
                var chaveExiste = await _context.Chaves.FirstOrDefaultAsync(c => c.NumeroChave == chave.NumeroChave);

                if (chaveExiste != null)
                {
                    TempData["erroExeption"] = "Uma chave já está cadastrada com esse número, tente outra vez.";
                    return RedirectToAction(nameof(Create));
                }
                _context.Add(chave);
                await _context.SaveChangesAsync();
                TempData["successMessage"] = "Cadastro realizado com sucesso!";
                return RedirectToAction(nameof(Create));
            }
            return View(chave);
        }

        // GET: Chave/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!Permissao())
                return Redirect("~/Administrador/Login");

            if (id == null)
            {
                return NotFound();
            }

            var chave = await _context.Chaves.FindAsync(id);
            if (chave == null)
            {
                return NotFound();
            }
            return View(chave);
        }

        // POST: Chave/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NumeroChave")] Chave chave)
        {
            if (id != chave.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var chaveExiste = await _context.Chaves.FirstOrDefaultAsync(c => c.NumeroChave == chave.NumeroChave);

                if (chaveExiste != null)
                {
                    TempData["erroExeption"] = "Uma chave já está cadastrada com esse número, tente outra vez.";
                    return RedirectToAction(nameof(Create));
                }

                try
                {
                    _context.Update(chave);
                    await _context.SaveChangesAsync();
                    TempData["successMessage"] = "Edição realizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChaveExists(chave.Id))
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
            return View(chave);
        }

        // GET: Chave/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!Permissao())
                return Redirect("~/Administrador/Login");

            if (id == null)
            {
                return NotFound();
            }

            var chave = await _context.Chaves
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chave == null)
            {
                return NotFound();
            }

            return View(chave);
        }

        // POST: Chave/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chave = await _context.Chaves.FindAsync(id);
            _context.Chaves.Remove(chave);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChaveExists(int id)
        {
            return _context.Chaves.Any(e => e.Id == id);
        }
    }
}
