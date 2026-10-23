using System;

namespace PotirendabaApp.Models
{
    /// <summary>
    /// Representa um lote de entrada de um produto: uma remessa com
    /// quantidade e data de validade próprias. Um Produto pode ter
    /// vários Lotes ativos ao mesmo tempo (relação 1:N).
    /// </summary>
    public class Lote
    {
        public int     Id           { get; set; }
        public int     ProdutoId    { get; set; }
        public string  NomeProduto  { get; set; } = string.Empty; // preenchido em joins, não persistido
        public int     Quantidade   { get; set; }
        public string  DataValidade { get; set; } = string.Empty; // ISO 8601 (yyyy-MM-dd)
        public string  DataEntrada  { get; set; } = string.Empty; // ISO 8601 (yyyy-MM-dd)
        public bool    Ativo        { get; set; } = true;

        /// <summary>Dias restantes até o vencimento (negativo = já vencido).</summary>
        public int DiasParaVencer
        {
            get
            {
                if (!DateTime.TryParse(DataValidade, out var validade)) return int.MaxValue;
                return (validade.Date - DateTime.Now.Date).Days;
            }
        }
    }
}
