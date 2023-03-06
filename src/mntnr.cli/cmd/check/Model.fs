namespace Mntnr.Cmd.Check

open Mntnr.Model.Types

module Model =

  type Options =
    {
      common: CommonOptions
      upToDateExitCode: int
      updatePendingExitCode: int
    }
  
 type ScriptFileResult = 
    | UpToDate of exitCode: int
    | PendingUpdate of exitCode: int 
