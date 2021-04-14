using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryKeyApp.Models
{
    public class Aluno
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Campo obrigatório")]
        public string Matricula { get; set; }

        [Required(ErrorMessage = "Campo obrigatório")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Campo obrigatório")]
        [DataType(DataType.Password)]
        public string Senha { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Campo obrigatório")]
        [DataType(DataType.Password)]
        [Compare("Senha", ErrorMessage = "As senhas não conferem")]
        public string ConfirmarSenha { get; set; }


        [Required(ErrorMessage = "Campo obrigatório")]
        public string Turno { get; set; }

        [Required(ErrorMessage = "Campo obrigatório")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string Telefone { get; set; }

		public Emprestimo Emprestimo { get; set; }
	}
}
