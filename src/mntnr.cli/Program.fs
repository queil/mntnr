open System.Reflection
open Argu
open Mntnr.Model
open Mntnr.Model.Types
open Mntnr.Cli
open Mntnr.Cmd
open Queil.FSharp.FscHost.Errors

[<EntryPoint>]
let main argv =

  let ver () = Assembly.GetExecutingAssembly().GetName().Version.ToString()
       
  try
    let parser = ArgumentParser.Create<MntnrArgs>(programName = "mntnr")
    let cmd = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
    let configFilePath = cmd.TryGetResult Repo_Config_File_Path |> Option.defaultValue ".mntnr"

    let loader = MntnrFileInfo(configFilePath)
    let verbose = cmd.Contains Verbose
    let scriptsRootDir = cmd.TryGetResult Scripts_Root_Dir |> Option.defaultValue ""
    let commonOpts loader = 
      {
        file = loader
        config = loader.Load()
        verbose = verbose
        discovery = {
          fileName = cmd.TryGetResult Scripts_File_Name |> Option.defaultValue "scripts.fsx"
          moduleName = cmd.TryGetResult Module_Name |> Option.defaultValue "Scripts" 
          propertyName = cmd.TryGetResult Property_Name |> Option.defaultValue "All"
          rootDir = scriptsRootDir
        }
      }

    try
      match cmd.GetSubCommand() with
      | Version -> 
        printfn $"⛰️  Mntnr %s{ver ()}"
        0
      | Check xs -> 
        let messages, code = xs |> Check.Cli.map (commonOpts loader) |> Check.Command.run
        messages |> List.rev |> List.iter (printfn "%s")
        code
      | Apply xs ->
        let opts = xs |> Apply.Cli.map (commonOpts loader) 
        let messages, code = opts |> Apply.Command.run 
        messages |> List.rev |> List.iter (printfn "%s")
        code
      | Preview xs ->
        let messages, code = xs |> Apply.Cli.mapPreview (commonOpts loader) |> Apply.Command.run 
        messages |> List.rev |> List.iter (printfn "%s")
        code
      | _ -> 
        printfn "%s" (parser.PrintUsage("mntnr"))
        1
    with
    | Errors.MntnrFileNotFound path ->
      printfn $"ERROR: the expected .mntnr config file was not found at path: %s{path}"
      1
    | Errors.PreviewModeDirtyRepo ->
      printfn "ERROR: the repository is not clean. Please clean it up first"
      1
    | Errors.NoScriptsFound ->
      let dirName =
        match scriptsRootDir with
        | "" -> "the current dir"
        | s -> s
      printfn $"ERROR: no scripts found (recursively) in {dirName}"
      1
    | ScriptParseError errors ->
      printfn "Script error:\n"
      errors |> Seq.iter (printfn "%s")
      1
    | ScriptCompileError errors ->
      printfn "Script error:\n"
      errors |> Seq.iter (printfn "%s")
      1
    | ExpectedMemberParentTypeNotFound memberPath ->
      printfn $"Expected member path not found: '%s{memberPath}'"
      1
    | ScriptMemberNotFound (prop, otherProps) ->
      printfn $"Expected member not found: %s{prop}\nOther found members:\n"
      otherProps |> Seq.iter (printfn "%s")
      1
       
    | e -> 
      printfn $"\n%s{e.Message}\n"
      printfn "Add --verbose to see the stack trace" 
      if verbose then
        printfn $"%s{e.StackTrace}"
      1  
  with
  | :? ArguParseException as p -> 
    printfn $"%s{p.Message}"
    1
