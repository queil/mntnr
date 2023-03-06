namespace Mntnr.Cmd.Apply

open Mntnr.Model.Types

module Model =

  type Options =
    {
      common: CommonOptions
      diffsMatchExitCode: int
      diffsDoNotMatchExitCode: int
      upToDateExitCode: int
      previewMode: bool
    }
  
  type Result = 
  | UpToDate
  | DiffsMatch
  | DiffsDoNotMatch
