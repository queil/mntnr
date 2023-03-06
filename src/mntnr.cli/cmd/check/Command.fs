namespace Mntnr.Cmd.Check

open Mntnr.Model.Types
open Model
open Mntnr
open Mntnr.ParserTypes

module Command =
  let run (opts: Options) =
    let check scripts (scriptsPath:ScriptsPath) =
      let latestMatchingScriptIndex =
        scripts
        |> Seq.filter (fun s -> TagMatching.isMatch opts.common.config.CombinedTags s.matchTags)
        |> Seq.map (fun s -> s.index) |> Seq.max

      let updateNeeded =
        let ver = opts.common.config.Version scriptsPath.NormalizedDir
        ver = 0 || ver < latestMatchingScriptIndex
      
      if updateNeeded 
      then PendingUpdate opts.updatePendingExitCode
      else UpToDate opts.upToDateExitCode
    
    let run () =
      opts.common.discovery 
      |> Loader.findScriptFiles ((Git.remoteUrl >> Git.relativePath) ())
      |> Seq.map (fun (scriptFilePath, scripts) -> 
                   printfn $"Checking script file: {scriptFilePath.NormalizedDir}"
                   (scriptFilePath, check scripts scriptFilePath)
        )
      |> Seq.fold (fun (messages, exitCode) (scriptFilePath, result) -> 
        
        match result with
         | UpToDate _ -> $"Up to date (%s{scriptFilePath.NormalizedDir})"::messages, exitCode
         | PendingUpdate code -> $"Pending scripts found (%s{scriptFilePath.NormalizedDir})"::messages, code

      ) ([], opts.upToDateExitCode)
    run ()
