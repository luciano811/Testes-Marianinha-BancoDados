using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testes_da_Mariana.Dominio.ModuloQuestao;

namespace Testes_da_Mariana.Infra.BancoDados
{
    public class RepositorioQuestaoEmBancoDados : IRepositorioQuestao
    {
        RepositorioDisciplinaEmBancoDados repositorioDisciplina = new RepositorioDisciplinaEmBancoDados();
        RepositorioMateriaEmBancoDados repositorioMateria = new RepositorioMateriaEmBancoDados();

        private const string enderecoBanco =
             "Data Source=(LocalDB)\\MSSqlLocalDB;" +
             "Initial Catalog=TestesDaMarianaDb;" +
             "Integrated Security=True;" +
             "Pooling=False";

        #region comandos da tabela Questao

        private const string sqlInserir =
           @"INSERT INTO [QUESTAO] 
                (
                    [ENUNCIADO],
                    [ID_DISCIPLINA],
                    [ID_MATERIA]
	            )
	            VALUES
                (
                    @ENUNCIADO,
                    @ID_DISCIPLINA,
                    @ID_MATERIA
                );SELECT SCOPE_IDENTITY();";

        private const string sqlEditar =
            @"UPDATE [QUESTAO]	
		        SET
			        [ENUNCIADO] = @ENUNCIADO,
			        [ID_DISCIPLINA] = @ID_DISCIPLINA,
			        [ID_MATERIA] = @ID_MATERIA
		        WHERE
			        [NUMERO] = @NUMERO";

        private const string sqlExcluir =
           @"DELETE FROM [QUESTAO]
		        WHERE
			        [NUMERO] = @NUMERO";

        private const string sqlSelecionarPorNumero =
           @"SELECT
	                [NUMERO],
                    [ENUNCIADO],
                    [ID_DISCIPLINA],
                    [ID_MATERIA]
              FROM 
	                [QUESTAO]
              WHERE 
	                [NUMERO] = @NUMERO";

        private const string sqlSelecionarTodos =
          @"SELECT
	                [NUMERO],
                    [ENUNCIADO],
                    [ID_DISCIPLINA],
                    [ID_MATERIA]
              FROM 
	                [QUESTAO]";

        #endregion

        #region comandos da tabela Alternativa

        private const string sqlSelecionarAlternativas =
            @"SELECT
                [NUMERO],
	            [DESCRICAO],
		        [CORRETA],
                [LETRA],
		        [ID_QUESTAO]
              FROM 
	            [ALTERNATIVA]
              WHERE 
	            [ID_QUESTAO] = @ID_QUESTAO";

        private const string sqlInserirAlternativas =
            @"INSERT INTO [ALTERNATIVA]
                (
		            [DESCRICAO],
		            [CORRETA],
                    [LETRA],
		            [ID_QUESTAO]
	            )
                 VALUES
                (
		            @DESCRICAO,
		            @CORRETA,
		            @LETRA,
                    @ID_QUESTAO
	            ); SELECT SCOPE_IDENTITY();";

        private const string sqlEditarAlternativas =
           @"UPDATE [ALTERNATIVA]	
		        SET
			        [DESCRICAO] = @DESCRICAO,
		            [CORRETA] = @CORRETA,
                    [LETRA] = @LETRA
		        WHERE
			        [NUMERO] = @NUMERO";

        private const string sqlExcluirAlternativas =
            @"DELETE FROM [ALTERNATIVA]
		        WHERE
			        [ID_QUESTAO] = @ID_QUESTAO";

        #endregion

        public ValidationResult Inserir(Questao novaQuestao)
        {
            var validador = new ValidadorQuestao();

            var resultadoValidacao = validador.Validate(novaQuestao);

            if (resultadoValidacao.IsValid == false)
                return resultadoValidacao;

            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoInsercao = new SqlCommand(sqlInserir, conexaoComBanco);

            ConfigurarParametrosQuestao(novaQuestao, comandoInsercao);

            conexaoComBanco.Open();
            var id = comandoInsercao.ExecuteScalar();
            novaQuestao.Numero = Convert.ToInt32(id);

            conexaoComBanco.Close();

            return resultadoValidacao;
        }

        public ValidationResult Editar(Questao questao)
        {
            var validador = new ValidadorQuestao();

            var resultadoValidacao = validador.Validate(questao);

            if (resultadoValidacao.IsValid == false)
                return resultadoValidacao;

            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoEdicao = new SqlCommand(sqlEditar, conexaoComBanco);

            ConfigurarParametrosQuestao(questao, comandoEdicao);

            conexaoComBanco.Open();
            comandoEdicao.ExecuteNonQuery();

            conexaoComBanco.Close();

            return resultadoValidacao;
        }

        public ValidationResult Excluir(Questao questao)
        {
            ExcluirAlternativasQuestao(questao);

            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoExclusao = new SqlCommand(sqlExcluir, conexaoComBanco);

            comandoExclusao.Parameters.AddWithValue("NUMERO", questao.Numero);

            conexaoComBanco.Open();
            int numeroRegistrosExcluidos = comandoExclusao.ExecuteNonQuery();

            var resultadoValidacao = new ValidationResult();

            if (numeroRegistrosExcluidos == 0)
                resultadoValidacao.Errors.Add(new ValidationFailure("", "Não foi possível remover o registro"));

            conexaoComBanco.Close();

            return resultadoValidacao;
        }

        public Questao SelecionarPorNumero(int numero)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoSelecao = new SqlCommand(sqlSelecionarPorNumero, conexaoComBanco);

            comandoSelecao.Parameters.AddWithValue("NUMERO", numero);

            conexaoComBanco.Open();
            SqlDataReader leitorTarefa = comandoSelecao.ExecuteReader();

