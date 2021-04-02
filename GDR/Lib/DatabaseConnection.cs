using FirebirdSql.Data.FirebirdClient;
using GDR.Models;
using GDR.Repositories;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GDR.Lib
{

    public interface IDatabaseConnection
    {
        Task<List<ReportResult>> Get(Report report, List<DatabaseGDR> databaseGDRList, string filtro);
    }
    public class DatabaseConnection : IDatabaseConnection
    {

        private readonly ILogRepository log;

        public DatabaseConnection(ILogRepository log)
        {
            this.log = log;
        }

        public async Task<List<ReportResult>> Get(Report report, List<DatabaseGDR> databaseGDRList, string filtro)
        {

            return await GetReportResult(databaseGDRList, report, filtro);

        }


        private async Task<List<ReportResult>> GetReportResult(List<DatabaseGDR> databaseGDRList, Report report, string filtro)
        {

            List<ReportResult> reportResult = new List<ReportResult>();
            bool header = true;

            foreach (var database in databaseGDRList)
            {
                reportResult.AddRange(await GetDatabaseResult(database, report, header, filtro));
                header = false;
            }

            if (reportResult.Count == 1) {
                ReportResult reportLine = new ReportResult();
                reportLine.listaColuna.Add($"Este relatório não retornou dados!");
                reportResult.Add(reportLine);
            }

            return reportResult;
        }

        private async Task<List<ReportResult>> GetDatabaseResult(DatabaseGDR databaseGDR, Report report, bool header, string filtro)
        {

            String connectionString;
            dynamic connection;
            dynamic cmd;

            string databaseUser = Cipher.Decrypt(databaseGDR.DatabaseUser, databaseGDR.ChangeDate.ToString());
            string databasePassword = Cipher.Decrypt(databaseGDR.DatabasePassword, databaseGDR.ChangeDate.ToString());


            switch (databaseGDR.DatabaseTypeId)
            {

                //MySQL/MariaDB
                case 1:
                    connection = new MySqlConnection();
                    cmd = new MySqlCommand();
                    connectionString = $"server={databaseGDR.DatabaseServer};uid={databaseUser};Pwd={databasePassword};Database={databaseGDR.DatabaseName};Port={databaseGDR.Port}";
                    return await MultiDatabaseConnection(report.SQL, connection, cmd, connectionString, header, filtro);

                //Oracle
                case 2:
                    connection = new OracleConnection();
                    cmd = new OracleCommand();
                    connectionString = $"User Id={databaseUser};Password={databasePassword};Data Source={databaseGDR.DatabaseServer}:{databaseGDR.Port}/{databaseGDR.DatabaseName}";
                    return await MultiDatabaseConnection(report.SQL, connection, cmd, connectionString, header, filtro);

                //PostgreSQL
                case 3:
                    connection = new NpgsqlConnection();
                    cmd = new NpgsqlCommand();
                    connectionString = $"Host={databaseGDR.DatabaseServer};Username={databaseUser};Password={databasePassword};Database={databaseGDR.DatabaseName};Port={databaseGDR.Port}";
                    return await MultiDatabaseConnection(report.SQL, connection, cmd, connectionString, header, filtro);

                //SQL Server
                case 4:
                    connection = new SqlConnection();
                    cmd = new SqlCommand();
                    connectionString = $"Server={databaseGDR.DatabaseServer},{databaseGDR.Port};User Id={databaseUser};Password={databasePassword};Initial Catalog={databaseGDR.DatabaseName}";
                    return await MultiDatabaseConnection(report.SQL, connection, cmd, connectionString, header, filtro);

                //Firebird
                case 5:
                    connection = new FbConnection();
                    cmd = new FbCommand();
                    connectionString = $"User={databaseUser};Password={databasePassword};Database={databaseGDR.DatabaseName};DataSource={databaseGDR.DatabaseServer};Port={databaseGDR.Port}";
                    return await MultiDatabaseConnection(report.SQL, connection, cmd, connectionString, header, filtro);

                //OpenEdge
                //Este driver usa o campo descrição para complementar a string odbc
                case 6:
                    connection = new OdbcConnection();
                    cmd = new OdbcCommand();
                    connectionString = databaseGDR.ConnectionString;
                    connectionString = connectionString.Replace("Nome do servidor", databaseGDR.DatabaseServer);
                    connectionString = connectionString.Replace("Porta", databaseGDR.Port.ToString());
                    connectionString = connectionString.Replace("Nome do banco", databaseGDR.DatabaseName);
                    connectionString = connectionString.Replace("Nome do usuário", databaseUser);
                    connectionString = connectionString.Replace("Senha", databasePassword);

                    return await MultiDatabaseConnection(report.SQL, connection, cmd, connectionString, header, filtro);
            }

            return null;
        }

        private async Task<List<ReportResult>> MultiDatabaseConnection(string sql, dynamic connection, dynamic cmd, string connectionString, bool header, string filtro)
        {

            List<ReportResult> reportResultList = new List<ReportResult>();

            try
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                cmd.Connection = connection;
                cmd.CommandText = sql;
                cmd.CommandType = System.Data.CommandType.Text;
                bool incluir = false;
                string valor;

                using (var reader = await cmd.ExecuteReaderAsync())
                {

                    //Salva cabeçalho
                    bool hasRows = reader.Read();


                    if (header)
                    {
                        ReportResult reportColumnTop = new ReportResult();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            reportColumnTop.listaColuna.Add(reader.GetName(i));
                        }
                        reportResultList.Add(reportColumnTop);
                    }

                    //Se não tiver dados retorna somente o cabeçalho
                    if (!hasRows)
                    {
                        return reportResultList;
                    }

                    do
                    {
                        ReportResult reportLine = new ReportResult();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            valor = reader[i].ToString();

                            if (!string.IsNullOrEmpty(filtro))
                            {
                                if (valor.Contains(filtro))
                                {
                                    incluir = true;
                                }
                            }
                            reportLine.listaColuna.Add(valor);
                            
                        }
                        if (incluir || string.IsNullOrEmpty(filtro)) {
                            reportResultList.Add(reportLine);
                            incluir = false;
                        }


                    } while (reader.Read());

                }

            }

            catch
            {
                throw;
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return reportResultList;
        }
    }
}



