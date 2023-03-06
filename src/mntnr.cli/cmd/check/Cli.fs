namespace Mntnr.Cmd.Check

open Mntnr.Model.Types
open Argu
open Model

module Cli =

  type Args = 
  | [<Unique>] Update_Pending_Exit_Code of exitCode:int
  | [<Unique>] Up_To_Date_Exit_Code of exitCode:int
  with 
    interface IArgParserTemplate with
      member s.Usage =
        match s with
        | Update_Pending_Exit_Code _ -> "Sets the exit code that gets returned if there are pending scripts to apply. Default: 3"
        | Up_To_Date_Exit_Code _ -> "Sets the exit code that gets returned if the current repository is up to date. Default: 0"
  
  let map (opts: CommonOptions) (xs: ParseResults<Args>) =
    {
      common = opts 
      updatePendingExitCode = xs.TryGetResult Update_Pending_Exit_Code |> Option.defaultValue 0
      upToDateExitCode = xs.TryGetResult Up_To_Date_Exit_Code |> Option.defaultValue 3
    }
