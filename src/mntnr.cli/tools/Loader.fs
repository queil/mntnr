namespace Mntnr

open System.IO
open Mntnr.Model.Errors
open Mntnr.Model.Types
open Queil.FSharp.FscHost.Plugin

module ParserTypes =
 
  type Script = {
    index: int
    description: string
    matchTags: string list
    diffHash: string
    func: unit -> unit
  }

[<RequireQualifiedAccess>]
module Loader =
  open ParserTypes

  let readScripts (bindingName:string) (scripts:ScriptsPath) =
    plugin<(int * string * string list * string * (unit -> unit)) list>{
        load
        dir scripts.Dir
        file scripts.FileName
        binding bindingName
      }
    |> Async.RunSynchronously
    |> (fun scripts ->
        Ok (
          scripts |> List.map (fun (idx, desc, matchTags, diffHash, func) -> 
          {
            index = idx
            description = desc
            matchTags = matchTags
            diffHash = diffHash
            func = func
          })
      ))

  let findScriptFiles (gitRelativePath:string) (discovery:DiscoverySettings) =
    let chunks = gitRelativePath.Split('/') |> Seq.toList

    let collectScripts path scripts =
      let scriptsPath = ScriptsPath (discovery.rootDir, Path.Combine(path, discovery.fileName))
     
      if File.Exists scriptsPath.FullPath
      then
        let result = scriptsPath |> readScripts discovery.propertyName
        match result with
        | Ok x -> 
          (scriptsPath, x)::scripts
        | Error e -> failwith e
      else scripts

    let rec recurse pathChunks scripts : (ScriptsPath * Script list) list =

      let dirPath = 
        match pathChunks |> String.concat $"%c{Path.DirectorySeparatorChar}" with
        | "" -> discovery.rootDir
        | dir -> Path.Combine(discovery.rootDir, dir)
        
      let scripts = collectScripts dirPath scripts
      
      match pathChunks with
      | [] -> scripts
      | s -> recurse (s[..^1]) scripts
      
    match recurse chunks [] with
    | [] -> raise NoScriptsFound
    | xs -> xs
