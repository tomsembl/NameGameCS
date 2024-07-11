using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.Design;
using NameGameCS.Models;

namespace NameGameCS {
    public class NameGameDbContext : DbContext {
        public NameGameDbContext()
            : base() {
            this.Database.Migrate();
            if (this.DefaultNames.Count() == 0) {
                IEnumerable<DefaultName> defaultNamesList =  DefaultNamesList.names.Select(name => new DefaultName { name = name });
                this.DefaultNames.AddRange(defaultNamesList);
                this.SaveChanges();
            }
            if (this.Mp3Order.Count() == 0) {
                Mp3Order mp3Order = new Mp3Order {
                    number_stops = 13,
                    current_stop = 2,
                    number_starts = 18,
                    current_start = 6
                };
                this.Mp3Order.Add(mp3Order);
                this.SaveChanges();
            }
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
