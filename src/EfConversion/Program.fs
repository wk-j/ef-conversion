// Learn more about F# at http://fsharp.org

open System
open System.ComponentModel.DataAnnotations
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Internal
open Microsoft.EntityFrameworkCore.Storage.ValueConversion

type Status =
    | Import = 0
    | Upload = 1

[<CLIMutable>]
type File = {
    [<Key>]
    Id: int
    Status: Status
    TargetName: string
    TargetPath: string
    LocalPath: string
    LocalName: string
}

type MyContext(options) =
    inherit DbContext(options)

    [<DefaultValue>]
    val mutable private localFiles: DbSet<File>

    member this.File
        with get() = this.localFiles
        and  set v =  this.localFiles <- v

    override __.OnModelCreating(builder) =
        builder
            .Entity<File>()
            .Property(fun x -> x.Status)
            .HasConversion(EnumToStringConverter())
            |> ignore

let createContext() =
    let connectionString = "Host=localhost; User Id=postgres; Password=1234;Database=ValueConversion"
    let builder = DbContextOptionsBuilder()
    let options = builder.UseNpgsql(connectionString).Options
    let context = new MyContext(options)
    context.Database.EnsureCreated() |> ignore
    context

[<EntryPoint>]
let main _ =

    let file = {
        Id = 0
        LocalName = "LocalName"
        LocalPath = "LocalPath"
        TargetName = "TargetName"
        TargetPath = "TargetPath"
        Status = Status.Import
    }

    let context = createContext()
    context.File.Add file |> ignore
    context.SaveChanges() |> ignore

    0 // return an integer exit code
