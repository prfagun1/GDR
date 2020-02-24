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
    public class ParameterController : Controller
    {
        private readonly IParameterRepository parameterRepository;

        public ParameterController(IParameterRepository parameterRepository)
        {
            this.parameterRepository = parameterRepository;
        }


        [Authorize(Policy = "Administration")]
        public IActionResult Create()
        {

            var parameter = parameterRepository.Get();

            //Somente cria um novo caso não exista
            if (parameter != null)
            {
                return RedirectToAction("Details", new { id = 1 });
            }

            ViewBag.LogLevelSelect = GetSelectListLogLevel(parameter.LogLevelApplication);

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administration")]
        public async Task<IActionResult> Create([Bind("LogErrorPath,LogLevelApplication,AdminUser,AdminPassword")] Parameter parameter)
        {
            if (ModelState.IsValid)
            {
                parameter.User = User.Identity.Name;
                parameter.ChangeDate = DateTime.Now;

                if (await parameterRepository.Create(parameter)) {
                    return RedirectToAction("Details", new { id = 1 });
                }
            }

            ViewBag.LogLevelSelect = GetSelectListLogLevel(parameter.LogLevelApplication);

            return View(parameter);
        }

        public SelectList GetSelectListLogLevel(LogLevelParameterEnum? logLevel)
        {
            SelectList logLevelSelect = new SelectList(new[] {
                new { ID = LogLevelParameterEnum.Info, Name = "Info" },
                new { ID = LogLevelParameterEnum.Error, Name = "Erro" },
            }, "ID", "Name", logLevel);

            return logLevelSelect;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administration")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LogErrorPath,LogLevelApplication,AdminUser,AdminPassword")] Parameter parameter)
        {
            if (id != parameter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                parameter.User = User.Identity.Name;
                parameter.ChangeDate = DateTime.Now;

                var result = await parameterRepository.Update(parameter);
                if (result != null)
                {
                    return RedirectToAction("Details", new { id = 1 });
                }

            }

            ViewBag.LogLevelSelect = GetSelectListLogLevel(parameter.LogLevelApplication);
            ViewBag.AdminUser = Lib.Cipher.Decrypt(parameter.AdminUser, parameter.ChangeDate.ToString());

            return View(parameter);
        }


        [Authorize(Policy = "Administration")]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();

            }

            var parameter = parameterRepository.Get();

            ViewBag.AdminUser = Lib.Cipher.Decrypt(parameter.AdminUser, parameter.ChangeDate.ToString());

            if (parameter == null)
            {
                return this.RedirectToAction("Create");
            }

            return View(parameter);
        }


        [Authorize(Policy = "Administration")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parameter = parameterRepository.Get();

            if (parameter == null)
            {
                return NotFound();
            }

            ViewBag.LogLevelSelect = GetSelectListLogLevel(parameter.LogLevelApplication);
            ViewBag.AdminUser = Lib.Cipher.Decrypt(parameter.AdminUser, parameter.ChangeDate.ToString());

            return View(parameter);
        }

    }
}
