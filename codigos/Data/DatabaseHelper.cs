using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using PotirendabaApp.Models;
using PotirendabaApp.Services;

namespace PotirendabaApp.Data
{
    public static class DatabaseHelper
    {
        private static readonly string _conn = "Data Source=potirendaba.db;";

        // ════════════════════════════════════════════════════════════════════
        //  INICIALIZAÇÃO
        // ════════════════════════════════════════════════════════════════════
        public static void InicializarBanco()
        {
            using var con = Abrir();

            // Tabelas principais
            Run(con, @"CREATE TABLE IF NOT EXISTS Alunos (
                Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome     TEXT    NOT NULL,
                Telefone TEXT,
                Data     TEXT,
                Sala     TEXT,
                Ativo    INTEGER DEFAULT 1);");

            Run(con, @"CREATE TABLE IF NOT EXISTS Produtos (
                Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome    TEXT NOT NULL,
                Data    TEXT,
                Valor   REAL,
                Estoque INTEGER,
                Ativo   INTEGER DEFAULT 1);");

            Run(con, @"CREATE TABLE IF NOT EXISTS Vendas (
                Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                Cliente        TEXT,
                ValorTotal     REAL,
                Data           TEXT,
                FormaPagamento TEXT,
                Status         TEXT DEFAULT 'Ativa');");

            Run(con, @"CREATE TABLE IF NOT EXISTS ItensVenda (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                VendaId     INTEGER NOT NULL,
                ProdutoId   INTEGER,
                NomeProduto TEXT,
                Quantidade  INTEGER,
                ValorUnit   REAL,
                Desconto    REAL,
                FOREIGN KEY(VendaId) REFERENCES Vendas(Id));");

            // Migração segura: adiciona coluna Ativo se o banco já existia sem ela
            MigrarColuna(con, "Alunos",   "Ativo", "INTEGER DEFAULT 1");
            MigrarColuna(con, "Produtos", "Ativo", "INTEGER DEFAULT 1");
        }

        /// <summary>Adiciona coluna se ainda não existir (migração não destrutiva).</summary>
        private static void MigrarColuna(SqliteConnection con, string tabela, string coluna, string tipo)
        {
            try
            {
                Run(con, $"ALTER TABLE {tabela} ADD COLUMN {coluna} {tipo};");
            }
            catch { /* coluna já existe — ignorar */ }
        }

        private static SqliteConnection Abrir()
        {
            var con = new SqliteConnection(_conn);
            con.Open();
            return con;
        }

        private static void Run(SqliteConnection con, string sql)
        {
            using var cmd = new SqliteCommand(sql, con);
            cmd.ExecuteNonQuery();
        }

        // ════════════════════════════════════════════════════════════════════
        //  FILTRO STATUS (enum para Alunos e Produtos)
        // ════════════════════════════════════════════════════════════════════
        // FiltroStatus agora vive em PotirendabaApp.Services.FiltroStatus
        // Mantido como alias para não quebrar código existente
        public enum FiltroStatus { Todos, Ativos, Inativos }

        private static string ClausulaAtivo(PotirendabaApp.Services.FiltroStatus status, string where) => status switch
        {
            PotirendabaApp.Services.FiltroStatus.Ativos   => where.Length > 0 ? where + " AND COALESCE(Ativo,1)=1" : "WHERE COALESCE(Ativo,1)=1",
            PotirendabaApp.Services.FiltroStatus.Inativos => where.Length > 0 ? where + " AND COALESCE(Ativo,1)=0" : "WHERE COALESCE(Ativo,1)=0",
            _                     => where
        };

        private static string BuildWhere(string filtro, bool filtrarNome, bool filtrarCodigo)
        {
            if (string.IsNullOrWhiteSpace(filtro) || (!filtrarNome && !filtrarCodigo))
                return "";
            var partes = new List<string>();
            if (filtrarNome)   partes.Add("Nome LIKE @Filtro");
            if (filtrarCodigo) partes.Add("CAST(Id AS TEXT) LIKE @Filtro");
            return "WHERE " + string.Join(" OR ", partes);
        }

