namespace Mntnr

open System.Text

module Utils =

  type StringBuilder with
    member x.appendEcho (text:string) =
      x.Append text |> ignore
      printf $"%s{text}"
    member x.appendnEcho (text:string) =
      x.AppendLine text |> ignore
      printfn $"%s{text}"
