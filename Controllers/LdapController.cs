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
    public class LdapController : Controller
    {
        private readonly ILdapRepository ldapRepository;

        public LdapController(ILdapRepository ldapRepository)
        {
            this.ldapRepository = ldapRepository;
        }

        [Authorize(Policy = "Administration")]
        public async Task<IActionResult> Create() {

            var ldap = await ldapRepository.Get();

//Somente cria um novo caso não exista
            if (ldap != null) {
                return RedirectToAction("Details", new { id = 1 });
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administration")]
        public async Task<IActionResult> Create([Bind("Server,Port,BindLogin,BindPassword,SearchFilterUser,SearchFilterGroup,SearchBase,LdapVersion")] Ldap ldap)
        {
            if (ModelState.IsValid)
            {
                ldap.User = User.Identity.Name;
                ldap.ChangeDate = DateTime.Now;

                await ldapRepository.Create(ldap);
                return RedirectToAction("Details", new { id = 1 });
            }
            return View(ldap);
        }


        [Authorize(Policy = "Administration")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();

            }

            var ldap = await ldapRepository.Get();

            if (ldap == null)
            {
                return this.RedirectToAction("Create");
            }

            ViewBag.BindLogin = Lib.Cipher.Decrypt(ldap.BindLogin, ldap.ChangeDate.ToString());

            return View(ldap);
        }


        [Authorize(Policy = "Administration")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ldap = await ldapRepository.Get();

            if (ldap == null)
            {
                return NotFound();
            }

            ViewBag.BindLogin = Lib.Cipher.Decrypt(ldap.BindLogin, ldap.ChangeDate.ToString());

            return View(ldap);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administration")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Server,Port,BindLogin,BindPassword,SearchFilterUser,SearchFilterGroup,SearchBase,LdapVersion")] Ldap ldap)
        {
            if (id != ldap.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                ldap.User = User.Identity.Name;
                ldap.ChangeDate = DateTime.Now;

                var result = await ldapRepository.Update(ldap);
                if (result == null)
                {
                    return RedirectToAction("Details", new { id = 1 });
                }

            }

            return View(ldap);
        }

    }
}