        // ════════════════════════════════════════════════════════════════════
        //  ALUNOS
        // ════════════════════════════════════════════════════════════════════
        public static int InserirAluno(Aluno aluno)
        {
            using var con = Abrir();
            using var cmd = new SqliteCommand(
                "INSERT INTO Alunos(Nome,Telefone,Data,Sala,Ativo) VALUES(@N,@T,@D,@S,@A); SELECT last_insert_rowid();", con);
            cmd.Parameters.AddWithValue("@N", aluno.Nome);
            cmd.Parameters.AddWithValue("@T", aluno.Telefone);
            cmd.Parameters.AddWithValue("@D", aluno.Data);
            cmd.Parameters.AddWithValue("@S", aluno.Sala);
            cmd.Parameters.AddWithValue("@A", aluno.Ativo ? 1 : 0);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static List<Aluno> ListarAlunos(
            string filtro = "", bool filtrarNome = false, bool filtrarCodigo = false,
            bool ordenarPorNome = false, PotirendabaApp.Services.FiltroStatus status = PotirendabaApp.Services.FiltroStatus.Ativos)
        {
            using var con = Abrir();
            string where = BuildWhere(filtro, filtrarNome, filtrarCodigo);
            where = ClausulaAtivo(status, where);
            string ord = ordenarPorNome ? "ORDER BY Nome" : "ORDER BY Id";
            using var cmd = new SqliteCommand(
                $"SELECT Id,Nome,Telefone,Data,Sala,Ativo FROM Alunos {where} {ord};", con);
            if (!string.IsNullOrWhiteSpace(filtro))
                cmd.Parameters.AddWithValue("@Filtro", $"%{filtro}%");
            var lista = new List<Aluno>();
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(new Aluno {
                Id=r.GetInt32(0), Nome=r.GetString(1),
                Telefone=r.IsDBNull(2)?"":r.GetString(2),
                Data=r.IsDBNull(3)?"":r.GetString(3),
                Sala=r.IsDBNull(4)?"":r.GetString(4),
                Ativo=r.IsDBNull(5) || r.GetInt32(5)==1 });
            return lista;
        }

        public static void AtualizarAluno(Aluno aluno)
        {
            using var con = Abrir();
            using var cmd = new SqliteCommand(
                "UPDATE Alunos SET Nome=@N,Telefone=@T,Data=@D,Sala=@S,Ativo=@A WHERE Id=@Id;", con);
            cmd.Parameters.AddWithValue("@Id", aluno.Id);
            cmd.Parameters.AddWithValue("@N",  aluno.Nome);
            cmd.Parameters.AddWithValue("@T",  aluno.Telefone);
            cmd.Parameters.AddWithValue("@D",  aluno.Data);
            cmd.Parameters.AddWithValue("@S",  aluno.Sala);
            cmd.Parameters.AddWithValue("@A",  aluno.Ativo ? 1 : 0);
            cmd.ExecuteNonQuery();
        }

        /// <summary>Alterna Ativo/Inativo sem apagar o registro.</summary>
        public static void AlternarStatusAluno(int id, bool ativo)
        {
            using var con = Abrir();
            using var cmd = new SqliteCommand("UPDATE Alunos SET Ativo=@A WHERE Id=@Id;", con);
            cmd.Parameters.AddWithValue("@A",  ativo ? 1 : 0);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        // ════════════════════════════════════════════════════════════════════
        //  PRODUTOS
        // ════════════════════════════════════════════════════════════════════
        public static int InserirProduto(Produto produto)
        {
            using var con = Abrir();
            using var cmd = new SqliteCommand(
                "INSERT INTO Produtos(Nome,Data,Valor,Estoque,Ativo) VALUES(@N,@D,@V,@E,@A); SELECT last_insert_rowid();", con);
            cmd.Parameters.AddWithValue("@N", produto.Nome);
            cmd.Parameters.AddWithValue("@D", produto.Data);
            cmd.Parameters.AddWithValue("@V", produto.Valor);
            cmd.Parameters.AddWithValue("@E", produto.Estoque);
            cmd.Parameters.AddWithValue("@A", produto.Ativo ? 1 : 0);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static List<Produto> ListarProdutos(
            string filtro = "", bool filtrarNome = false, bool filtrarCodigo = false,
            bool ordenarPorNome = false, PotirendabaApp.Services.FiltroStatus status = PotirendabaApp.Services.FiltroStatus.Ativos)
        {
            using var con = Abrir();
            string where = BuildWhere(filtro, filtrarNome, filtrarCodigo);
            where = ClausulaAtivo(status, where);
            string ord = ordenarPorNome ? "ORDER BY Nome" : "ORDER BY Id";
            using var cmd = new SqliteCommand(
                $"SELECT Id,Nome,Data,Valor,Estoque,Ativo FROM Produtos {where} {ord};", con);
            if (!string.IsNullOrWhiteSpace(filtro))
                cmd.Parameters.AddWithValue("@Filtro", $"%{filtro}%");
            var lista = new List<Produto>();
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(new Produto {
                Id=r.GetInt32(0), Nome=r.GetString(1),
                Data=r.IsDBNull(2)?"":r.GetString(2),
                Valor=r.IsDBNull(3)?0m:Convert.ToDecimal(r.GetDouble(3)),
                Estoque=r.IsDBNull(4)?0:r.GetInt32(4),
                Ativo=r.IsDBNull(5) || r.GetInt32(5)==1 });
            return lista;
        }

        public static void AtualizarProduto(Produto produto)
        {
            using var con = Abrir();
            using var cmd = new SqliteCommand(
                "UPDATE Produtos SET Nome=@N,Data=@D,Valor=@V,Estoque=@E,Ativo=@A WHERE Id=@Id;", con);
            cmd.Parameters.AddWithValue("@Id", produto.Id);
            cmd.Parameters.AddWithValue("@N",  produto.Nome);
            cmd.Parameters.AddWithValue("@D",  produto.Data);
            cmd.Parameters.AddWithValue("@V",  produto.Valor);
            cmd.Parameters.AddWithValue("@E",  produto.Estoque);
            cmd.Parameters.AddWithValue("@A",  produto.Ativo ? 1 : 0);
            cmd.ExecuteNonQuery();
        }

        public static void AlternarStatusProduto(int id, bool ativo)
        {
            using var con = Abrir();
            using var cmd = new SqliteCommand("UPDATE Produtos SET Ativo=@A WHERE Id=@Id;", con);
            cmd.Parameters.AddWithValue("@A",  ativo ? 1 : 0);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public static Produto BuscarProdutoPorId(int id)
        {
            using var con = Abrir();
            using var cmd = new SqliteCommand(
                "SELECT Id,Nome,Data,Valor,Estoque,Ativo FROM Produtos WHERE Id=@Id;", con);
            cmd.Parameters.AddWithValue("@Id", id);
            using var r = cmd.ExecuteReader();
            if (r.Read()) return new Produto {
                Id=r.GetInt32(0), Nome=r.GetString(1),
                Data=r.IsDBNull(2)?"":r.GetString(2),
                Valor=r.IsDBNull(3)?0m:Convert.ToDecimal(r.GetDouble(3)),
                Estoque=r.IsDBNull(4)?0:r.GetInt32(4),
                Ativo=!r.IsDBNull(5) && r.GetInt32(5)==1 };
            return null;
        }

        public static void ExcluirAluno(int id)   { using var con=Abrir(); using var cmd=new SqliteCommand("DELETE FROM Alunos WHERE Id=@Id;",con); cmd.Parameters.AddWithValue("@Id",id); cmd.ExecuteNonQuery(); }
        public static void ExcluirProduto(int id) { using var con=Abrir(); using var cmd=new SqliteCommand("DELETE FROM Produtos WHERE Id=@Id;",con); cmd.Parameters.AddWithValue("@Id",id); cmd.ExecuteNonQuery(); }

        // ════════════════════════════════════════════════════════════════════
        //  VENDAS
        // ════════════════════════════════════════════════════════════════════
        public static int InserirVenda(Venda venda)
        {
            using var con = Abrir();
            using var tx  = con.BeginTransaction();
            using var cmdV = new SqliteCommand(@"
                INSERT INTO Vendas(Cliente,ValorTotal,Data,FormaPagamento,Status)
                VALUES(@C,@VT,@D,@FP,@S); SELECT last_insert_rowid();", con, tx);
            cmdV.Parameters.AddWithValue("@C",  venda.Cliente);
            cmdV.Parameters.AddWithValue("@VT", venda.ValorTotal);
            cmdV.Parameters.AddWithValue("@D",  venda.Data);
            cmdV.Parameters.AddWithValue("@FP", venda.FormaPagamento);
            cmdV.Parameters.AddWithValue("@S",  venda.Status);
            int vendaId = Convert.ToInt32(cmdV.ExecuteScalar());
            foreach (var item in venda.Itens)
            {
                using var cmdI = new SqliteCommand(@"
                    INSERT INTO ItensVenda(VendaId,ProdutoId,NomeProduto,Quantidade,ValorUnit,Desconto)
                    VALUES(@VI,@PI,@NP,@QT,@VU,@DC);", con, tx);
                cmdI.Parameters.AddWithValue("@VI", vendaId);
                cmdI.Parameters.AddWithValue("@PI", item.ProdutoId);
                cmdI.Parameters.AddWithValue("@NP", item.NomeProduto);
                cmdI.Parameters.AddWithValue("@QT", item.Quantidade);
                cmdI.Parameters.AddWithValue("@VU", item.ValorUnit);
                cmdI.Parameters.AddWithValue("@DC", item.Desconto);
                cmdI.ExecuteNonQuery();
            }
            tx.Commit();
            return vendaId;
        }

        public static List<Venda> ListarVendas(string filtro="",
            bool filtrarNome=false, bool filtrarCodigo=false,
            bool filtrarData=false, bool filtrarValor=false)
        {
            using var con = Abrir();
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                if (filtrarNome)   partes.Add("Cliente LIKE @F");
                if (filtrarCodigo) partes.Add("CAST(Id AS TEXT) LIKE @F");
                if (filtrarData)   partes.Add("Data LIKE @F");
                if (filtrarValor)  partes.Add("CAST(ValorTotal AS TEXT) LIKE @F");
            }
            string where = partes.Count > 0 ? "WHERE " + string.Join(" OR ", partes) : "";
            using var cmd = new SqliteCommand(
                $"SELECT Id,Cliente,ValorTotal,Data,FormaPagamento,Status FROM Vendas {where} ORDER BY Id DESC;", con);
            if (partes.Count > 0) cmd.Parameters.AddWithValue("@F", $"%{filtro}%");
            var lista = new List<Venda>();
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(new Venda {
                Id=r.GetInt32(0), Cliente=r.IsDBNull(1)?"":r.GetString(1),
                ValorTotal=r.IsDBNull(2)?0m:Convert.ToDecimal(r.GetDouble(2)),
                Data=r.IsDBNull(3)?"":r.GetString(3),
                FormaPagamento=r.IsDBNull(4)?"":r.GetString(4),
                Status=r.IsDBNull(5)?"":r.GetString(5) });
            return lista;
        }

        public static Venda BuscarVendaComItens(int id)
        {
            using var con = Abrir();
            using var cmdV = new SqliteCommand(
                "SELECT Id,Cliente,ValorTotal,Data,FormaPagamento,Status FROM Vendas WHERE Id=@Id;", con);
            cmdV.Parameters.AddWithValue("@Id", id);
            Venda venda = null;
            using (var r = cmdV.ExecuteReader())
            {
                if (r.Read()) venda = new Venda {
                    Id=r.GetInt32(0), Cliente=r.IsDBNull(1)?"":r.GetString(1),
                    ValorTotal=r.IsDBNull(2)?0m:Convert.ToDecimal(r.GetDouble(2)),
                    Data=r.IsDBNull(3)?"":r.GetString(3),
                    FormaPagamento=r.IsDBNull(4)?"":r.GetString(4),
                    Status=r.IsDBNull(5)?"":r.GetString(5) };
            }
            if (venda == null) return null;
            using var cmdI = new SqliteCommand(
                "SELECT ProdutoId,NomeProduto,Quantidade,ValorUnit,Desconto FROM ItensVenda WHERE VendaId=@Vid;", con);
            cmdI.Parameters.AddWithValue("@Vid", id);
            using var ri = cmdI.ExecuteReader();
            while (ri.Read()) venda.Itens.Add(new ItemVenda {
                ProdutoId=ri.IsDBNull(0)?0:ri.GetInt32(0),
                NomeProduto=ri.IsDBNull(1)?"":ri.GetString(1),
                Quantidade=ri.IsDBNull(2)?1:ri.GetInt32(2),
                ValorUnit=ri.IsDBNull(3)?0m:Convert.ToDecimal(ri.GetDouble(3)),
                Desconto=ri.IsDBNull(4)?0m:Convert.ToDecimal(ri.GetDouble(4)) });
            return venda;
        }
    }
}
