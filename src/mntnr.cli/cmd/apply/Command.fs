namespace Mntnr.Cmd.Apply

open Mntnr.Model.Errors
open Mntnr.Model.Types
open Model
open Mntnr
open Mntnr.ParserTypes
open System.Text
open Mntnr.Utils
open System
open System.IO

module Command =
  let run (opts: Options) =
    
    if opts.previewMode && Git.isDirty () then raise PreviewModeDirtyRepo
        
    let apply scripts (scriptsPath: ScriptsPath) =
      let builder = StringBuilder()
        
      let apply s =
        builder.appendEcho $"🔧 {s.index} - {s.description} ... "
        let snapshot = Git.status ()

        try
          s.func()
          builder.appendnEcho "✅"
        with 
        | exn -> builder.appendnEcho $"❌\n```{exn.ToString()}```\n---"

        let currentHash = Git.diff opts.common.verbose |> DiffHash.compute
        let isMatch = currentHash.Equals(s.diffHash, StringComparison.OrdinalIgnoreCase)
        printfn "\nHash: %s %s %s" currentHash (match isMatch with | true -> "=" | _ -> "<>") s.diffHash
        
        opts.common.file.SetVersion { path = scriptsPath.NormalizedDir; version = s.index }
        
        match opts.previewMode with
        | true -> ()
        | false ->
        
          Git.add "--all"
          if Git.anyStaged() then
            match snapshot with
            | [] -> ()
            | xs ->
              if opts.common.verbose then printfn "\n---\nWill ignore pre-existing changes: "
              xs
              |> Seq.filter (
                   function
                   | "M", ".mntnr" when Environment.GetEnvironmentVariable("MNTNR_CONFIG_UPGRADE") = "1" -> false
                   | _ -> true
                )
              |> Seq.iter (fun (k,v) ->
                if opts.common.verbose then printfn $"%s{k} %s{v}"
                Git.reset v)
            Git.commit $"{s.index} - {s.description}" |> printfn "%s"
        isMatch

      let eligibleScripts =
        scripts
        |> List.sortBy (fun s -> s.index)
        |> List.skipWhile (fun s ->  s.index <= opts.common.config.Version scriptsPath.NormalizedDir)
        |> List.filter (fun s -> TagMatching.isMatch opts.common.config.CombinedTags s.matchTags)

      builder.appendnEcho $"## Scripts from: `{scriptsPath.NormalizedDir}`"
      
      match eligibleScripts with
      | [] -> UpToDate
      | xs ->
        let allDiffsMatch = xs |> Seq.map apply |> Seq.fold (&&) true
        builder.appendnEcho "\n"
        File.AppendAllLines(".mntnr.log.md", [| builder.ToString() |], UTF8Encoding(false))
        if allDiffsMatch then DiffsMatch else DiffsDoNotMatch
        
    let run () =

      opts.common.discovery
      |> Loader.findScriptFiles ((Git.remoteUrl >> Git.relativePath) ()) 
      |> Seq.map (fun (scriptFilePath, scripts) -> (scriptFilePath, apply scripts scriptFilePath))
      |> Seq.fold (fun (messages, previous) (scriptFilePath, current) ->

        let result =
          match previous, current with
          | _, DiffsDoNotMatch -> current
          | DiffsDoNotMatch, _ -> previous
          | _, DiffsMatch -> current
          | DiffsMatch, _ -> previous
          | _ -> current

        match current with
        | UpToDate -> $"Up to date (%s{scriptFilePath.NormalizedDir})"::messages, result
        | DiffsMatch -> $"Diffs match (%s{scriptFilePath.NormalizedDir})"::messages, result
        | DiffsDoNotMatch -> $"Diffs do not match (%s{scriptFilePath.NormalizedDir})"::messages, result

        ) ([], UpToDate)

    let map =
      function
      | UpToDate -> opts.upToDateExitCode
      | DiffsDoNotMatch -> opts.diffsDoNotMatchExitCode
      | DiffsMatch -> opts.diffsMatchExitCode

    (run >> fun (messages, result) -> (messages, (result |> map))) ()
