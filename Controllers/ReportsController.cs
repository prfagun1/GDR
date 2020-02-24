using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GDR;
using GDR.Models;
using GDR.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace GDR.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportRepository reportRepository;
        private readonly IDatabaseGDRRepository databaseGDRRepository;
        private readonly IPermissionGroupRepository permissionGroupRepository;

        public ReportsController(IReportRepository reportRepository, IDatabaseGDRRepository databaseGDRRepository, IPermissionGroupRepository permissionGroupRepository)
        {
            this.reportRepository = reportRepository;
            this.databaseGDRRepository = databaseGDRRepository;
            this.permissionGroupRepository = permissionGroupRepository;
        }

        [Authorize(Policy = "ReportsAdministration")]
        public async Task<IActionResult> Index(bool registroApagado)
        {
            var reportList = await reportRepository.GetList(EnableEnum.All);

            if (registroApagado)
                ViewBag.RegistroApagado = "<p>Registro apagado com sucesso </p>";

            return View(reportList);
        }

        [Authorize(Policy = "ReportsAdministration")]
        public async Task<IActionResult> Create()
        {

            //Busca lista de bancos de dados
            List<DatabaseGDR> databasesGDRList = await databaseGDRRepository.GetList(EnableEnum.Enabled);
            ViewBag.DatabaseID = databasesGDRList.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            ViewBag.DatabaseIDSelected = new MultiSelectList(String.Empty, "Text", "Value");


            //Busca lista de grupos de permissões
            List<PermissionGroup> permissionGroupList = await permissionGroupRepository.GetList();
            ViewBag.PermissionGroupID = permissionGroupList.Select(x => new SelectListItem
            {
                Text = x.GroupName,
                Value = x.Id.ToString()
            }).ToList();

            ViewBag.PermissionGroupIdSelected = new MultiSelectList(String.Empty, "Text", "Value");

            ViewBag.EnableSelect = reportRepository.GetSelectList(EnableEnum.Enabled);


            return View();
        }

        [Authorize(Policy = "ReportsAdministration")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,SQL")] Report report, int[] DatabaseIDSelected, int[] PermissionGroupIdSelected)
        {

            if (ModelState.IsValid)
            {
                foreach (var databaseId in DatabaseIDSelected) {
                    ReportDatabaseGDR databaseGDR = new ReportDatabaseGDR();
                    databaseGDR.DatabaseGDRId = databaseId;
                    report.ReportDatabaseGDR.Add(databaseGDR);
                }

                foreach (var permissionGroupId in PermissionGroupIdSelected)
                {
                    ReportPermissionGroup permissionGroup = new ReportPermissionGroup();
                    permissionGroup.PermissionGroupId = permissionGroupId;
                    report.ReportPermissionGroup.Add(permissionGroup);
                }

                report.User = User.Identity.Name;
                report.ChangeDate = DateTime.Now;

                bool status = await reportRepository.Create(report);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.DatabaseGDR = new SelectList(await databaseGDRRepository.GetList(EnableEnum.Enabled), "Id", "Name");

            ViewBag.EnableSelect = reportRepository.GetSelectList(report.Enable);

            return View(report);
        }


        [Authorize(Policy = "ReportsAdministration")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await reportRepository.Get(id);

            if (report == null)
            {
                return NotFound();
            }


            await SetSelectViewBag(report);

            return View(report);
        }

        [Authorize(Policy = "ReportsAdministration")]
        private async Task SetSelectViewBag(Report report)
        {
            //Busca bancos de dados
            List<DatabaseGDR> databasesGDRSelect = await databaseGDRRepository.GetList(EnableEnum.Enabled);
            List<DatabaseGDR> databaseGDRSelected = report.ReportDatabaseGDR.Select(x => x.DatabaseGDR).ToList();
            databasesGDRSelect = databasesGDRSelect.Except(databaseGDRSelected, new DatabaseGDRIdComparer()).ToList();

            ViewBag.DatabaseID = databasesGDRSelect.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            ViewBag.DatabaseIDSelected = databaseGDRSelected.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();


            //Busca lista de grupos de permissões
            List<PermissionGroup> permissionGroupSelect = await permissionGroupRepository.GetList();
            List<PermissionGroup> permissionGroupSelected = report.ReportPermissionGroup.Select(x => x.PermissionGroup).ToList();
            permissionGroupSelect = permissionGroupSelect.Except(permissionGroupSelected, new PermissionGroupIdComparer()).ToList();


            ViewBag.PermissionGroupID = permissionGroupSelect.Select(x => new SelectListItem
            {
                Text = x.GroupName,
                Value = x.Id.ToString()
            }).ToList();

            ViewBag.PermissionGroupIdSelected = permissionGroupSelected.Select(x => new SelectListItem
            {
                Text = x.GroupName,
                Value = x.Id.ToString()
            }).ToList();


            ViewBag.EnableSelect = reportRepository.GetSelectList(report.Enable);

        }

        [Authorize(Policy = "ReportsAdministration")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,SQL,Enable")] Report report, int[] DatabaseIDSelected, int[] PermissionGroupIdSelected)
        {
            if (id != report.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                report.User = User.Identity.Name;
                report.ChangeDate = DateTime.Now;


                var result = await reportRepository.Update(report, DatabaseIDSelected, PermissionGroupIdSelected);
                if (result == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(Index));
            }


            await SetSelectViewBag(report);

            return View(report);
        }

        [Authorize(Policy = "ReportsAdministration")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await reportRepository.Get(id);

            if (report == null)
            {
                return NotFound();
            }

            ViewBag.DatabaseList = await databaseGDRRepository.GetList(report.Id);
            ViewBag.PermissionGroupList = await permissionGroupRepository.GetList();

            return View(report);
        }

        [Authorize(Policy = "ReportsAdministration")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            var result = await reportRepository.Delete(id);
            if (result == false)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }



    }

    /*
    public static class ExtensionMethods
    {
        public static IEnumerable<TA> Except<TA, TB, TK>(
            this IEnumerable<TA> a,
            IEnumerable<TB> b,
            Func<TA, TK> selectKeyA,
            Func<TB, TK> selectKeyB,
            IEqualityComparer<TK> comparer = null)
        {
            return a.Where(aItem => !b.Select(bItem => selectKeyB(bItem)).Contains(selectKeyA(aItem), comparer));
        }
    }*/
}
