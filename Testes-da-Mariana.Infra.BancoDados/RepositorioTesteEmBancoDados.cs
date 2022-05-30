using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testes_da_Mariana.Dominio.ModuloQuestao;
using Testes_da_Mariana.Dominio.ModuloTeste;

namespace Testes_da_Mariana.Infra.BancoDados
{
    public class RepositorioTesteEmBancoDados:IRepositorioTeste
    {

        RepositorioDisciplinaEmBancoDados repositorioDisciplina = new RepositorioDisciplinaEmBancoDados();
        RepositorioMateriaEmBancoDados repositorioMateria = new RepositorioMateriaEmBancoDados();
        //RepositorioQuestaoEmBancoDados repositorioQuestao = new RepositorioQuestaoEmBancoDados();


        private const string enderecoBanco =
               "Data Source=(LocalDB)\\MSSqlLocalDB;" +
               "Initial Catalog=TestesDaMarianaDB;" +
               "Integrated Security=True;" +
               "Pooling=False";

        private const string sqlInserir =
            @"INSERT INTO [TESTE] 
                (
                    [TITULO],
                    [ID_DISCIPLINA],
                    [ID_MATERIA],
                    [DATA]
                    
                    
                )
	            VALUES
                (
                    @TITULO,
                    @ID_DISCIPLINA,
                    @ID_MATERIA,
                    @DATA



                );SELECT SCOPE_IDENTITY();";

        private const string sqlEditar =
            @"UPDATE [TESTE]	
		        SET
			        [TITULO] = @TITULO,
                    [ID_DISCIPLINA] = @ID_DISCIPLINA,
                    [ID_MATERIA] = @ID_MATERIA,
			        [DATA] = @DATA

		        WHERE
			        [NUMERO] = @NUMERO";

        private const string sqlExcluir =
            @"DELETE FROM [TESTE]
		        WHERE
			        [NUMERO] = @NUMERO";

        private const string sqlSelecionarPorNumero =
            @"SELECT
	                T.NUMERO,
                    T.TITULO,
                    T.DATA,
                    D.NUMERO AS ID_DISCIPLINA,
                    D.NOME AS DISCIPLINA_NOME,
                    M.NUMERO AS ID_MATERIA,
                    M.NOME AS MATERIA_NOME
                FROM 
	                TESTE AS T INNER JOIN DISCIPLINA AS D ON
                    T.ID_DISCIPLINA = D.NUMERO
                        INNER JOIN MATERIA AS M ON
                        T.ID_MATERIA = M.NUMERO
                WHERE
                    T.NUMERO = @NUMERO";

        private const string sqlSelecionarTodos =
            @"SELECT
	                T.NUMERO,
                    T.TITULO,
                    T.DATA,
                    D.NUMERO AS ID_DISCIPLINA,
                    D.NOME AS DISCIPLINA_NOME,
                    M.NUMERO AS ID_MATERIA,
                    M.NOME AS MATERIA_NOME
              FROM 
	                TESTE AS T INNER JOIN DISCIPLINA AS D ON
                    T.ID_DISCIPLINA = D.NUMERO
                        INNER JOIN MATERIA AS M ON
                        T.ID_MATERIA = M.NUMERO";

        private const string sqlSelecionarQuestoesTeste =
            @"SELECT
                    [NUMERO],
		            [ENUNCIADO],
                    [ID_MATERIA],
                    [ID_DISCIPLINA]

                FROM QUESTAO
                ";

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

        public ValidationResult Editar(Teste teste)
        {
            var validador = new ValidadorTeste();

            var resultadoValidacao = validador.Validate(teste);

            if (resultadoValidacao.IsValid == false)
                return resultadoValidacao;

            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoEdicao = new SqlCommand(sqlEditar, conexaoComBanco);

            ConfigurarParametrosTeste(teste, comandoEdicao);

            conexaoComBanco.Open();
            comandoEdicao.ExecuteNonQuery();

            conexaoComBanco.Close();

            return resultadoValidacao;
        }

        public ValidationResult Excluir(Teste teste)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoExclusao = new SqlCommand(sqlExcluir, conexaoComBanco);

            comandoExclusao.Parameters.AddWithValue("NUMERO", teste.Numero);

            conexaoComBanco.Open();
            int numeroRegistrosExcluidos = comandoExclusao.ExecuteNonQuery();

            var resultadoValidacao = new ValidationResult();

            if (numeroRegistrosExcluidos == 0)
                resultadoValidacao.Errors.Add(new ValidationFailure("", "Não foi possível remover o registro"));

            conexaoComBanco.Close();

            return resultadoValidacao;
        }

        public ValidationResult Inserir(Teste novoTeste)
        {
            var validador = new ValidadorTeste();

            var resultadoValidacao = validador.Validate(novoTeste);

            if (resultadoValidacao.IsValid == false)
                return resultadoValidacao;

            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoInsercao = new SqlCommand(sqlInserir, conexaoComBanco);

            ConfigurarParametrosTeste(novoTeste, comandoInsercao);

            conexaoComBanco.Open();
            var id = comandoInsercao.ExecuteScalar();
            novoTeste.Numero = Convert.ToInt32(id);

            conexaoComBanco.Close();

            return resultadoValidacao;
        }

