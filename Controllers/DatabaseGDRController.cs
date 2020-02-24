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
    public class DatabaseGDRController : Controller
    {
        private readonly IDatabaseGDRRepository databaseGDRRepository;
        private readonly IDatabaseTypeRepository databaseTypeRepository;
        private readonly IReportRepository reportRepository;

        public DatabaseGDRController(IDatabaseGDRRepository databaseGDRRepository, IDatabaseTypeRepository databaseTypeRepository, IReportRepository reportRepository)
        {
            this.databaseGDRRepository = databaseGDRRepository;
            this.databaseTypeRepository = databaseTypeRepository;
            this.reportRepository = reportRepository;
        }


        [Authorize(Policy = "DatabaseAdministration")]
        public async Task<IActionResult> Index(bool registroApagado)
        {
            var databaseGDR = await databaseGDRRepository.GetList(EnableEnum.All);

            if (registroApagado)
                ViewBag.RegistroApagado = "<p>Registro apagado com sucesso </p>";

            return View(databaseGDR);
        }


        [Authorize(Policy = "DatabaseAdministration")]
        public async Task<IActionResult> Create()
        {

            //Busca lista de relatorios
            List<Report> reportList = await reportRepository.GetList(EnableEnum.Enabled);
            ViewBag.ReportId = reportList.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            ViewBag.ReportIdSelected = new MultiSelectList(String.Empty, "Text", "Value");

            ViewBag.DatabaseTypeId = new SelectList(await databaseTypeRepository.GetList(EnableEnum.Enabled), "Id", "Name");
            ViewBag.EnableSelect = databaseGDRRepository.GetSelectList(EnableEnum.All);

            return View();
        }


        [Authorize(Policy = "DatabaseAdministration")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,DatabaseName,DatabaseServer,DatabaseUser,DatabasePassword,DatabaseTypeId,Port,ConnectionString")] DatabaseGDR databaseGDR, int[] ReportIdSelected)
        {

            if (ModelState.IsValid)
            {

                foreach (var reportId in ReportIdSelected)
                {
                    ReportDatabaseGDR reportDatabaseGDR = new ReportDatabaseGDR();
                    reportDatabaseGDR.ReportID = reportId;
                    databaseGDR.ReportDatabaseGDR.Add(reportDatabaseGDR);
                }

                databaseGDR.User = User.Identity.Name;
                databaseGDR.ChangeDate = DateTime.Parse(DateTime.Now.ToString());

                if (await databaseGDRRepository.Create(databaseGDR)) {
                    return RedirectToAction(nameof(Index));
                }
                
            }

            ViewBag.EnableSelect = databaseGDRRepository.GetSelectList(databaseGDR.Enable);
            ViewBag.DatabaseTypeId = new SelectList(await databaseTypeRepository.GetList(EnableEnum.Enabled), "Id", "Name");

            return View(databaseGDR);
        }


        [Authorize(Policy = "DatabaseAdministration")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var databaseGDR = await databaseGDRRepository.Get(id);

            if (databaseGDR == null)
            {
                return NotFound();
            }


            await SetSelectViewBag(databaseGDR);

            try
            {
                ViewBag.DatabaseUser = Lib.Cipher.Decrypt(databaseGDR.DatabaseUser, databaseGDR.ChangeDate.ToString());
            }
            catch (Exception e)
            {
                ViewBag.DatabaseUser = "";
            }


            return View(databaseGDR);
        }

        [Authorize(Policy = "DatabaseAdministration")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,DatabaseName,DatabaseServer,DatabaseUser,DatabasePassword,DatabaseTypeId,Port,ConnectionString,Enable")] DatabaseGDR databaseGDR, int[] ReportIdSelected)
        {
            if (id != databaseGDR.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                databaseGDR.User = User.Identity.Name;
                databaseGDR.ChangeDate = DateTime.Parse(DateTime.Now.ToString());

                var result = await databaseGDRRepository.Update(databaseGDR, ReportIdSelected);

                if (result == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(Index));
            }


            await SetSelectViewBag(databaseGDR);

            return View(databaseGDR);
        }

        [Authorize(Policy = "DatabaseAdministration")]
        private async Task SetSelectViewBag(DatabaseGDR databaseGDR)
        {
            //Busca bancos de dados
            List<Report> reportsSelect = await reportRepository.GetList(EnableEnum.Enabled);
            List<Report> reportSelected = databaseGDR.ReportDatabaseGDR.Select(x => x.Report).ToList();
            reportsSelect = reportsSelect.Except(reportSelected, new ReportIdComparer()).ToList();


            ViewBag.ReportId = reportsSelect.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            ViewBag.ReportIdSelected = reportSelected.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();


            ViewBag.EnableSelect = databaseGDRRepository.GetSelectList(databaseGDR.Enable);
            ViewBag.DatabaseTypeId = new SelectList(await databaseTypeRepository.GetList(EnableEnum.Enabled), "Id", "Name", databaseGDR.DatabaseTypeId);

        }

        [Authorize(Policy = "DatabaseAdministration")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var databaseGDR = await databaseGDRRepository.Get(id);

            if (databaseGDR == null)
            {
                return NotFound();
            }

            try
            {
                ViewBag.DatabaseUser = Lib.Cipher.Decrypt(databaseGDR.DatabaseUser, databaseGDR.ChangeDate.ToString());
            }
            catch (Exception e)
            {
                ViewBag.DatabaseUser = "";
            }


            return View(databaseGDR);
        }


        [Authorize(Policy = "DatabaseAdministration")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            var result = await databaseGDRRepository.Delete(id);
            if (result == false)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
