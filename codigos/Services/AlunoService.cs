using System.Collections.Generic;
using PotirendabaApp.Data;
using PotirendabaApp.Models;
using static PotirendabaApp.Data.DatabaseHelper;

namespace PotirendabaApp.Services
{
    public static class AlunoService
    {
        public static List<Aluno> Listar(string filtro = "",
            bool porNome = true, bool porCodigo = false,
            FiltroStatus status = FiltroStatus.Ativos) =>
            DatabaseHelper.ListarAlunos(filtro, porNome, porCodigo, porNome, status);
    }
}
