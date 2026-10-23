using System;
using System.Collections.Generic;

namespace PotirendabaApp.Models
{
    /// <summary>
    /// Cabeçalho da venda (uma venda pode ter vários itens).
    /// </summary>
    public class Venda
    {
        public int     Id             { get; set; }
        public string  Cliente        { get; set; } = string.Empty;  // Aluno
        public decimal ValorTotal     { get; set; }
        public string  Data           { get; set; } = string.Empty;
        public string  FormaPagamento { get; set; } = string.Empty;
        public string  Status         { get; set; } = "Ativa";       // Ativa | Cancelada

        // Itens em memória (não persistidos separado — simplificado para o fluxo)
        public List<ItemVenda> Itens  { get; set; } = new();
    }

    /// <summary>
    /// Um produto dentro de uma venda.
    /// </summary>
    public class ItemVenda
    {
        public int     ProdutoId   { get; set; }
        public string  NomeProduto { get; set; } = string.Empty;
        public int     Quantidade  { get; set; }
        public decimal ValorUnit   { get; set; }
        public decimal Desconto    { get; set; }
        public decimal ValorTotal  => (ValorUnit * Quantidade) - Desconto;

        // Lote de onde essa quantidade foi de fato debitada (FEFO).
        // Nullable: preenchido pelo LoteService.ConsumirFEFO no momento da venda.
        // Se a quantidade do carrinho for maior que um único lote,
        // o VendaService divide o item em múltiplas linhas de ItemVenda,
        // uma por lote consumido — por isso aqui é sempre 1:1 com Lotes.
        public int?    LoteId      { get; set; }
    }
}
