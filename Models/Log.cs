using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GDR.Models
{
    public enum LogTypeEnum {
        Info,
        Error
    }

    public class Log : GDRModel
    {
        [Display(Name = "Tipo")]
        public LogTypeEnum LogType { get; set; }

        [Display(Name = "Local")]
        public string ControllerName { get; set; }

        [Display(Name = "Mensagem")]
        public string Message { get; set; }

    }
}
