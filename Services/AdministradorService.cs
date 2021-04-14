using LibraryKeyApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryKeyApp.Services
{
    public class AdministradorService
    {
        private readonly ClassContext _context;

        public AdministradorService(ClassContext context)
        {
            _context = context;
        }

        public Task<List<Emprestimo>> FindAfterLocacao(DateTime data)
        {
            return _context.Emprestimos.Where(e => e.Locacao >= data).ToListAsync();
        }

        public Task<List<Emprestimo>> FindBeforeLocacao(DateTime data)
        {
            return _context.Emprestimos.Where(e => e.Locacao <= data).ToListAsync();
        }
    }
}
