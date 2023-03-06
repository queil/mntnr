namespace Mntnr.Cli

open Mntnr.Cmd
open Argu

type MntnrArgs =
  | [<CliPrefix(CliPrefix.None)>][<SubCommand>] Version
  | [<AltCommandLine("-v")>] Verbose
  | [<AltCommandLine("-c")>][<Unique>]Repo_Config_File_Path of path:string
  | [<Unique>][<First>][<MainCommand>] Scripts_Root_Dir of path: string
  | [<Unique>] Scripts_File_Name of name:string  
  | [<Unique>] Module_Name of name:string
  | [<Unique>] Property_Name of name: string
  | [<CliPrefix(CliPrefix.None)>] Check of ParseResults<Check.Cli.Args>
  | [<CliPrefix(CliPrefix.None)>] Apply of ParseResults<Apply.Cli.Args>
  | [<CliPrefix(CliPrefix.None)>] Preview of ParseResults<Apply.Cli.Args>
 with
   interface IArgParserTemplate with
       member this.Usage =
           match this with
           | Version -> "Prints the version"
           | Verbose -> "Print a lot of output to stdout."
           | Repo_Config_File_Path _ -> "Path to the repository config file. Default: .mntnr"
           | Scripts_Root_Dir _ -> "The directory all the script files reside in. Default: current working dir"
           | Scripts_File_Name _ -> "Only files with this name will be recursively discovered within the scripts root dir: Default: scripts.fsx"
           | Module_Name _ -> "F# module name declared in the scripts source file. Default: Scripts"
           | Property_Name _ -> "Property name declared in the scripts module. Default: All"
           | Check _ -> "Checks if the current repository is up to date"
           | Apply _ -> "Applies pending scripts if any"
           | Preview _ -> "Like apply but it doesn't stage/commit changes"
