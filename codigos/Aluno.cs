namespace PotirendabaApp.Models
{
    public class Aluno
    {
        public int    Id       { get; set; }
        public string Nome     { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Data     { get; set; } = string.Empty;
        public string Sala     { get; set; } = string.Empty;
        public bool   Ativo    { get; set; } = true;
    }
}
