// Learn more about F# at http://fsharp.org

open System
open System.ComponentModel.DataAnnotations
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Storage.ValueConversion

module Conversion =

  open Microsoft.FSharp.Linq.RuntimeHelpers
  open System.Linq.Expressions

  let toOption<'T> =
    <@ Func<'T, 'T option>(fun (x : 'T) -> match box x with null -> None | _ -> Some x) @>
    |> LeafExpressionConverter.QuotationToExpression
    |> unbox<Expression<Func<'T, 'T option>>>

  let fromOption<'T> =
    <@ Func<'T option, 'T>(fun (x : 'T option) -> match x with Some y -> y | None -> Unchecked.defaultof<'T>) @>
    |> LeafExpressionConverter.QuotationToExpression
    |> unbox<Expression<Func<'T option, 'T>>>

type OptionConverter<'T> () =
  inherit ValueConverter<'T option, 'T> (Conversion.fromOption, Conversion.toOption)

type Status =
    | Import = 0
    | Upload = 1

[<CLIMutable>]
type File = {
    [<Key>]
    Id: int
    Status: Status
    Creator: string option
    Ref: int option
    Name: string
}

type MyContext(options) =
    inherit DbContext(options)

    [<DefaultValue>]
    val mutable private localFiles: DbSet<File>

    member this.Files
        with get() = this.localFiles
        and  set v =  this.localFiles <- v

    override __.OnModelCreating(builder) =
        let options = OptionConverter<string>()
        builder
            .Entity<File>()
            .Property(fun x -> x.Status).HasConversion(EnumToStringConverter())
            |> ignore

        builder
            .Entity<File>()
            .Property(fun x -> x.Creator).HasConversion(options)
            |> ignore

        let op = new OptionConverter<int>()

        builder
            .Entity<File>()
            .Property(fun x -> x.Ref).HasConversion(op)
            |> ignore

let createContext() =
    let connectionString = "Host=localhost; User Id=postgres; Password=1234;Database=ValueConversion"
    let builder = DbContextOptionsBuilder()
    let options = builder.UseNpgsql(connectionString).Options
    let context = new MyContext(options)
    context.Database.EnsureDeleted() |> ignore
    context.Database.EnsureCreated() |> ignore
    context

let insert (context: MyContext) (files: seq<File>) =
    context.Files.AddRange(files) |> ignore
    context.SaveChanges() |> ignore

[<EntryPoint>]
let main _ =

    let context = createContext()

    let files = [
        { Id = 0; Creator = Some "wk"; Ref = None;     Name = "N1"; Status = Status.Import }
        { Id = 0; Creator = Some "wk"; Ref = Some 100; Name = "N2"; Status = Status.Import }
        { Id = 0; Creator = None;      Ref = None;     Name = "N3"; Status = Status.Import }
    ]

    insert context files

    let matchWk = function
        | Some "wk" -> true
        | _  -> false

    query {
        for file in context.Files do
        where (file.Creator |> matchWk)
        select file
    }

    |> Seq.toList
    |> printfn "%A"

    0 // return an integer exit code