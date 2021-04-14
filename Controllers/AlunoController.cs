using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryKeyApp.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using X.PagedList;

namespace LibraryKeyApp.Controllers
{
    public class AlunoController : Controller
    {
        private readonly ClassContext _context;
        private const int ITENSPAGINA = 15;
        private const string AUTHSESSION = "Administrador";
        private const string AUTHPERMISSION = "Permissao";
        private const string MAIL_SEND = "<MAIL_SEND>";
        private const string PASS_SEND = "<PASS_SEND>";

        public AlunoController(ClassContext context)
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


        // GET: Aluno
        public async Task<IActionResult> Index(int? pagina)
        {
            int numeroPagina = (pagina ?? 1);
            if (!Permissao())
                return Redirect("~/Administrador/Login");

            return View(await _context.Alunos.OrderByDescending(d => d.Id).ToPagedListAsync(numeroPagina, ITENSPAGINA));
        }

        [HttpPost]
        public async Task<IActionResult> Index(string busca)
        {
            if (String.IsNullOrEmpty(busca))
                return Redirect("~/Aluno/Index");

            var buscaDeAlunos = await _context.Alunos.Where(a => a.Nome.ToUpper().Contains(busca.ToUpper())).ToListAsync();
            var numeroDeAlunos = await _context.Alunos.CountAsync(a => a.Nome.ToUpper().Contains(busca.ToUpper()));
            return View(await buscaDeAlunos.ToPagedListAsync(1, numeroDeAlunos));  //Manda para o Index.cshtml com a Lista emprestimos
        }
        // GET: Aluno/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!Permissao())
                return Redirect("~/Administrador/Login");

            if (id == null)
            {
                return NotFound();
            }

            var aluno = await _context.Alunos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aluno == null)
            {
                return NotFound();
            }

            return View(aluno);
        }

        // GET: Aluno/Create
        public IActionResult Create()
        {
            ViewBag.Turno = new[]{
                new SelectListItem(){Value = "Manhã", Text = "Manhã"},
                new SelectListItem(){Value = "Tarde", Text = "Tarde"},
                new SelectListItem(){Value = "Noite", Text = "Noite"}
            };
            return View();
        }

        // POST: Aluno/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id, Matricula, Nome, Senha, ConfirmarSenha, Turno, Email, Telefone")] Aluno aluno)
        {
            if (ModelState.IsValid)
            {

            var matriculaExiste = await _context.Alunos.FirstOrDefaultAsync(a => a.Matricula == aluno.Matricula);

            if (matriculaExiste != null)
            {
                TempData["erroExeption"] = "Esta matrícula já está cadastrada.";
                return RedirectToAction(nameof(Create));
            }

                _context.Add(aluno);
                await _context.SaveChangesAsync();
                TempData["successMessage"] = "Cadastro realizado com sucesso!";
                return Redirect("~/Emprestimo/Create");
            }
            return View(aluno);
        }

        // GET: Aluno/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!Permissao())
                return Redirect("~/Administrador/Login");

            if (id == null)
            {
                return NotFound();
            }

            var aluno = await _context.Alunos.FindAsync(id);
            if (aluno == null)
            {
                return NotFound();
            }

            ViewBag.Turno = new[]{
                new SelectListItem(){Value = "Manhã", Text = "Manhã"},
                new SelectListItem(){Value = "Tarde", Text = "Tarde"},
                new SelectListItem(){Value = "Noite", Text = "Noite"}
            };
            return View(aluno);
        }

        // POST: Aluno/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Matricula,Nome,Senha,ConfirmarSenha,Turno,Email,Telefone")] Aluno aluno)
        {
            if (id != aluno.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aluno);
                    await _context.SaveChangesAsync();
                    TempData["successMessage"] = "Cadastro realizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlunoExists(aluno.Id))
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
            return View(aluno);
        }

        // GET: Aluno/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!Permissao())
                return Redirect("~/Administrador/Login");

            if (id == null)
            {
                return NotFound();
            }

            var aluno = await _context.Alunos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aluno == null)
            {
                return NotFound();
            }

            return View(aluno);
        }

        // POST: Aluno/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aluno = await _context.Alunos.FindAsync(id);
            _context.Alunos.Remove(aluno);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Aluno/RecuperaSenha
        public IActionResult RecuperaSenha()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperaSenha([Bind("Id,Matricula, Email")] Aluno aluno)
        {
            var estudante = await _context.Alunos
                .FirstOrDefaultAsync(al => al.Matricula == aluno.Matricula && al.Email == aluno.Email);

            if (estudante != null)
            {
                SendMail(estudante);
            }

            TempData["infoMessage"] = "Um email será enviado à sua caixa de entrada, " +
                "se não receber em alguns instantes, contate o administrador.";

            return Redirect("~/Emprestimo/Create");

        }

        protected void SendMail(Aluno a)
        {
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(MAIL_SEND, PASS_SEND);
            MailMessage mail = new MailMessage();
            mail.Sender = new System.Net.Mail.MailAddress(MAIL_SEND);
            mail.From = new MailAddress(MAIL_SEND);
            mail.To.Add(new MailAddress(a.Email));
            mail.Subject = "Contato";
            mail.Body = "Recuperação de Senha <br/> Sua senha é: " + a.Senha;
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;

            client.Send(mail);
        }

        private bool AlunoExists(int id)
        {
            return _context.Alunos.Any(e => e.Id == id);
        }
    }
}
