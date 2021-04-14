using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryKeyApp.Models
{
    public class Chave
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Campo obrigatório")]
        public int NumeroChave { get; set; }

        public Emprestimo Emprestimo { get; set; }

        public bool Disponivel { get; set; }
    }
}
