using GDR.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GDR.Repositories
{

    public interface IPermissionGroupRepository
    {
        Task<List<PermissionGroup>> GetList();
        Task<bool> Create(PermissionGroup permissionGroup);
        Task<PermissionGroup> Get(int? id);
        Task<PermissionGroup> Update(PermissionGroup permissionGroup, int[] ReportIdSelected);
        Task<bool> Delete(int? id);
        SelectList GetSelectAccessTypeList(AccessTypeEnum? type);
    }
    public class PermissionGroupRepository : GDRRepository<PermissionGroup>, IPermissionGroupRepository
    {

        private readonly string ControllerName = "PermissionGroup";
        private readonly ILogRepository log;

        public PermissionGroupRepository(GDRContext context, ILogRepository log) : base(context)
        {
            this.log = log;
        }

        public async Task<bool> Create(PermissionGroup permissionGroup)
        {
            try
            {

                await dbSet.AddAsync(permissionGroup);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Criado grupo de permissão {permissionGroup.GroupName}.");
                return true;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao criar grupo de permissão com o nome: {permissionGroup.GroupName}" + error.ToString());
                return false;
            }
        }

        public async Task<bool> Delete(int? id)
        {
            var permissionGroup = await this.Get(id);

            if (permissionGroup == null)
                return false;

            try
            {
                dbSet.Remove(permissionGroup);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Remoção do grupo de permissão com nome {permissionGroup.GroupName}.");

                return true;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao remover grupo de permissão com o ID: {permissionGroup.Id}" + error.ToString());

                return false;
            }
        }

        public async Task<PermissionGroup> Get(int? id)
        {
            if (id == null)
                return null;

            try
            {
                var permissionGroup = await dbSet
                    .Include(x => x.ReportPermissionGroup)
                    .ThenInclude(x => x.Report)
                    .Where(x => x.Id == id).FirstOrDefaultAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Pesquisa do grupo de permissão com nome {permissionGroup.GroupName}.");

                return permissionGroup;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao pesquisar grupo de permissão com o ID: {id}" + error.ToString());
                return null;
            }
        }

        public async Task<List<PermissionGroup>> GetList()
        {
            try
            {
                var permissionGroup = dbSet
                    .Include(x => x.ReportPermissionGroup)
                    .ThenInclude(x => x.Report)
                    .AsQueryable();


                await log.SaveLogApplicationMessage(ControllerName, "Lista de grupo de permissão retornada");
                return await permissionGroup.OrderBy(x => x.GroupName).ToListAsync();
            }

            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, "Erro ao buscar grupo de permissão de dados: " + error.ToString());
                return null;
            }
        }

        public async Task<List<PermissionGroup>> GetList(int reportId)
        {
            try
            {
                var permissionGroup = dbSet
                    .Include(x => x.ReportPermissionGroup)
                    .ThenInclude(x => x.Report)
                    .AsQueryable();

                IQueryable<int> reportList = context.ReportPermissionGroup.Where(x => x.ReportID == reportId).Select(x => x.PermissionGroupId);
                permissionGroup = permissionGroup.Where(x => reportList.Contains(x.Id));
                return await permissionGroup.OrderBy(x => x.GroupName).ToListAsync();
            }

            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, "Erro ao buscar lista de bancos de dados: " + error.ToString());
                return null;
            }
        }


        public async Task<PermissionGroup> Update(PermissionGroup permissionGroup, int[] ReportIdSelected)
        {
            try
            {

                var reportsRemove = context.ReportPermissionGroup.Where(x => x.PermissionGroupId == permissionGroup.Id);
                context.RemoveRange(reportsRemove);


                List<ReportPermissionGroup> reportPermissionGroupList = new List<ReportPermissionGroup>();

                foreach (var reportId in ReportIdSelected)
                {
                    ReportPermissionGroup reportPermissionGroup = new ReportPermissionGroup();

                    reportPermissionGroup.ReportID = reportId;
                    reportPermissionGroupList.Add(reportPermissionGroup);
                }

                permissionGroup.ReportPermissionGroup = reportPermissionGroupList;

                dbSet.Update(permissionGroup);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Atualização do grupo de permissão com nome {permissionGroup.GroupName}.");

                return permissionGroup;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao atualizar grupo de permissão com o ID: {permissionGroup.Id}" + error.ToString());
                return null;
            }
        }

        public SelectList GetSelectAccessTypeList(AccessTypeEnum? type)
        {
            SelectList select = new SelectList(new[] {
                new { ID = AccessTypeEnum.Administration, Name = "Administração da ferramenta" },
                new { ID = AccessTypeEnum.DatabaseAdministration, Name = "Administração de banco de dados" },
                new { ID = AccessTypeEnum.ReportsAdministration, Name = "Administração de relatórios" },
                new { ID = AccessTypeEnum.Read, Name = "Leitura de relatórios" },
            }, "ID", "Name", type);

            return select;
        }
    }
}
