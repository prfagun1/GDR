using GDR.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GDR.Repositories
{

    public interface ILdapRepository
    {
        Task<Ldap> Get();
        Task<bool> Create(Ldap ldap);
        Task<Ldap> Update(Ldap ldap);
    }
    public class LdapRepository : GDRRepository<Ldap>, ILdapRepository
    {
        private readonly ILogRepository log;
        private readonly string ControllerName = "Ldap";

        public LdapRepository(GDRContext context, ILogRepository log) : base(context)
        {
            this.log = log;
        }

        public async Task<Ldap> Get()
        {
            var ldap = await dbSet.FirstOrDefaultAsync();

            if (ldap == null)
            {
                await log.SaveLogApplicationMessage(ControllerName, $"Parâmetros LDAP não encontrados.");
                return null;
            }
            else
            {
                await log.SaveLogApplicationMessage(ControllerName, $"Pesquisa de parâmetros LDAP.");
                return ldap;
            }
        }

        public async Task<bool> Create(Ldap ldap)
        {
            try
            {
                ldap.BindLogin = Lib.Cipher.Encrypt(ldap.BindLogin, ldap.ChangeDate.ToString());
                ldap.BindPassword = Lib.Cipher.Encrypt(ldap.BindPassword, ldap.ChangeDate.ToString());

                await dbSet.AddAsync(ldap);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Cadastro de parâmetros LDAP realizado.");
                return true;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao cadastrar parâmetros LDAP. " + error.ToString());
                return false;
            }
        }

        public async Task<Ldap> Update(Ldap ldap)
        {
            try
            {
                ldap.BindLogin = Lib.Cipher.Encrypt(ldap.BindLogin, ldap.ChangeDate.ToString());
                ldap.BindPassword = Lib.Cipher.Encrypt(ldap.BindPassword, ldap.ChangeDate.ToString());

                dbSet.Update(ldap);
                await context.SaveChangesAsync();

                await log.SaveLogApplicationMessage(ControllerName, $"Atualização de parâmetros LDAP.");

                return ldap;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(ControllerName, $"Erro ao atualizar parâmetros LDAP. " + error.ToString());
                return null;
            }
        }
    }
}
