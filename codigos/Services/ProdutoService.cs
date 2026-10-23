using System.Collections.Generic;
using PotirendabaApp.Data;
using PotirendabaApp.Models;

namespace PotirendabaApp.Services
{
    public static class ProdutoService
    {
        // ── Leitura ───────────────────────────────────────────────────────────
        public static List<Produto> Listar(
            string filtro = "",
            bool porNome = false, bool porCodigo = false,
            bool ordenarPorNome = false,
            FiltroStatus status = FiltroStatus.Ativos)
            => DatabaseHelper.ListarProdutos(filtro, porNome, porCodigo, ordenarPorNome, status);

        public static List<Produto> ListarTodos()
            => DatabaseHelper.ListarProdutos(status: FiltroStatus.Todos);

        public static Produto BuscarPorId(int id)
            => DatabaseHelper.BuscarProdutoPorId(id);

        // ── Escrita ───────────────────────────────────────────────────────────
        public static int Inserir(Produto produto)
            => DatabaseHelper.InserirProduto(produto);

        public static void Atualizar(Produto produto)
            => DatabaseHelper.AtualizarProduto(produto);

        public static void AlternarStatus(int id, bool ativo)
            => DatabaseHelper.AlternarStatusProduto(id, ativo);

        public static void Excluir(int id)
            => DatabaseHelper.ExcluirProduto(id);

        /// <summary>
        /// MÉTODO ANTERIOR ao controle de lotes. Não é mais chamado pelo fluxo de
        /// venda (ver VendaService.Finalizar + LoteService.ConsumirFEFO), pois
        /// agora Produtos.Estoque é recalculado automaticamente a partir da soma
        /// dos Lotes ativos. Mantido apenas para não quebrar código legado/testes
        /// que ainda dependam dele.
        /// </summary>
        public static void DebitarEstoque(int produtoId, int quantidade)
        {
            var p = BuscarPorId(produtoId);
            if (p == null) return;
            p.Estoque = System.Math.Max(0, p.Estoque - quantidade);
            Atualizar(p);
        }

        // ── Utilitário ────────────────────────────────────────────────────────
        public static int ProximoId()
        {
            var todos = ListarTodos();
            int proximo = 1;
            foreach (var p in todos)
                if (p.Id >= proximo) proximo = p.Id + 1;
            return proximo;
        }
    }
}
