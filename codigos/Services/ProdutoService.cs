using System.Collections.Generic;
using PotirendabaApp.Data;
using PotirendabaApp.Models;
using static PotirendabaApp.Data.DatabaseHelper;

namespace PotirendabaApp.Services
{
    public static class ProdutoService
    {
        public static Produto BuscarPorId(int id) =>
            DatabaseHelper.BuscarProdutoPorId(id);

        /// <summary>Lista produtos. Para o PDV use status=Ativos (padrão).</summary>
        public static List<Produto> Listar(string filtro = "",
            bool porNome = true, bool porCodigo = false,
            FiltroStatus status = FiltroStatus.Ativos) =>
            DatabaseHelper.ListarProdutos(filtro, porNome, porCodigo, porNome, status);

        public static void DebitarEstoque(int produtoId, int quantidade)
        {
            var p = DatabaseHelper.BuscarProdutoPorId(produtoId);
            if (p == null) return;
            p.Estoque = System.Math.Max(0, p.Estoque - quantidade);
            DatabaseHelper.AtualizarProduto(p);
        }
    }
}
