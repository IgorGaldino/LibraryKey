using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace LibraryKeyApp.Models
{
    public class ClassContext : DbContext
    {
		public DbSet<Chave> Chaves { get; set; }
		public DbSet<Aluno> Alunos { get; set; }
		public DbSet<Administrador> Administradores { get; set; }
		public DbSet<Emprestimo> Emprestimos { get; set; }

		public ClassContext(DbContextOptions<ClassContext> options)
            : base(options)
        {
        }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<Chave>().HasIndex(c => c.NumeroChave).IsUnique(true);
			builder.Entity<Aluno>().HasIndex(a => a.Matricula).IsUnique(true);
			builder.Entity<Aluno>().HasIndex(a => a.Email).IsUnique(true);
			builder.Entity<Aluno>().HasIndex(a => a.Telefone).IsUnique(true);
            builder.Entity<Emprestimo>().HasIndex(e => e.ChaveId).IsUnique(false);
            builder.Entity<Emprestimo>().HasIndex(e => e.AlunoId).IsUnique(false);
            builder.Entity<Administrador>().HasIndex(a => a.Login).IsUnique(true);

			builder.Entity<Aluno>()
			.HasOne(a => a.Emprestimo)
			.WithOne(b => b.Aluno)
			.HasForeignKey<Emprestimo>(b => b.AlunoId);

			builder.Entity<Chave>()
			.HasOne(a => a.Emprestimo)
			.WithOne(b => b.Chave)
			.HasForeignKey<Emprestimo>(b => b.ChaveId);

			base.OnModelCreating(builder);
		}

		
    }
}
