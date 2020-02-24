using GDR.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GDR.Repositories
{

    public interface IDatabaseTypeRepository
    {
        Task<List<DatabaseType>> GetList(EnableEnum enable);
        Task<bool> Create(DatabaseType databaseType);
        Task<DatabaseType> Get(int? id);
        Task<DatabaseType> Update(DatabaseType databaseType);
        SelectList GetSelectList(EnableEnum? status);
        Task<bool> Delete(int? id);

    }
    public class DatabaseTypeRepository : GDRRepository<DatabaseType>, IDatabaseTypeRepository
    {
        private readonly string ControllerName = "DatabaseType";
        private readonly ILogRepository log;
        public DatabaseTypeRepository(GDRContext context, ILogRepository log) : base(context)
        {
            this.log = log;
        }

        public async Task<List<DatabaseType>> GetList(EnableEnum status)
        {

            try
            {
                var databaseType = dbSet.OrderBy(x => x.Name).AsQueryable();

                if (status != EnableEnum.All)
                {
                    databaseType = databaseType.Where(x => x.Enable == status);
                }


                await log.SaveLogApplicationMessage(ControllerName, "Lista de tipos de bancos de dados retornada");
                return await databaseType.ToListAsync();
            }

            catch(Exception error) {
                await log.SaveLogApplicationError(ControllerName, "Erro ao buscar lista de tipos de bancos de dados: " + error.ToString());
                return null;
            }
        }

        public async Task<bool> Create(DatabaseType databaseType) {

            try
            {
                databaseType.Enable = EnableEnum.Enabled;

                await dbSet.AddAsync(databaseType);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Criado tipo de banco de dados {databaseType.Name}.");
                return true;
            }
            catch(Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao criar tipo de bancos de dados com o nome: {databaseType.Name}" + error.ToString());
                return false;
            }
        }

        public async Task<DatabaseType> Get(int? id) {

            if (id == null)
                return null;

            try
            {
                DatabaseType databaseType = await dbSet.Where(x => x.Id == id).FirstOrDefaultAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Pesquisa do tipo de banco de dados com nome {databaseType.Name}.");

                return databaseType;
            }
            catch (Exception error)
            { 
                await log.SaveLogApplicationError(ControllerName, $"Erro ao pesquisar tipo de bancos de dados com o ID: {id}" + error.ToString());
                return null;
            }
        }

        public async Task<DatabaseType> Update(DatabaseType databaseType)
        {
            try
            {
                dbSet.Update(databaseType);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Atualização do tipo de banco de dados com nome {databaseType.Name}.");

                return databaseType;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao atualizar tipo de bancos de dados com o ID: {databaseType.Id}" + error.ToString());
                return null;
            }
        }

        public async Task<bool> Delete(int? id)
        {
            DatabaseType databaseType = await this.Get(id);

            if (databaseType == null)
                return false;

            try
            {
                dbSet.Remove(databaseType);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Remoção do tipo de banco de dados com nome {databaseType.Name}.");

                return true;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao remover tipo de bancos de dados com o ID: {databaseType.Id}" + error.ToString());

                return false;
            }
        }

        public SelectList GetSelectList(EnableEnum? enable) {
            SelectList enableSelect = new SelectList(new[] {
                new { ID = EnableEnum.Enabled, Name = "Sim" },
                new { ID = EnableEnum.Disabled, Name = "Não" },
            }, "ID", "Name", enable);

            return enableSelect;
        }

    }
}
