using System;
using System.Collections.Generic;
using PotirendabaApp.Data;
using PotirendabaApp.Models;

namespace PotirendabaApp.Services
{
    public static class VendaService
    {
        public static readonly string[] FormasPagamento =
            { "Dinheiro", "Pix", "Cartão de Crédito", "Cartão de Débito", "Vale" };

        // ── Leitura ───────────────────────────────────────────────────────────
        public static List<Venda> Listar(
            string filtro = "",
            bool porNome = false, bool porCodigo = false,
            bool porData = false, bool porValor = false)
            => DatabaseHelper.ListarVendas(filtro, porNome, porCodigo, porData, porValor);

        public static Venda BuscarComItens(int id)
            => DatabaseHelper.BuscarVendaComItens(id);

        // ── Cálculo ───────────────────────────────────────────────────────────
        public static decimal CalcularValorItem(decimal valorUnit, int qtd, string descStr)
        {
            decimal bruto = valorUnit * qtd;
            if (string.IsNullOrWhiteSpace(descStr)) return bruto;
            descStr = descStr.Trim();
            if (descStr.EndsWith("%"))
            {
                if (decimal.TryParse(descStr.TrimEnd('%').Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal pct))
                    return bruto - bruto * pct / 100m;
            }
            else
            {
                if (decimal.TryParse(descStr.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal val))
                    return Math.Max(0, bruto - val);
            }
            return bruto;
        }

        // ── Validação ─────────────────────────────────────────────────────────
        public static ResultadoValidacao Validar(
            List<ItemVenda> itens, string aluno, string formaPgto)
        {
            if (itens == null || itens.Count == 0)
                return new ResultadoValidacao("itens", "Adicione pelo menos um produto à venda.");
            if (string.IsNullOrWhiteSpace(aluno))
                return new ResultadoValidacao("aluno", "Selecione um aluno para a venda.");
            if (string.IsNullOrWhiteSpace(formaPgto))
                return new ResultadoValidacao("pagamento", "Selecione a forma de pagamento.");
            return new ResultadoValidacao();
        }

        // ── Persistência ──────────────────────────────────────────────────────
        public static int Finalizar(
            List<ItemVenda> itens, string aluno, string formaPgto)
        {
            decimal total = 0;
            foreach (var i in itens) total += i.ValorTotal;

            var venda = new Venda
            {
                Cliente        = aluno,
                ValorTotal     = total,
                Data           = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                FormaPagamento = formaPgto,
                Status         = "Ativa",
                Itens          = new List<ItemVenda>(itens)
            };

            int vendaId = DatabaseHelper.InserirVenda(venda);
            foreach (var item in itens)
                ProdutoService.DebitarEstoque(item.ProdutoId, item.Quantidade);

            return vendaId;
        }
    }

    public class ResultadoValidacao
    {
        public bool   Valido   { get; }
        public string Campo    { get; }
        public string Mensagem { get; }

        public ResultadoValidacao()
            { Valido = true; Campo = ""; Mensagem = ""; }
        public ResultadoValidacao(string campo, string mensagem)
            { Valido = false; Campo = campo; Mensagem = mensagem; }
    }
}
