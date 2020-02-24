using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GDR.Models
{

    public enum EnableEnum {
        Enabled,
        Disabled,
        All
    }
    public class GDRModel : GDRBaseModel
    {
        public int Id { get; set; }

        [Display(Name = "Usuário")]
        public string User { get; set; }

        [Display(Name = "Data alteração")]
        public DateTime ChangeDate { get; set; }
    }

    public class GDRBaseModel { }
}