            Questao questao = null;

            if (leitorTarefa.Read())
                questao = ConverterParaQuestao(leitorTarefa);

            conexaoComBanco.Close();

            CarregarAlternativasQuestao(questao);

            return questao;
        }

        public List<Questao> SelecionarTodos()
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoSelecao = new SqlCommand(sqlSelecionarTodos, conexaoComBanco);

            conexaoComBanco.Open();
            SqlDataReader leitorQuestao = comandoSelecao.ExecuteReader();

            //subs questoes normal
            List<Questao> _questoes = new List<Questao>();

            while (leitorQuestao.Read())
            {
                Questao questao = ConverterParaQuestao(leitorQuestao);

                _questoes.Add(questao);
            }

            conexaoComBanco.Close();

            return _questoes;
        }

        public void AtualizarAlternativas(Questao questaoSelecionada, List<Alternativa> alternativas)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);
            conexaoComBanco.Open();

            foreach (var item in alternativas)
            {
                SqlCommand comandoEdicao = new SqlCommand(sqlEditarAlternativas, conexaoComBanco);

                ConfigurarParametrosAlternativasQuestao(item, comandoEdicao);

                comandoEdicao.ExecuteNonQuery();
            }

            conexaoComBanco.Close();

            Editar(questaoSelecionada);
        }

        public void AdicionarAlternativas(Questao questaoSelecionada, List<Alternativa> alternativas)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);
            conexaoComBanco.Open();

            foreach (var item in alternativas)
            {
                if (item.Numero > 0)
                    continue;

                item.Id_Questao = questaoSelecionada.Numero;
                questaoSelecionada.AdicionarAlternativa(item);

                SqlCommand comandoInsercao = new SqlCommand(sqlInserirAlternativas, conexaoComBanco);

                ConfigurarParametrosAlternativasQuestao(item, comandoInsercao);
                var id = comandoInsercao.ExecuteScalar();
                item.Numero = Convert.ToInt32(id);
            }

            conexaoComBanco.Close();

            Editar(questaoSelecionada);
        }

        private void ConfigurarParametrosAlternativasQuestao(Alternativa alternativa, SqlCommand comando)
        {
            comando.Parameters.AddWithValue("NUMERO", alternativa.Numero);
            comando.Parameters.AddWithValue("DESCRICAO", alternativa.Descricao);
            comando.Parameters.AddWithValue("CORRETA", alternativa.Correta);
            comando.Parameters.AddWithValue("LETRA", alternativa.Letra);
            comando.Parameters.AddWithValue("ID_QUESTAO", alternativa.Id_Questao);
        }

        private void ConfigurarParametrosQuestao(Questao questao, SqlCommand comando)
        {
            comando.Parameters.AddWithValue("NUMERO", questao.Numero);
            comando.Parameters.AddWithValue("ENUNCIADO", questao.Enunciado);
            comando.Parameters.AddWithValue("ID_DISCIPLINA", questao.Disciplina.Numero);
            comando.Parameters.AddWithValue("ID_MATERIA", questao.Materia.Numero);
        }

        private void CarregarAlternativasQuestao(Questao questao)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoSelecao = new SqlCommand(sqlSelecionarAlternativas, conexaoComBanco);

            comandoSelecao.Parameters.AddWithValue("ID_QUESTAO", questao.Numero);

            conexaoComBanco.Open();
            SqlDataReader leitorAlternativasQuestao = comandoSelecao.ExecuteReader();

            //List<ItemTarefa> itensTarefa = new List<ItemTarefa>();

            while (leitorAlternativasQuestao.Read())
            {
                Alternativa alternativa = ConverterParaAlternativa(leitorAlternativasQuestao);

                questao.AdicionarAlternativa(alternativa);
                //itensTarefa.Add(itemTarefa);
            }

            conexaoComBanco.Close();
        }

        private void ExcluirAlternativasQuestao(Questao questao)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoExclusao = new SqlCommand(sqlExcluirAlternativas, conexaoComBanco);

            comandoExclusao.Parameters.AddWithValue("ID_QUESTAO", questao.Numero);

            conexaoComBanco.Open();
            comandoExclusao.ExecuteNonQuery();

            conexaoComBanco.Close();
        }

        private Questao ConverterParaQuestao(SqlDataReader leitorQuestao)
        {
            var numero = Convert.ToInt32(leitorQuestao["NUMERO"]);
            var enunciado = Convert.ToString(leitorQuestao["ENUNCIADO"]);
            var id_disciplina = Convert.ToInt32(leitorQuestao["ID_DISCIPLINA"]);
            var id_materia = Convert.ToInt32(leitorQuestao["ID_MATERIA"]);

            var questao = new Questao
            {
                Numero = numero,
                Enunciado = enunciado,
                Disciplina = repositorioDisciplina.SelecionarPorNumero(id_disciplina),
                Materia = repositorioMateria.SelecionarPorNumero(id_materia)
            };

            return questao;
        }

        private Alternativa ConverterParaAlternativa(SqlDataReader leitorAlternativa)
        {
            var numero = Convert.ToInt32(leitorAlternativa["NUMERO"]);
            var descricao = Convert.ToString(leitorAlternativa["DESCRICAO"]);
            var correta = Convert.ToBoolean(leitorAlternativa["CORRETA"]);
            var letra = Convert.ToChar(leitorAlternativa["LETRA"]);
            var id_questao = Convert.ToInt32(leitorAlternativa["ID_QUESTAO"]);

            var alternativa = new Alternativa
            {
                Numero = numero,
                Descricao = descricao,
                Correta = correta,
                Letra = letra,
                Id_Questao = id_questao
            };

            return alternativa;
        }

    }
}
