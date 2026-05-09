using System.Collections.Generic;
using PotirendabaApp.Data;
using PotirendabaApp.Models;

namespace PotirendabaApp.Services
{
    public static class AlunoService
    {
        // ── Leitura ───────────────────────────────────────────────────────────
        public static List<Aluno> Listar(
            string filtro = "",
            bool porNome = false, bool porCodigo = false,
            bool ordenarPorNome = false,
            FiltroStatus status = FiltroStatus.Ativos)
            => DatabaseHelper.ListarAlunos(filtro, porNome, porCodigo, ordenarPorNome, status);

        public static List<Aluno> ListarTodos()
            => DatabaseHelper.ListarAlunos(status: FiltroStatus.Todos);

        public static Aluno BuscarPorId(int id)
        {
            var todos = DatabaseHelper.ListarAlunos(
                status: FiltroStatus.Todos);
            return todos.Find(a => a.Id == id);
        }

        // ── Escrita ───────────────────────────────────────────────────────────
        public static int Inserir(Aluno aluno)
            => DatabaseHelper.InserirAluno(aluno);

        public static void Atualizar(Aluno aluno)
            => DatabaseHelper.AtualizarAluno(aluno);

        public static void AlternarStatus(int id, bool ativo)
            => DatabaseHelper.AlternarStatusAluno(id, ativo);

        public static void Excluir(int id)
            => DatabaseHelper.ExcluirAluno(id);

        // ── Utilitário ────────────────────────────────────────────────────────
        /// <summary>Retorna o próximo ID disponível (max + 1).</summary>
        public static int ProximoId()
        {
            var todos = ListarTodos();
            int proximo = 1;
            foreach (var a in todos)
                if (a.Id >= proximo) proximo = a.Id + 1;
            return proximo;
        }
    }
}
