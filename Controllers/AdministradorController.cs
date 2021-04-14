using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryKeyApp.Models;
using LibraryKeyApp.Services;
using Microsoft.AspNetCore.Http;
using X.PagedList;
using System.Net.Mail;

namespace LibraryKeyApp.Controllers
{
    public class AdministradorController : Controller
    {
        private readonly ClassContext _context;
        private readonly EmprestimoService emprestimoService;
        private const int ITENSPAGINA = 15;
        private const string AUTHSESSION = "Administrador";
        private const string AUTHPERMISSION = "Permissao";
        private const string MAIL_SEND = "<EMAIL_SEND>"
        private const string PASS_SEND = "<PASS_SEND>"
        
        public AdministradorController(ClassContext context)
        {
            _context = context;
            emprestimoService = new EmprestimoService(context);
        }


        // GET: Administrador
        public async Task<IActionResult> List(int? pagina)
        {
            int numeroPagina = (pagina ?? 1);
            if (!Permissao())
                return Redirect("~/Administrador/Login");

            return View(await _context.Administradores.ToPagedListAsync(numeroPagina, ITENSPAGINA));
        }

        public async Task<IActionResult> DevolverChave(bool confirm, int empId)
        {
            if(!confirm)
                return RedirectToAction(nameof(Index));
            var emprestimo = await _context.Emprestimos.FirstOrDefaultAsync(e => e.Id == empId);
            if (emprestimo.Devolucao != DateTime.MinValue)
            {
                TempData["infoMessage"] = "Chave não foi emprestada!";
                return RedirectToAction(nameof(Index));
            }
                
            if (emprestimo != null) {
                //VERIFICA DISPONIBILIDADE DA CHAVE E ATUALIZA ESTADO
                var chave = await _context.Chaves.FirstOrDefaultAsync(c => c.Id == emprestimo.ChaveId);

                if (chave != null)
                    emprestimo = await _context.Emprestimos.LastOrDefaultAsync(e => e.AlunoId == emprestimo.AlunoId && chave.Id == e.ChaveId && e.Devolucao == DateTime.MinValue);

                emprestimo.Devolucao = DateTime.Now;
                
                ////DISPONIBILIDADE
                chave.Disponivel = true;
                
                _context.Update(emprestimo);
                await _context.SaveChangesAsync();
                TempData["successMessage"] = "Chave devolvida!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Administrador (Emprestimo)
        public async Task<IActionResult> Index(int? pagina)
        {
            ViewBag.Count = await _context.Emprestimos.CountAsync(d => d.Devolucao == DateTime.MinValue);
            if (!Permissao())
            {
                return RedirectToAction(nameof(Login));
            }
            else
            {
               
                int numeroPagina = (pagina ?? 1);
                
                
                //return View(await contexto.ToPagedListAsync(numeroPagina, ITENSPAGINA));

                var emprestimos = await _context.Emprestimos.OrderBy(e => e.numChave).ToListAsync();
                emprestimos = await emprestimoService.ConcatenaEmprestimo(emprestimos); //Concatena emprestimo com as chaves e os alunos associados

                //TempData["NaoDevolvido"] = ;

                return View(await emprestimos.OrderByDescending(o => o.Locacao).ToPagedListAsync(numeroPagina, ITENSPAGINA));

            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(DateTime Busca, int? pagina)
        {
            int dia = Busca.Day;
            int mes = Busca.Month;
            int ano = Busca.Year;

            ViewBag.Count = await _context.Emprestimos.CountAsync(d => d.Devolucao == DateTime.MinValue);

            int numeroPagina = (pagina ?? 1);

            
            if (!(Busca == DateTime.MinValue))
            {
                var busca = await _context.Emprestimos.Where(d => d.Locacao.Day == dia).Where(m => m.Locacao.Month == mes).Where(a => a.Locacao.Year == ano).ToListAsync();
                busca = await emprestimoService.ConcatenaEmprestimo(busca); //Concatena emprestimo com as chaves e os alunos associados
                var itens = await _context.Emprestimos.CountAsync();
                return View(await busca.OrderByDescending(o => o.Locacao).ToPagedListAsync(1, itens));
            }
            else
            {
                var busca = await _context.Emprestimos.ToListAsync();
                busca = await emprestimoService.ConcatenaEmprestimo(busca); //Concatena emprestimo com as chaves e os alunos associados
                return View(await busca.OrderByDescending(o => o.Locacao).ToPagedListAsync(numeroPagina, ITENSPAGINA));
            }
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
            catch (Exception){}
            return false;
        }

        public IActionResult WriteSession()
        {
            try
            {
                HttpContext.Session.SetString(AUTHSESSION, AUTHPERMISSION);
            }
            catch(Exception) {}
            return RedirectToAction("Index");
        }

        public IActionResult RemoveSession()
        {
            try
            {
                HttpContext.Session.SetString(AUTHSESSION, "");
            }
            catch(Exception) { }
            return RedirectToAction("Home");
        }

        // GET: Administrador/Login
        public IActionResult Login()
        {
            if (Permissao())
                return RedirectToAction(nameof(Index));
            return View();
        }

        // POST: Administrador/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Id,Login,Senha")] Administrador administrador)
        {
            var admin = await _context.Administradores
                .FirstOrDefaultAsync(ad => ad.Login == administrador.Login && ad.Senha == administrador.Senha);

            if (admin == null)
            {

                TempData["erroExeption"] = "Login ou Senha Incorretos.";
                return RedirectToAction(nameof(Login));
            }
            else
            {
                WriteSession();
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.SetString(AUTHSESSION, "");
            }
            catch (Exception) { }
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> FindNaoDevolvidos(int? pagina)
        {

            var naoDevolvidas = await _context.Emprestimos.CountAsync(d => d.Devolucao == DateTime.MinValue);
            ViewBag.Count = naoDevolvidas;
            int numeroPagina = (pagina ?? 1);
            try
            {
                var emprestimos = await emprestimoService.FindNaoDevolvido();
                emprestimos = await emprestimoService.ConcatenaEmprestimo(emprestimos); //Concatena emprestimo com as chaves e os alunos associados
                
                return View("Index", await emprestimos.ToPagedListAsync(numeroPagina, naoDevolvidas));  //Manda para o Index.cshtml com a Lista emprestimos
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> FindByDay(String day, int opcao)
        {
            try
            {
                List<Emprestimo> emprestimos = null;
                if (opcao == 1)
                    emprestimos = await emprestimoService.FindLocacao(Int32.Parse(day));
                else if (opcao == 2)
                    emprestimos = await emprestimoService.FindDevolucao(Int32.Parse(day));
                else
                    return RedirectToAction(nameof(Index));

                emprestimos = await emprestimoService.ConcatenaEmprestimo(emprestimos); //Concatena emprestimo com as chaves e os alunos associados
                return View("Index", emprestimos);  //Manda para o Index.cshtml com a Lista emprestimos
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Administrador/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrador = await _context.Administradores
                .FirstOrDefaultAsync(m => m.Id == id);
            if (administrador == null)
            {
                return NotFound();
            }

            return View(administrador);
        }

        // GET: Administrador/Create
        public IActionResult Create()
        {
            if (!Permissao()) 
                return RedirectToAction(nameof(Login));

            return View();
        }

        // POST: Administrador/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id, Login, Email, Senha, ConfirmarSenha")] Administrador administrador)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var administradorExiste = await _context.Administradores.FirstOrDefaultAsync(a => a.Login == administrador.Login);

                    if (administradorExiste != null)
                    {
                        TempData["erroExeption"] = "Este usuário não está disponível, tente outro.";
                        return RedirectToAction(nameof(Create));
                    }
                    _context.Add(administrador);
                    await _context.SaveChangesAsync();
                    TempData["successMessage"] = "Cadastro realizado com sucesso!";
                    return RedirectToAction(nameof(List));
                }
            } catch(Exception) { }
            return View(administrador);
        }

        // GET: Administrador/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!Permissao())
                return RedirectToAction(nameof(Login));

            if (id == null)
            {
                return NotFound();
            }

            var administrador = await _context.Administradores.FindAsync(id);
            if (administrador == null)
            {
                return NotFound();
            }
            return View(administrador);
        }

        // POST: Administrador/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Login,Email,Senha,ConfirmarSenha")] Administrador administrador)
        {
            var verifica = await _context.Administradores.FirstOrDefaultAsync(u => u.Login == administrador.Login && administrador.Id != u.Id);

            if (verifica != null)
            {
                TempData["erroExeption"] = "Este usuário não está disponível, tente outro.";
                return RedirectToAction(nameof(Edit));
            }

            if (id != administrador.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(administrador);
                    await _context.SaveChangesAsync();
                    TempData["successMessage"] = "Cadastro realizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdministradorExists(administrador.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(List));
            }
            return View(administrador);
        }

        // GET: Administrador/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!Permissao())
                return RedirectToAction(nameof(Login));

            if (id == null)
            {
                return NotFound();
            }

            var administrador = await _context.Administradores
                .FirstOrDefaultAsync(m => m.Id == id);
            if (administrador == null)
            {
                return NotFound();
            }

            return View(administrador);
        }

        // GET: Administrador/RecuperaSenha
        public IActionResult RecuperaSenha()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperaSenha([Bind("Id,Login,Email")] Administrador administrador)
        {
            var adm = await _context.Administradores
                .FirstOrDefaultAsync(a => a.Login == administrador.Login && a.Email == administrador.Email);

            if (adm != null)
            {
                SendMail(adm);
            }

            TempData["infoMessage"] = "Um email será enviado à sua caixa de entrada, " +
                "se não receber em alguns instantes, contate o suporte de desenvolvimento.";

            return Redirect("~/Administrador/Login");

        }

        protected void SendMail(Administrador a)
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

        // POST: Administrador/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var administrador = await _context.Administradores.FindAsync(id);
            _context.Administradores.Remove(administrador);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        private bool AdministradorExists(int id)
        {
            return _context.Administradores.Any(e => e.Id == id);
        }
    }
}
