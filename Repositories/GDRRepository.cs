using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GDR.Models;
using Microsoft.EntityFrameworkCore;

namespace GDR.Repositories
{

    public abstract class GDRRepository<T> where T : GDRBaseModel
    {
        protected readonly GDRContext context;
        protected readonly DbSet<T> dbSet;

        public GDRRepository(GDRContext context)
        {
            this.context = context;
            dbSet = context.Set<T>();
        }
    }
}
