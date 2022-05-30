using Testes_da_Mariana.Dominio.Compartilhado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testes_da_Mariana.Dominio.ModuloQuestao
{
    public class Alternativa
    {
        public int Id_Questao { get; set; }
        public int Numero { get; set; }
        public string Descricao { get; set; }
        public bool Correta { get; set; }
        public char Letra { get; set; }

        public override string ToString()
        {
            return Descricao;
        }
    }
}
