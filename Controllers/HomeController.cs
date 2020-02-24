using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GDR.Models;
using Microsoft.AspNetCore.Authorization;
using GDR.Lib;
using GDR.Repositories;

namespace GDR.Controllers
{
    public class HomeController : Controller
    {
        private readonly IReportRepository reportRepository;
        private readonly IDatabaseGDRRepository databaseGDRRepository;
        private readonly ILogRepository log;

        public HomeController(IReportRepository reportRepository, IDatabaseGDRRepository databaseGDRRepository, ILogRepository log) {
            this.reportRepository = reportRepository;
            this.databaseGDRRepository = databaseGDRRepository;
            this.log = log;
        }

        [Authorize(Policy = "Read")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Reports = await reportRepository.CountAsync();
            ViewBag.Databases = await databaseGDRRepository.CountAsync();
            ViewBag.Logs = await log.CountAsync();


            return View();
        }

        [Authorize(Policy = "Read")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
