using System;
using System.Collections.Generic;

namespace GDR.Models
{
    public class ReportResult
    {
        public List<String> listaColuna = new List<string>();
        public void addColuna(String coluna)
        {
            listaColuna.Add(coluna);
        }
    }
}
