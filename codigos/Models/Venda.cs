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
    }
}
