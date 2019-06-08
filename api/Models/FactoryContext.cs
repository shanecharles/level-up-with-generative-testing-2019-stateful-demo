using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace api.Models 
{
    public class FactoryContext : DbContext
    {
        public FactoryContext(DbContextOptions<FactoryContext> options)
       : base(options)
       {
       }
        public DbSet<Rfid> Rfids { get; set; }
        public DbSet<RfidEvent> RfidEvents { get; set; }
    }

}