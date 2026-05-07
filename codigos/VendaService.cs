using System;
using System.Collections.Generic;
using PotirendabaApp.Data;
using PotirendabaApp.Models;

namespace PotirendabaApp.Services
{
    public static class VendaService
    {
        public static readonly string[] FormasPagamento =
            { "Dinheiro", "Cartao", "Pix" };

        /// <summary>
        /// Calcula valor final do item. Desconto com % = percentual, sem % = valor fixo.
        /// </summary>
        public static decimal CalcularValorItem(decimal valorUnit, int qtd, string descStr)
        {
            decimal bruto = valorUnit * qtd;
            if (string.IsNullOrWhiteSpace(descStr)) return bruto;

            descStr = descStr.Trim();
            bool pct = descStr.EndsWith("%");
            descStr  = descStr.TrimEnd('%').Replace(",", ".");

            if (!decimal.TryParse(descStr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal desc))
                return bruto;

            decimal descValor = pct ? bruto * desc / 100m : desc;
            return Math.Max(0, bruto - descValor);
        }

        public class ResultadoValidacao
        {
            public bool   Valido   { get; set; } = true;
            public string Mensagem { get; set; } = "";
            public string Campo    { get; set; } = "";
        }

        public static ResultadoValidacao Validar(
            List<ItemVenda> itens, string aluno, string formaPgto)
        {
            if (itens == null || itens.Count == 0)
                return new ResultadoValidacao { Valido=false,
                    Mensagem="Adicione ao menos um produto na venda.", Campo="produto" };
            if (string.IsNullOrWhiteSpace(aluno))
                return new ResultadoValidacao { Valido=false,
                    Mensagem="Selecione um aluno antes de finalizar.", Campo="aluno" };
            if (string.IsNullOrWhiteSpace(formaPgto))
                return new ResultadoValidacao { Valido=false,
                    Mensagem="Selecione uma forma de pagamento.", Campo="pagamento" };
            return new ResultadoValidacao { Valido=true };
        }

        public static int Finalizar(List<ItemVenda> itens, string aluno, string formaPgto)
        {
            decimal total = 0;
            foreach (var i in itens) total += i.ValorTotal;

            var venda = new Venda
            {
                Cliente=aluno, ValorTotal=total,
                Data=DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                FormaPagamento=formaPgto, Status="Ativa",
                Itens=new List<ItemVenda>(itens)
            };
            int id = DatabaseHelper.InserirVenda(venda);
            foreach (var item in itens)
                ProdutoService.DebitarEstoque(item.ProdutoId, item.Quantidade);
            return id;
        }
    }
}
