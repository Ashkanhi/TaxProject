using DBSqlLitePlatform.DBClassObject;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DBSqlLitePlatform.Context
{
    public class NPSGeneralContext : DbContext
    {
        public NPSGeneralContext() : base("Data Source=|DataDirectory|GeneralDb.sqlite;Version=3;") // Connection string name
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<NPSGeneralContext>());
        }
        public DbSet<tEntityComment> tEntityComments { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<tEntityComment>().ToTable("tEntityComments"); // Specify the table name
            base.OnModelCreating(modelBuilder);
        }       
    }
}
