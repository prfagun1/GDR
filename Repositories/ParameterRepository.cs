using GDR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GDR.Repositories
{


    public interface IParameterRepository
    {
        LogLevelParameterEnum GetLogLevel();
        Parameter Get();
        Task<Parameter> Update(Parameter parameter);
        Task<bool> Create(Parameter parameter);
    }

    public class ParameterRepository : GDRRepository<Parameter>, IParameterRepository
    {

        public ParameterRepository(GDRContext context) : base(context)
        {
        }


        public Parameter Get()
        {

            try
            {
                Parameter parameter = dbSet.FirstOrDefault();
                return parameter;
            }
            catch
            {
                return null;
            }
        }

        public LogLevelParameterEnum GetLogLevel()
        {

            Parameter parameter = dbSet.FirstOrDefault();

            if (parameter == null)
            {
                return LogLevelParameterEnum.Info;
            }

            return parameter.LogLevelApplication;
        }

        public async Task<bool> Create(Parameter parameter)
        {
            try
            {
                parameter.AdminUser = Lib.Cipher.Encrypt(parameter.AdminUser, parameter.ChangeDate.ToString());
                parameter.AdminPassword = Lib.Cipher.Encrypt(parameter.AdminPassword, parameter.ChangeDate.ToString());

                await dbSet.AddAsync(parameter);
                await context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Parameter> Update(Parameter parameter)
        {
            try
            {
                parameter.AdminUser = Lib.Cipher.Encrypt(parameter.AdminUser, parameter.ChangeDate.ToString());
                parameter.AdminPassword = Lib.Cipher.Encrypt(parameter.AdminPassword, parameter.ChangeDate.ToString());

                dbSet.Update(parameter);
                await context.SaveChangesAsync();

                return parameter;
            }
            catch
            {
                return null;
            }
        }

    }
}
