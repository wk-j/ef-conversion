using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace BoolToString {
    public class Student {
        [Key]
        public int Id { set; get; }

        public bool Height { set; get; }
        public bool Fast { set; get; }
    }

    public class MyContext : DbContext {

        public MyContext(DbContextOptions options) : base(options) {

        }

        public DbSet<Student> Students { set; get; }

        protected override void OnModelCreating(ModelBuilder builder) {
            builder.Entity<Student>().Property(x => x.Height).HasConversion(new BoolToStringConverter("OFF", "ON"));
            builder.Entity<Student>().Property(x => x.Fast).HasConversion(new BoolToTwoValuesConverter<string>("OFF", "ON"));
        }
    }

    class Program {
        static void Main(string[] args) {
            var collection = new ServiceCollection();
            collection.AddDbContextPool<MyContext>(options => {
                options.UseNpgsql("Host=localhost;User Id=postgres;Password=1234;Database=BoolToString");
            });

            var provider = collection.BuildServiceProvider();

            using (var context = provider.GetService<MyContext>()) {
                // context.Database.EnsureDeleted();
                // context.Database.EnsureCreated();
                // context.Students.Add(new Student { Height = true, Fast = true });
                // context.Students.Add(new Student { Height = false, Fast = false });
                // context.Students.Add(new Student { Height = true, Fast = false });
                // context.Students.Add(new Student { Height = false, Fast = true });
                // context.SaveChanges();

            }

            using (var context = provider.GetService<MyContext>()) {
                var students = context.Students;
                foreach (var item in students) {
                    Console.WriteLine("{0} {1}", item.Height, item.Fast);
                }
            }
        }
    }
}
