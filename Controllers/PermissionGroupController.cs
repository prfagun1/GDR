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
    public class PermissionGroupController : Controller
    {
        private readonly IPermissionGroupRepository permissionGroupRepository;
        private readonly IReportRepository reportRepository;

        public PermissionGroupController(IPermissionGroupRepository permissionGroupRepository, IReportRepository reportRepository)
        {
            this.permissionGroupRepository = permissionGroupRepository;
            this.reportRepository = reportRepository;
        }

        [Authorize(Policy = "Administration")]
        public async Task<IActionResult> Index(bool registroApagado)
        {
            var permissionGroup = await permissionGroupRepository.GetList();

            if (registroApagado)
                ViewBag.RegistroApagado = "<p>Registro apagado com sucesso </p>";

            return View(permissionGroup);
        }

        [Authorize(Policy = "Administration")]
        public async Task<IActionResult> Create()
        {

            try
            {
                //Busca lista de relatórios
                List<Report> reportList = await reportRepository.GetList(EnableEnum.Enabled);
                ViewBag.ReportId = reportList.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();

                ViewBag.ReportIdSelected = new MultiSelectList(String.Empty, "Text", "Value");

                ViewBag.AccessType = permissionGroupRepository.GetSelectAccessTypeList(null);

                return View();
            }
            catch {
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Policy = "Administration")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupName,Domain,AccessType")] PermissionGroup permissionGroup, int[] ReportIdSelected)
        {

            if (ModelState.IsValid)
            {

                foreach (var reportId in ReportIdSelected)
                {
                    ReportPermissionGroup report = new ReportPermissionGroup();
                    report.ReportID = reportId;
                    permissionGroup.ReportPermissionGroup.Add(report);
                }


                permissionGroup.User = User.Identity.Name;
                permissionGroup.ChangeDate = DateTime.Now;

                bool status = await permissionGroupRepository.Create(permissionGroup);
                return RedirectToAction(nameof(Index));
            }


            ViewBag.AccessType = permissionGroupRepository.GetSelectAccessTypeList(permissionGroup.AccessType);
            return View(permissionGroup);
        }

        [Authorize(Policy = "Administration")]
        private async Task SetSelectViewBag(PermissionGroup permissionGroup)
        {
            //Busca bancos de dados
            List<Report> reportSelect = await reportRepository.GetList(EnableEnum.Enabled);
            List<Report> reportSelected = permissionGroup.ReportPermissionGroup.Select(x => x.Report).ToList();
            reportSelect = reportSelect.Except(reportSelected, new ReportIdComparer()).ToList();

            ViewBag.ReportId = reportSelect.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            ViewBag.ReportIDSelected = reportSelected.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            ViewBag.AccessType = permissionGroupRepository.GetSelectAccessTypeList(permissionGroup.AccessType);
        }

        [Authorize(Policy = "Administration")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permissionGroup = await permissionGroupRepository.Get(id);

            if (permissionGroup == null)
            {
                return NotFound();
            }

            await SetSelectViewBag(permissionGroup);

            return View(permissionGroup);
        }


        [Authorize(Policy = "Administration")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupName,Domain,AccessType,Id")] PermissionGroup permissionGroup, int[] ReportIDSelected)
        {
            if (id != permissionGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                permissionGroup.User = User.Identity.Name;
                permissionGroup.ChangeDate = DateTime.Now;


                var result = await permissionGroupRepository.Update(permissionGroup, ReportIDSelected);

                if (result == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(Index));
            }

            await SetSelectViewBag(permissionGroup);

            return View(permissionGroup);
        }

        [Authorize(Policy = "Administration")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permissionGroup = await permissionGroupRepository.Get(id);

            if (permissionGroupRepository == null)
            {
                return NotFound();
            }

            return View(permissionGroup);
        }


        [Authorize(Policy = "Administration")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            var result = await permissionGroupRepository.Delete(id);
            if (result == false)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
