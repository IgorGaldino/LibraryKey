using LibraryKeyApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryKeyApp.Services
{
    public class EmprestimoService
    {
        private readonly ClassContext _context;

        public EmprestimoService(ClassContext context)
        {
            _context = context;
        }

        public Task<List<Emprestimo>> FindAfterLocacao(DateTime data)
        {
            return _context.Emprestimos.Where(e => e.Locacao >= data).ToListAsync();
        }

        public Task<List<Emprestimo>> FindAfterDevolucao(DateTime data)
        {
            return _context.Emprestimos.Where(e => e.Devolucao >= data ).ToListAsync();
        }

        public Task<List<Emprestimo>> FindLocacao(int day)
        {
            return _context.Emprestimos.Where(e => e.Locacao.Day == day).ToListAsync();
        }

        public Task<List<Emprestimo>> FindDevolucao(int day)
        {
            return _context.Emprestimos.Where(e => e.Devolucao.Day == day).ToListAsync();
        }

        public Task<List<Emprestimo>> FindNaoDevolvido()
        {
            return _context.Emprestimos.Where(e => e.Devolucao == DateTime.MinValue).ToListAsync();
        }

        public async Task<List<Emprestimo>> ConcatenaEmprestimo(List<Emprestimo> emprestimos)
        {
            foreach (var emprestimo in emprestimos)
            {
                var aluno = await _context.Alunos
                .FirstOrDefaultAsync(a => a.Id == emprestimo.AlunoId);

                var chave = await _context.Chaves
                .FirstOrDefaultAsync(c => c.Id == emprestimo.ChaveId);
                emprestimo.Mat = aluno.Matricula;
                emprestimo.NomeAluno = aluno.Nome;
                emprestimo.numChave = chave.NumeroChave;

            }
            return emprestimos;
        }
    }
}