        public Teste SelecionarPorNumero(int numero)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoSelecao = new SqlCommand(sqlSelecionarPorNumero, conexaoComBanco);

            comandoSelecao.Parameters.AddWithValue("NUMERO", numero);

            conexaoComBanco.Open();
            SqlDataReader leitorTeste = comandoSelecao.ExecuteReader();

            Teste teste = null;

            if (leitorTeste.Read())
                teste = ConverterParaTeste(leitorTeste);

            conexaoComBanco.Close();

            CarregarQuestoesTeste(teste);

            return teste;
        }

        public List<Teste> SelecionarTodos()
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoSelecao = new SqlCommand(sqlSelecionarTodos, conexaoComBanco);

            conexaoComBanco.Open();
            SqlDataReader leitorTeste = comandoSelecao.ExecuteReader();

            List<Teste> testes = new List<Teste>();

            while (leitorTeste.Read())
            {
                Teste teste = ConverterParaTeste(leitorTeste);

                testes.Add(teste);
            }

            conexaoComBanco.Close();

            return testes;
        }

        private void CarregarQuestoesTeste(Teste teste)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoSelecao = new SqlCommand(sqlSelecionarQuestoesTeste, conexaoComBanco);

            comandoSelecao.Parameters.AddWithValue("NUMERO", teste.Numero);

            conexaoComBanco.Open();

            SqlDataReader leitorQuestao = comandoSelecao.ExecuteReader();

            

            while (leitorQuestao.Read())
            {
                Questao questao = ConverterParaQuestao(leitorQuestao);

                CarregarAlternativasQuestaoTeste(questao);
                //List<Questao> questoes;
                teste.AdicionarQuestaoT(questao);
            }

            conexaoComBanco.Close();

            
        }

        private void CarregarAlternativasQuestaoTeste(Questao questao)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoSelecao = new SqlCommand(sqlSelecionarAlternativas, conexaoComBanco);

            comandoSelecao.Parameters.AddWithValue("ID_QUESTAO", questao.Numero);

            conexaoComBanco.Open();
            SqlDataReader leitorAlternativasQuestao = comandoSelecao.ExecuteReader();

            //List<ItemTarefa> itensTarefa = new List<ItemTarefa>();

            while (leitorAlternativasQuestao.Read())
            {
                Alternativa alternativa = ConverterParaAlternativaTeste(leitorAlternativasQuestao);

                questao.AdicionarAlternativa(alternativa);
                //itensTarefa.Add(itemTarefa);
            }

            conexaoComBanco.Close();
        }

        #region Métodos privados

        private void ConfigurarParametrosTeste(Teste teste, SqlCommand comando)
        {
            comando.Parameters.AddWithValue("NUMERO", teste.Numero);
            comando.Parameters.AddWithValue("TITULO", teste.Titulo);
            comando.Parameters.AddWithValue("DATA", teste.Data);
            comando.Parameters.AddWithValue("ID_MATERIA", teste.Materia.Numero);
            comando.Parameters.AddWithValue("ID_DISCIPLINA", teste.Disciplina.Numero);
        }

        private Teste ConverterParaTeste(SqlDataReader leitorTeste)
        {
            var numero = Convert.ToInt32(leitorTeste["NUMERO"]);
            var titulo = Convert.ToString(leitorTeste["TITULO"]);
            var data = Convert.ToDateTime(leitorTeste["DATA"]);
            var numeroMateria = Convert.ToInt32(leitorTeste["ID_MATERIA"]);
            var numeroDisciplina = Convert.ToInt32(leitorTeste["ID_DISCIPLINA"]);
            //var numeroQuestao = Convert.ToInt32(leitorTeste["ID_QUESTAO"]);


            var teste = new Teste
            {
                Numero = numero,
                Titulo = titulo,
                Data = data,
                Materia = repositorioMateria.SelecionarPorNumero(numeroMateria),
                Disciplina = repositorioDisciplina.SelecionarPorNumero(numeroDisciplina),
                //Questoes = repositorioQuestao.SelecionarPorNumero(numeroQuestao)
            };

            return teste;
        }

        private Questao ConverterParaQuestao(SqlDataReader leitorQuestao)
        {
            var numero = Convert.ToInt32(leitorQuestao["NUMERO"]);
            var enunciado = Convert.ToString(leitorQuestao["ENUNCIADO"]);
            var numeroMateria = Convert.ToInt32(leitorQuestao["ID_MATERIA"]);
            var numeroDisciplina = Convert.ToInt32(leitorQuestao["ID_DISCIPLINA"]);

            var questao = new Questao
            {
                Numero = numero,
                Enunciado = enunciado,
                Materia = repositorioMateria.SelecionarPorNumero(numeroMateria),
                Disciplina = repositorioDisciplina.SelecionarPorNumero(numeroDisciplina)
            };

            return questao;
        }

        private Alternativa ConverterParaAlternativaTeste(SqlDataReader leitorAlternativa)
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

        #endregion
    }



}

