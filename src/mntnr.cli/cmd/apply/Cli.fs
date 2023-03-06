namespace Mntnr.Cmd.Apply

open Mntnr.Model.Types
open Argu
open Model

module Cli =

  type Args = 
  | [<Unique>] Diffs_Match_Exit_Code of exitCode:int
  | [<Unique>] Diffs_Do_Not_Match_Exit_Code of exitCode:int
  | [<Unique>] Up_To_Date_Exit_Code of exitCode:int
  with 
    interface IArgParserTemplate with
      member s.Usage =
        match s with
        | Diffs_Match_Exit_Code _ -> "Sets the exit code that gets returned if all diff hashes for all scripts matched the expected ones. Default: 0"
        | Diffs_Do_Not_Match_Exit_Code _ -> "Sets the exit code that gets returned if at least one diff hash didn't match the expected one. Default: 3"
        | Up_To_Date_Exit_Code _ -> "Sets the exit code that gets returned if the current repository is up to date. Default: 0"

  let map (opts: CommonOptions) (xs: ParseResults<Args>) =
    {
      common = opts 
      diffsMatchExitCode = xs.TryGetResult Diffs_Match_Exit_Code |> Option.defaultValue 0
      diffsDoNotMatchExitCode = xs.TryGetResult Diffs_Do_Not_Match_Exit_Code |> Option.defaultValue 3
      upToDateExitCode = xs.TryGetResult Up_To_Date_Exit_Code |> Option.defaultValue 0
      previewMode = false
    }

  let mapPreview opts xs = { map opts xs with previewMode = true }
