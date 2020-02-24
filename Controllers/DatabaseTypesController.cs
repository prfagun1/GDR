using GDR.Models;
using GDR.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GDR.Controllers
{
    public class DatabaseTypesController : Controller
    {

        private readonly IDatabaseTypeRepository databaseTypeRepository;


        public DatabaseTypesController(IDatabaseTypeRepository databaseTypeRepository)
        {
            this.databaseTypeRepository = databaseTypeRepository;
        }

        [Authorize(Policy = "DatabaseAdministration")]
        public async Task<IActionResult> Index(bool registroApagado)
        {
            var databaseType = await databaseTypeRepository.GetList(EnableEnum.All);

            if (registroApagado)
                ViewBag.RegistroApagado = "<p>Registro apagado com sucesso </p>";

            return View(databaseType);
        }

        [Authorize(Policy = "DatabaseAdministration")]
        public IActionResult Create()
        {
            return View();
        }


        [Authorize(Policy = "DatabaseAdministration")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] DatabaseType databaseType)
        {

            if (ModelState.IsValid)
            {
                databaseType.User = User.Identity.Name;
                databaseType.ChangeDate = DateTime.Now;
                databaseType.Enable = EnableEnum.Enabled;

                bool status = await databaseTypeRepository.Create(databaseType);
                return RedirectToAction(nameof(Index));
            }
            return View(databaseType);
        }


        [Authorize(Policy = "DatabaseAdministration")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var databaseType = await databaseTypeRepository.Get(id);

            if (databaseType == null)
            {
                return NotFound();
            }


            return View(databaseType);
        }

        [Authorize(Policy = "DatabaseAdministration")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Description,Enable,Id")] DatabaseType databaseType)
        {
            if (id != databaseType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                databaseType.User = User.Identity.Name;
                databaseType.ChangeDate = DateTime.Now;
                databaseType.Enable = EnableEnum.Enabled;

                var result = await databaseTypeRepository.Update(databaseType);
                if (result == null) {
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.EnableSelect = databaseTypeRepository.GetSelectList(databaseType.Enable);
            return View(databaseType);
        }

        [Authorize(Policy = "DatabaseAdministration")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var databaseType = await databaseTypeRepository.Get(id);

            if (databaseType == null)
            {
                return NotFound();
            }

            return View(databaseType);
        }


        [Authorize(Policy = "DatabaseAdministration")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            var result = await databaseTypeRepository.Delete(id);
            if (result == false) {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
