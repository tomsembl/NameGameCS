using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.Design;
using NameGameCS.Models;

namespace NameGameCS {
    public class NameGameDbContext : DbContext {
        public NameGameDbContext()
            : base() {
            this.Database.Migrate();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserInstance> UserInstances { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Name> Names { get; set; }
        public DbSet<DefaultName> DefaultNames { get; set; }
        public DbSet<Turn> Turns { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Mp3Order> Mp3Order { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseSqlite("Data Source=NameGame.db");
            }
        }
        //dotnet add package Microsoft.EntityFrameworkCore.Design
        //dotnet tool install --global dotnet-ef
        //dotnet ef migrations add InitialCreate
        //dotnet ef database update


        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

        }
    }
}
