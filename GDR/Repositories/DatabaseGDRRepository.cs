using GDR.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GDR.Repositories
{

    public interface IDatabaseGDRRepository
    {
        Task<List<DatabaseGDR>> GetList(EnableEnum enable);
        Task<List<DatabaseGDR>> GetList(int reportId);
        Task<List<DatabaseGDR>> GetList(int?[] databasesId);
        Task<bool> Create(DatabaseGDR databaseType);
        Task<DatabaseGDR> Get(int? id);
        Task<DatabaseGDR> Update(DatabaseGDR databaseGDR, int[] ReportIdSelected);
        SelectList GetSelectList(EnableEnum? status);
        Task<bool> Delete(int? id);
        Task<int> CountAsync();
    }
    public class DatabaseGDRRepository : GDRRepository<DatabaseGDR>, IDatabaseGDRRepository
    {
        private readonly string ControllerName = "DatabaseGDR";
        private readonly ILogRepository log;
        public DatabaseGDRRepository(GDRContext context, ILogRepository log) : base(context)
        {
            this.log = log;
        }

        public async Task<int> CountAsync()
        {
            return await dbSet.CountAsync();
        }

        public async Task<bool> Create(DatabaseGDR databaseGDR)
        {
            try
            {
                databaseGDR.Enable = EnableEnum.Enabled;

                databaseGDR.DatabasePassword = Lib.Cipher.Encrypt(databaseGDR.DatabasePassword, databaseGDR.ChangeDate.ToString());
                databaseGDR.DatabaseUser = Lib.Cipher.Encrypt(databaseGDR.DatabaseUser, databaseGDR.ChangeDate.ToString());


                await dbSet.AddAsync(databaseGDR);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Criado banco de dados {databaseGDR.Name}.");
                return true;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao criar bancos de dados com o nome: {databaseGDR.Name}" + error.ToString());
                return false;
            }
        }

        public async Task<bool> Delete(int? id)
        {
            var databaseGDR = await this.Get(id);

            if (databaseGDR == null)
                return false;

            try
            {
                dbSet.Remove(databaseGDR);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Remoção do banco de dados com nome {databaseGDR.Name}.");

                return true;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao remover bancos de dados com o ID: {databaseGDR.Id}" + error.ToString());

                return false;
            }
        }

        public async Task<DatabaseGDR> Get(int? id)
        {
            if (id == null)
                return null;

            try
            {
                var databaseGDR = await dbSet
                    .Include(x => x.ReportDatabaseGDR)
                    .ThenInclude(x => x.Report)
                    .Where(x => x.Id == id).FirstOrDefaultAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Pesquisa do banco de dados com nome {databaseGDR.Name}.");

                return databaseGDR;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao pesquisar bancos de dados com o ID: {id}" + error.ToString());
                return null;
            }
        }

        public async Task<List<DatabaseGDR>> GetList(EnableEnum status)
        {
            try
            {
                var databaseGDRList = dbSet
                    .Include(y => y.DatabaseType)
                    .Include(x => x.ReportDatabaseGDR)
                    .ThenInclude(x => x.Report)
                    .AsQueryable();

                

                if (status != EnableEnum.All)
                {
                    databaseGDRList = databaseGDRList.Where(x => x.Enable == status);
                }



                await log.SaveLogApplicationMessage(ControllerName, "Lista de bancos de dados retornada");
                return await databaseGDRList.OrderBy(x => x.Name).ToListAsync();
            }

            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, "Erro ao buscar lista de bancos de dados: " + error.ToString());
                return null;
            }
        }

        public async Task<List<DatabaseGDR>> GetList(int reportId)
        {
            try
            {
                var databaseGDR = dbSet
                    .Include(x => x.ReportDatabaseGDR)
                    .ThenInclude(x => x.Report)
                    .Include(x => x.DatabaseType)
                    .AsQueryable();

                IQueryable<int> databaseIdList = context.ReportDatabaseGDR.Where(x => x.ReportID == reportId).Select(x => x.DatabaseGDRId);
                databaseGDR = databaseGDR.Where(x => x.Enable == EnableEnum.Enabled).Where(x => databaseIdList.Contains(x.Id));
                return await databaseGDR.OrderBy(x => x.Name).ToListAsync();
            }

            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, "Erro ao buscar lista de bancos de dados: " + error.ToString());
                return null;
            }
        }

        public async Task<List<DatabaseGDR>> GetList(int?[] databasesId)
        {

            if (databasesId == null) return null;

            try
            {
                var databaseGDR = dbSet
                    .Include(x => x.ReportDatabaseGDR)
                    .ThenInclude(x => x.Report)
                    .Include(x => x.DatabaseType)
                    .AsQueryable();

                databaseGDR = databaseGDR.Where(x => x.Enable == EnableEnum.Enabled).Where(x => databasesId.Contains(x.Id));
                return await databaseGDR.OrderBy(x => x.Name).ToListAsync();
            }

            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, "Erro ao buscar lista de bancos de dados: " + error.ToString());
                return null;
            }
        }

        public SelectList GetSelectList(EnableEnum? status)
        {
            SelectList enableSelect = new SelectList(new[] {
                new { ID = EnableEnum.Enabled, Name = "Sim" },
                new { ID = EnableEnum.Disabled, Name = "Não" },
            }, "ID", "Name", status);

            return enableSelect;
        }

        public async Task<DatabaseGDR> Update(DatabaseGDR databaseGDR, int[] ReportIdSelected)
        {
            try
            {

                var reportsRemove = context.ReportDatabaseGDR.Where(x => x.DatabaseGDRId == databaseGDR.Id);
                context.RemoveRange(reportsRemove);


                List<ReportDatabaseGDR> reportDatabaseGDRList = new List<ReportDatabaseGDR>();

                foreach (var reportId in ReportIdSelected)
                {
                    ReportDatabaseGDR reportDatabaseGDR = new ReportDatabaseGDR();

                    reportDatabaseGDR.ReportID = reportId;
                    reportDatabaseGDRList.Add(reportDatabaseGDR);
                }

                databaseGDR.ReportDatabaseGDR = reportDatabaseGDRList;


                databaseGDR.DatabasePassword = Lib.Cipher.Encrypt(databaseGDR.DatabasePassword, databaseGDR.ChangeDate.ToString());
                databaseGDR.DatabaseUser = Lib.Cipher.Encrypt(databaseGDR.DatabaseUser, databaseGDR.ChangeDate.ToString());

                dbSet.Update(databaseGDR);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Atualização do de banco de dados com nome {databaseGDR.Name}.");

                return databaseGDR;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao atualizar de bancos de dados com o ID: {databaseGDR.Id}" + error.ToString());
                return null;
            }
        }
    }
}

