using GDR.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GDR.Repositories
{
    public interface IReportRepository
    {
        Task<List<Report>> GetList(EnableEnum enable);
        Task<bool> Create(Report report);
        Task<Report> Get(int? id);
        Task<Report> Update(Report report, int[] DatabaseIDSelected, int[] PermissionGroupIdSelected);
        SelectList GetSelectList(EnableEnum? status);
        Task<bool> Delete(int? id);
        Task<List<Report>> GetList(List<int> permissionGroups);
        Task<int> CountAsync();
    }
    public class ReportRepository : GDRRepository<Report>, IReportRepository
    {
        private readonly string ControllerName = "Reports";
        private readonly ILogRepository log;

        public ReportRepository(GDRContext context, ILogRepository log) : base(context)
        {
            this.log = log;
        }

        public async Task<int> CountAsync()
        {
            return await dbSet.CountAsync();
        }

        public async Task<bool> Create(Report report)
        {
            try
            {
                report.Enable = EnableEnum.Enabled;

                await dbSet.AddAsync(report);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Criado relatório {report.Name}.");
                return true;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao criar relatório com o nome: {report.Name}" + error.ToString());
                return false;
            }
        }

        public async Task<bool> Delete(int? id)
        {
            var report = await this.Get(id);

            if (report == null)
                return false;

            try
            {
                dbSet.Remove(report);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Remoção do relatório com nome {report.Name}.");

                return true;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao remover relatório com o ID: {report.Id}" + error.ToString());

                return false;
            }
        }

        public async Task<Report> Get(int? id)
        {
            if (id == null)
                return null;

            try
            {
                var report = await dbSet
                    .Include(x => x.ReportDatabaseGDR)
                    .ThenInclude(x => x.DatabaseGDR)
                    .Include(x => x.ReportPermissionGroup)
                    .ThenInclude(x => x.PermissionGroup)
                    .Where(x => x.Id == id).FirstOrDefaultAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Pesquisa do relatório com nome {report.Name}.");

                return report;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao pesquisar relatório com o ID: {id}" + error.ToString());
                return null;
            }
        }

        public async Task<List<Report>> GetList(EnableEnum status)
        {

            var teste = dbSet.ToList();

            try
            {
                var report = dbSet
                    .Include(x => x.ReportDatabaseGDR)
                    .ThenInclude(x => x.DatabaseGDR)
                    .Include(x => x.ReportPermissionGroup)
                    .ThenInclude(x => x.PermissionGroup)
                    .AsQueryable();



                if (status != EnableEnum.All)
                {
                    report = report.Where(x => x.Enable == status);
                }


                await log.SaveLogApplicationMessage(ControllerName, "Lista de relatórios retornada");
                return await report.OrderBy(x => x.Name).ToListAsync();
            }

            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, "Erro ao buscar relatórios de dados: " + error.ToString());
                return null;
            }
        }


        public async Task<List<Report>> GetList(List<int> permissionGroups)
        {

            var teste = dbSet.ToList();

            try
            {
                var report = dbSet
                    .Include(x => x.ReportDatabaseGDR)
                    .ThenInclude(x => x.DatabaseGDR)
                    .Include(x => x.ReportPermissionGroup)
                    .ThenInclude(x => x.PermissionGroup)
                    .AsQueryable();




                var reportIdList =  context.ReportPermissionGroup.Where(x => permissionGroups.Contains(x.PermissionGroupId)).Select(x => x.ReportID);

                report = report.Where(x => x.Enable == EnableEnum.Enabled).Where(x => reportIdList.Contains(x.Id));
  

     
                return await report.OrderBy(x => x.Name).ToListAsync();
            }

            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, "Erro ao buscar lista de relatórios: " + error.ToString());
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

        public async Task<Report> Update(Report report, int[] DatabaseIDSelected, int[] PermissionGroupIdSelected)
        {
            try
            {
                var databaseGDRRemove = context.ReportDatabaseGDR.Where(x => x.ReportID == report.Id);
                var permissionGroupRemove = context.ReportPermissionGroup.Where(x => x.ReportID == report.Id);
                context.RemoveRange(databaseGDRRemove);
                context.RemoveRange(permissionGroupRemove);


                List<ReportDatabaseGDR> reportDatabaseGDR = new List<ReportDatabaseGDR>();
                foreach (var databaseId in DatabaseIDSelected)
                {
                    ReportDatabaseGDR databaseGDR = new ReportDatabaseGDR();
                    databaseGDR.DatabaseGDRId = databaseId;
                    reportDatabaseGDR.Add(databaseGDR);
                }
                report.ReportDatabaseGDR = reportDatabaseGDR;


                List<ReportPermissionGroup> reportPermissionGroup = new List<ReportPermissionGroup>();
                foreach (var permissionGroupId in PermissionGroupIdSelected)
                {
                    ReportPermissionGroup permissionGroup = new ReportPermissionGroup();
                    permissionGroup.PermissionGroupId = permissionGroupId;
                    reportPermissionGroup.Add(permissionGroup);

                }
                report.ReportPermissionGroup = reportPermissionGroup;



                dbSet.Update(report);

                await context.SaveChangesAsync();
                await log.SaveLogApplicationMessage(ControllerName, $"Atualização do relatório com nome {report.Name}.");

                return report;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao atualizar de relatório com o ID: {report.Id}" + error.ToString());
                return null;
            }
        }
    }
}

