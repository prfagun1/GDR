using GDR.Models;
using GDR.Models.ViewModels;
using GDR.Repositories;
using Microsoft.AspNetCore.Http;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace GDR.Lib
{

    public interface IAuthentication
    {
        Task<List<Claim>> VerifyAccess(LoginViewModel login);
        bool VerifyAdminUser(LoginViewModel login);
        List<Claim> GetClaimType(AccessTypeEnum Permission);
        List<int> GetPermissionGroups(IEnumerable<Claim> permissions);
    }


    public class Authentication : IAuthentication
    {

        private readonly IPermissionGroupRepository permissionGroupRepository;
        private readonly IParameterRepository parameterRepository;
        private readonly ILdapRepository ldapRepository;
        private readonly ILogRepository log;
         private readonly string controllerName = "Authentication";

        private Ldap ldap;
        private LdapConnection ldapConnection;
        public Authentication(ILdapRepository ldapRepository, IPermissionGroupRepository permissionGroupRepository,
            IParameterRepository parameterRepository, ILogRepository log)
        {
            this.ldapRepository = ldapRepository;
            this.permissionGroupRepository = permissionGroupRepository;
            this.parameterRepository = parameterRepository;
            this.log = log;
        }

        public List<int> GetPermissionGroups(IEnumerable<Claim> permissions)
        {
 
            List<int> permissionGroupList = new List<int>();

            foreach (Claim permission in permissions) {
                if (permission.Type == "Reports") {
                    permissionGroupList.Add(int.Parse(permission.Value));
                }
            }
            return permissionGroupList;
        }

        private async Task CreateLdapConnection()
        {
            
            ldap = await ldapRepository.Get();
            String ldapUser = Lib.Cipher.Decrypt(ldap.BindLogin, ldap.ChangeDate.ToString());
            String ldapPassword = Lib.Cipher.Decrypt(ldap.BindPassword, ldap.ChangeDate.ToString());

            ldapConnection = new LdapConnection();
            ldapConnection.Connect(ldap.Server, ldap.Port);
            ldapConnection.Bind(ldap.LdapVersion, ldapUser, ldapPassword);
        }


        public bool VerifyAdminUser(LoginViewModel login)
        {
            Parameter parameter = parameterRepository.Get();

            if (parameter == null) return false;

            if (login.Username == parameter.AdminUser && login.Password == login.Password)
            {
                return true;
            }

            return false;
        }

        public async Task<List<Claim>> VerifyAccess(LoginViewModel login)
        {
            await CreateLdapConnection();

            String userDN = await GetUserDN(login.Username);
            if (userDN == null) return null;

            Boolean userVerifyPassword = await UserVerifyPassword(login, userDN);

            if (userVerifyPassword)
            {
                List<Claim> claims = await UserVerifyGroup(userDN);
                ldapConnection.Disconnect();
                return claims;
            }

            ldapConnection.Disconnect();

            return null;
        }

        public List<Claim> GetClaimType(AccessTypeEnum Permission)
        {
            List<Claim> claims = new List<Claim>();

            switch (Permission)
            {
                case AccessTypeEnum.Administration:
                    claims.Add(new Claim("Administration", "true"));
                    claims.Add(new Claim("DatabaseAdministration", "true"));
                    claims.Add(new Claim("ReportsAdministration", "true"));
                    claims.Add(new Claim("Read", "true"));
                    break;
                case AccessTypeEnum.DatabaseAdministration:
                    claims.Add(new Claim("DatabaseAdministration", "true"));
                    claims.Add(new Claim("Read", "true"));
                    break;
                case AccessTypeEnum.ReportsAdministration:
                    claims.Add(new Claim("ReportsAdministration", "true"));
                    claims.Add(new Claim("Read", "true"));
                    break;
                case AccessTypeEnum.Read:
                    claims.Add(new Claim("Read", "true"));
                    break;

            }

            return claims;
        }


        private async Task<List<Claim>> UserVerifyGroup(String userDN)
        {


            LdapSearchConstraints cons = new LdapSearchConstraints();
            String[] atributos = new String[] { "member" };

            List<Claim> claims = new List<Claim>();
            var permissionGroups = await permissionGroupRepository.GetList();

            try
            {
                foreach (PermissionGroup group in permissionGroups)
                {
                    String groupDN = await GetGroupDN(group.GroupName);
                    LdapSearchResults searchResults = ldapConnection.Search(groupDN, LdapConnection.SCOPE_BASE, null, atributos, false, cons);

                    var nextEntry = searchResults.Next();
                    nextEntry.getAttributeSet();

                    try
                    {
                        if (nextEntry.getAttribute("member").StringValueArray.Where(x => x == userDN).Count() > 0)
                        {
                            claims.AddRange(GetClaimType(group.AccessType));
                            claims.Add(new Claim("Reports", group.Id.ToString()));
                        }
                    }
                    catch { }
                }
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(controllerName, "Erro ao verificar grupo de segurança: " + error.ToString());
            }

            ldapConnection.Disconnect();

            return claims;
        }

        private async Task<Boolean> UserVerifyPassword(LoginViewModel login, String usuarioDN)
        {
            //Valida usuário e senha
            try
            {
                ldapConnection.Bind(ldap.LdapVersion, usuarioDN, login.Password);
                return true;

            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(controllerName, "Erro ao verificar senha: " + error.ToString());
                return false;
            }

        }

        //Busca o caminho completo do usuário no AD
        private async Task<String> GetUserDN(String username)
        {
            String filter = String.Format(ldap.SearchFilterUser, username);
            var result = ldapConnection.Search(ldap.SearchBase, LdapConnection.SCOPE_SUB, filter, null, false);

            try
            {
                return result.First().DN;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(controllerName, "Erro ao buscar DN do usuário: " + error.ToString());
                return null;
            }

        }

        private async Task<String> GetGroupDN(String grupo)
        {
            String filter = String.Format(ldap.SearchFilterGroup, grupo);
            var result = ldapConnection.Search(ldap.SearchBase, LdapConnection.SCOPE_SUB, filter, null, false);

            try
            {
                return result.First().DN;
            }
            catch (Exception error)
            {
                await log.SaveLogApplicationError(controllerName, "Erro ao buscar DN do grupo: " + error.ToString());
                return null;
            }

        }

    }


}
