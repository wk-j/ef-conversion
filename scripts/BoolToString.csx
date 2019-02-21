#! "netcoreapp2.1"
#r "nuget:Microsoft.Extensions.DependencyInjection,2.1.0"
#r "nuget:Microsoft.Extensions.DependencyInjection.Abstractions,2.1.0"
#r "nuget:Npgsql.EntityFrameworkCore.PostgreSQL,2.1.0"

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

public class Student {
    [Key]
    public int Id { set; get; }

    public bool Height { set; get; }
    public bool Fast { set; get; }
}

public class MyContext : DbContext {
    public MyContext(DbContextOptions options) : base(options) { }
    public DbSet<Student> Students { set; get; }

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.Entity<Student>().Property(x => x.Height).HasConversion(new BoolToStringConverter("OFF", "ON"));
        builder.Entity<Student>().Property(x => x.Fast).HasConversion(new BoolToTwoValuesConverter<string>("OFF", "ON"));
    }
}

var collection = new ServiceCollection();
collection.AddDbContextPool<MyContext>(options => {
    options.UseNpgsql("Host=localhost;User Id=postgres;Password=1234;Database=BoolToString");
});

var provider = collection.BuildServiceProvider();

using (var context = provider.GetService<MyContext>()) {
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
    context.Students.Add(new Student { Height = true, Fast = true });
    context.Students.Add(new Student { Height = false, Fast = false });
    context.SaveChanges();
}

using (var context = provider.GetService<MyContext>()) {
    var students = context.Students.ToList();
    foreach (var item in students) {
        Console.WriteLine("Height: {0}, Fast: {1}", item.Height, item.Fast);
    }
}

/*
Height: True, Fast: True
Height: False, Fast: False
 */
