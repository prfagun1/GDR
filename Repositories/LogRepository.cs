using GDR.Lib;
using GDR.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GDR.Repositories
{

    public interface ILogRepository
    {
        Task SaveLogApplicationMessage(string controllerName, string message);
        Task SaveLogApplicationError(string controllerName, string message);
        Task<int> CountAsync();

    }
    public class LogRepository : GDRRepository<Log>, ILogRepository
    {

        private readonly IParameterRepository parameterRepository;
        private readonly IHttpContextAccessor contextAcessor;
        public LogRepository(GDRContext context, IParameterRepository parameterRepository, IHttpContextAccessor contextAcessor) : base(context)
        {
            this.parameterRepository = parameterRepository;
            this.contextAcessor = contextAcessor;

        }

        public async Task<int> CountAsync()
        {
            return await dbSet.CountAsync();
        }


        public async Task SaveLogApplicationMessage(string controllerName, string message)
        {
            Log messageLog = new Log();
            messageLog.LogType = LogTypeEnum.Info;
            await SaveLogApplication(messageLog, controllerName, message);
        }


        public async Task SaveLogApplicationError(string controllerName, string message)
        {
            Log messageLog = new Log();
            messageLog.LogType = LogTypeEnum.Error;
            await SaveLogApplication(messageLog, controllerName, message);

        }

        private async Task SaveLogApplication(Log messageLog, string controllerName, string message)
        {
            try
            {

                //var logLevelApplication = _context.Parameter.First().LogLevelApplication;
                //if (!(logLevelApplication <= messageLog.LogType)) { return; }

                string username = contextAcessor.HttpContext.User.Identity.Name;

                messageLog.ControllerName = controllerName;
                messageLog.ChangeDate = DateTime.Now;
                messageLog.User = username;
                messageLog.Message = message;

                await context.AddAsync(messageLog);
                await context.SaveChangesAsync();
            }
            catch (Exception error)
            {
                SaveLogError(error);
            }
        }


        private void SaveLogError(Exception error)
        {
            string logFile;

            try
            {
                //logFile = _context.Parameter.First().LogErrorPath;

            }
            catch
            {
                logFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
                logFile = logFile.Substring(0, logFile.LastIndexOf("\\"));
                logFile = logFile + @"\\error.log";
            }

            try
            {
                //System.IO.File.AppendAllText(logFile, DateTime.Now.ToString() + " - " + error.ToString() + System.Environment.NewLine);
            }
            catch { }
        }
    }
}
