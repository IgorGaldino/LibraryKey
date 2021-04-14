using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryKeyApp.Models
{
    public class Emprestimo
    {
        public int Id { get; set; }

        public DateTime Locacao { get; set; }

        public DateTime Devolucao { get; set; }

		public int AlunoId { get; set; }
		public Aluno Aluno { get; set; }

        
        public int ChaveId { get; set; }

		public Chave Chave { get; set; }

		[NotMapped]
		public string Mat { get; set; }
		[NotMapped]
		public int numChave { get; set; }
		[NotMapped]
        public string Senha { get; set; }
        [NotMapped]
        public string NomeAluno { get; set; }
    }
}
