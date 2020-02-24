using System.ComponentModel.DataAnnotations;

namespace GDR.Models.ViewModels
{
    public class LoginViewModel
    {

        [Required(ErrorMessage = "É necessário informar o usuário")]
        [Display(Name = "Usuário")]

        public string Username { get; set; }

        [Required(ErrorMessage = "É necessário informar a senha")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Display(Name = "Lembrar?")]
        public bool RememberMe { get; set; }
    }
}
