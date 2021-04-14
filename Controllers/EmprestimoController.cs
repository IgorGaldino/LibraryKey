using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryKeyApp.Models;
using System.Globalization;
using LibraryKeyApp.Services;

namespace LibraryKeyApp.Controllers
{
    public class EmprestimoController : Controller
    {
        private readonly ClassContext _context;
        private readonly EmprestimoService emprestimoService;

        public EmprestimoController(ClassContext context)
        {
            _context = context;
            emprestimoService = new EmprestimoService(context);
        }

        // GET: Emprestimo/Create
        public IActionResult Create()
        {
			ViewData["ChaveId"] = new SelectList(_context.Chaves, "Id", "NumeroChave", "Disponivel");
			ViewData["Chaves"] = _context.Chaves;
			return View();
        }

        // POST: Emprestimo/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ChaveId,Mat,Senha")] Emprestimo emprestimo)
        {
            
            var aluno = await _context.Alunos
                .FirstOrDefaultAsync(a => a.Matricula == emprestimo.Mat && a.Senha == emprestimo.Senha);

            //verifica se a chave já está disponível, retorna not null se está disponivel
            var chave = await _context.Emprestimos.LastOrDefaultAsync(d => emprestimo.ChaveId == d.ChaveId && d.Devolucao == DateTime.MinValue);

            if (aluno == null)
            {
                TempData["erroExeption"] = "Você não está cadastrado, por favor, realize seu cadastro.";
                return RedirectToAction(nameof(Create));
            }

            var aluno_pendente = await _context.Emprestimos.LastOrDefaultAsync(d => d.AlunoId == aluno.Id && d.Devolucao == DateTime.MinValue);

            if (aluno_pendente != null)
            {
                TempData["erroExeption"] = "Você não devolveu a última chave emprestada.";
                return RedirectToAction(nameof(Create));
            }

            if (chave == null)
            {
                
                emprestimo.AlunoId = aluno.Id;
                emprestimo.Locacao = DateTime.Now;
                emprestimo.Devolucao = DateTime.MinValue;

                //VERIFICA DISPONIBILIDADE DA CHAVE E ATUALIZA ESTADO
                var key = await _context.Chaves.FirstOrDefaultAsync(d => emprestimo.ChaveId == d.Id);
                key.Disponivel = false;


                if (ModelState.IsValid)
				{
                    
					_context.Add(emprestimo);
                    await _context.SaveChangesAsync();
                    TempData["successMessage"] = "Chave Alocada!";
                    return RedirectToAction(nameof(Create));
				}
            }
            
            
			ViewData["ChaveId"] = new SelectList(_context.Chaves, "Id", "NumeroChave", emprestimo.ChaveId);
			return View(emprestimo);
        }

        // GET: Emprestimo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emprestimo = await _context.Emprestimos.FindAsync(id);
            if (emprestimo == null)
            {
                return NotFound();
            }
            return View(emprestimo);
        }

        // POST: Emprestimo/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,[Bind("Id,numChave,Mat,Senha")] Emprestimo emprestimo)
        {
            try
            {
                var aluno = await _context.Alunos
                    .FirstOrDefaultAsync(a => a.Matricula == emprestimo.Mat && a.Senha == emprestimo.Senha);

                if (aluno == null)
                {
                    TempData["erroExeption"] = "Aluno inexistente, verifique suas credenciais.";
                    return RedirectToAction(nameof(Create));
                }

                var chave = await _context.Chaves.FirstOrDefaultAsync(c => c.NumeroChave == emprestimo.numChave);

                if (chave != null)
                    emprestimo = await _context.Emprestimos.LastOrDefaultAsync(e => e.AlunoId == aluno.Id && chave.Id == e.ChaveId && e.Devolucao == DateTime.MinValue);

                if (emprestimo == null)
                {
                    TempData["erroExeption"] = "Aluno não pegou esta chave, verifique suas credenciais.";
                    return RedirectToAction(nameof(Create));
                }
                emprestimo.Devolucao = DateTime.Now;

                //VERIFICA DISPONIBILIDADE DA CHAVE E ATUALIZA ESTADO
                var key = await _context.Chaves.FirstOrDefaultAsync(d => emprestimo.ChaveId == d.Id);

                //DISPONIBILIDADE
                key.Disponivel = true;

                _context.Update(emprestimo);
                await _context.SaveChangesAsync();
                TempData["successMessage"] = "Chave devolvida!";
                return RedirectToAction(nameof(Create));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Create));
            }
        }

    }
}
