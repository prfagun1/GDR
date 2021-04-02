using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GDR.Lib;
using GDR.Models;
using GDR.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GDR.Controllers
{
    public class UserReportsController : Controller
    {
        private readonly IDatabaseConnection databaseConnection;
        private readonly IReportRepository reportRepository;
        private readonly IAuthentication authentication;
        private readonly IDatabaseGDRRepository databaseGDRRepository;
        private readonly ILogRepository log;
        private readonly string ControllerName = "UserReports";

        public UserReportsController(IDatabaseConnection databaseConnection, IReportRepository reportRepository, IAuthentication authentication
            , IDatabaseGDRRepository databaseGDRRepository, ILogRepository log)
        {
            this.databaseConnection = databaseConnection;
            this.reportRepository = reportRepository;
            this.authentication = authentication;
            this.databaseGDRRepository = databaseGDRRepository;
            this.log = log;
        }

        [Authorize(Policy = "Read")]
        public async Task<IActionResult> Index(int? ReportList, int?[] DatabaseList, string Filtro)
        {

            List<DatabaseGDR> databaseReport;
            List<DatabaseGDR> databaseList;

            List<int> permissionGroups = authentication.GetPermissionGroups(HttpContext.User.Claims);

            List<Report> reportList = await reportRepository.GetList(permissionGroups);
            ViewBag.ReportList = new SelectList(reportList, "Id", "Name", ReportList);
            ViewBag.Filtro = Filtro;


            List<ReportResult> reportResult = new List<ReportResult>();

            //Somente pesquisa caso seja informado o relatório e um banco
            if (ReportList != null)
            {
                var report = await reportRepository.Get(ReportList);
                databaseList = await databaseGDRRepository.GetList(report.Id);

                if (DatabaseList.Length > 0)
                {
                    databaseReport = await databaseGDRRepository.GetList(DatabaseList);
                    
                }
                else {
                    databaseReport = databaseList;
                }

                try
                {
                    ViewBag.DatabaseList = new MultiSelectList(databaseList, "Id", "Name", DatabaseList);
                    reportResult = await databaseConnection.Get(report, databaseReport, Filtro);

                    await log.SaveLogApplicationMessage(ControllerName, $"Relatório {report.Name} executado.");
                }
                catch(Exception error) {
                    ViewBag.Error = "Erro ao gerar relatório: " + error.ToString();
                    await log.SaveLogApplicationError(ControllerName, $"Erro ao executar relatório {report.Name}: {error.ToString()}");
                }

            }


            return View(reportResult.Take(5000));
        }

        [Authorize(Policy = "Read")]
        [HttpPost]
        public async Task<ActionResult> GetDatabases() {

            int.TryParse(Request.Form["reportId"], out int reportId);

            var databaseList = (await databaseGDRRepository.GetList(reportId)).Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            return Json(databaseList);
        }
    }
}