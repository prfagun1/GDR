using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GDR.Models
{
    public enum LogLevelParameterEnum
    {
        Info,
        Error
    }
    public class Parameter : GDRModel
    {
        [Display(Name = "Caminho do arquivo de log")]
        [Required(ErrorMessage = "É preciso informar o caminho do log em caso de erros de conexão ao banco.")]
        [MinLength(1, ErrorMessage = "Nome deve ter no mínimo  caracter")]
        [MaxLength(255, ErrorMessage = "Nome deve ter no máximo 255 caracteres")]
        public string LogErrorPath { get; set; }

        [Display(Name = "Nível do log")]
        [Required]
        public LogLevelParameterEnum LogLevelApplication { get; set; }

        [Display(Name = "Usuário administrativo")]
        [Required(ErrorMessage = "É preciso informar o nome.")]
        [MaxLength(50, ErrorMessage = "Nome deve ter no máximo 50 caracteres")]
        public string AdminUser { get; set; }

        [Display(Name = "Senha do usuário administrativo")]
        [Required(ErrorMessage = "É preciso informar a senha do usuário.")]
        [DataType(DataType.Password)]
        [MaxLength(500, ErrorMessage = "Nome deve ter no máximo 500 caracteres")]
        public string AdminPassword { get; set; }


    }

    public class Ldap : GDRModel
    {
        [Display(Name = "Servidor")]
        [Required(ErrorMessage = "É preciso informar o servidor")]
        [MaxLength(255, ErrorMessage = "Servidor deve ter no máximo 255 caracteres")]
        public string Server { get; set; }

        [Display(Name = "Porta")]
        [Required(ErrorMessage = "É preciso informar a porta")]
        public int Port { get; set; }

        [Display(Name = "Bind login")]
        [Required(ErrorMessage = "É preciso informar o usuário de conexão")]
        [MaxLength(255, ErrorMessage = "Bind login deve ter no máximo 255 caracteres")]
        public string BindLogin { get; set; }

        [Display(Name = "Bind password")]
        [Required(ErrorMessage = "É preciso informar a senha de conexão")]
        [DataType(DataType.Password)]
        [MaxLength(255, ErrorMessage = "Bind password deve ter no máximo 255 caracteres")]
        public string BindPassword { get; set; }

        [Display(Name = "Filtro de busca de usuário")]
        [Required(ErrorMessage = "É preciso informar o filtro de busca de usuário")]
        [MaxLength(255, ErrorMessage = "Filtro de busca de usuário deve ter no máximo 255 caracteres")]
        public string SearchFilterUser { get; set; }

        [Display(Name = "Filtro de busca de grupo")]
        [Required(ErrorMessage = "É preciso informar o filtro de busca de grupos")]
        [MaxLength(255, ErrorMessage = "Filtro de busca de grupo deve ter no máximo 255 caracteres")]
        public string SearchFilterGroup { get; set; }

        [Display(Name = "Base de busca")]
        [Required(ErrorMessage = "É preciso informar a base onde serão buscados os usuários")]
        [MaxLength(255, ErrorMessage = "Base de busca deve ter no máximo 255 caracteres")]
        public string SearchBase { get; set; }

        [Display(Name = "Versão LDAP")]
        [Required(ErrorMessage = "É preciso informar a versão do LDAP")]
        public int LdapVersion { get; set; }

    }



}
