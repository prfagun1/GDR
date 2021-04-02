using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GDR.Models
{
    public class Report : GDRModel
    {
        public Report() {
            this.ReportDatabaseGDR = new HashSet<ReportDatabaseGDR>();
            this.ReportPermissionGroup = new HashSet<ReportPermissionGroup>();
        }

        [Required(ErrorMessage = "O campo nome não pode estar em branco")]
        [Display(Name = "Nome do relatório")]
        [StringLength(500, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "A descrição pode ter no máximo 1.000 caracteres.")]
        [Display(Name = "Descrição")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "O campo SQL não pode estar em branco")]
        [StringLength(5000, ErrorMessage = "O comando SQL pode ter no máximo 5.000 caracteres.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "SQL")]
        public string SQL { get; set; }

        [Display(Name = "Ativo")]
        public EnableEnum Enable { get; set; }

        [Display(Name = "Bancos de dados")]
        public virtual ICollection<ReportDatabaseGDR> ReportDatabaseGDR { get; set; }

        [Display(Name = "Grupo de permissões")]
        public virtual ICollection<ReportPermissionGroup> ReportPermissionGroup { get; set; }

    }

    public class ReportDatabaseGDR{

        public int ReportID { get; set; }
        public Report Report { get; set; }
        public int DatabaseGDRId { get; set; }
        public DatabaseGDR DatabaseGDR { get; set; }
    }

    public class ReportPermissionGroup
    {
        public int ReportID { get; set; }
        public Report Report { get; set; }
        public int PermissionGroupId { get; set; }
        public PermissionGroup PermissionGroup { get; set; }
    }


    internal class ReportIdComparer : IEqualityComparer<Report>
    {
        public bool Equals(Report x, Report y)
        {
            if (x.Id == y.Id)
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(Report obj)
        {
            return obj.Name.GetHashCode();
        }
    }

}
