namespace Mntnr

open System.Text.RegularExpressions
open Fake.Core

[<RequireQualifiedAccess>]
module Git =
  
  let (|ParseRegex|_|) regex str =
    let m = Regex(regex).Match(str)
    if m.Success
    then Some (List.tail [ for x in m.Groups -> x.Value ])
    else None
  
  let run cmd = 
    cmd |> CreateProcess.redirectOutput
        |> CreateProcess.ensureExitCode
        |> Proc.run
        |> (fun x -> x.Result.Output)
 
  let diff verbose = 
    let output = CreateProcess.fromRawCommand "git" ["--no-pager"; "diff"; "--unified=1"] |> run
    if verbose then printfn $"----- \n%s{output}\n -----"
    output.Split("\n")
    |> Array.toList
    |> function | [] -> [] | h::t -> [h]@t.[1..]
    |> String.concat "\n"
    
  let commit message = CreateProcess.fromRawCommand "git" ["commit"; "-m"; message] |> run
  let status () = CreateProcess.fromRawCommand "git" ["status"; "--porcelain"] 
                |> run
                |> String.split '\n'
                |> List.choose (function | s when String.isNullOrWhiteSpace s -> None | s -> Some s)
                |> List.map (String.trim >> fun s -> 
                                        let chunks = s.Split(" ")
                                        chunks.[0], chunks.[1])

  let isDirty () = status () <> []
  let add path = CreateProcess.fromRawCommand "git" ["add"; path] |> run |> ignore
  let reset path = CreateProcess.fromRawCommand "git" ["reset"; "--"; path] |> run |> ignore
  let anyStaged () =
    CreateProcess.fromRawCommand "git" ["update-index"; "--refresh"] |> Proc.run |> ignore
    let anyStaged =
      CreateProcess.fromRawCommand "git" ["diff-index"; "--quiet"; "HEAD"; "--"]
      |> CreateProcess.redirectOutput
      |> Proc.run
      |> (fun x -> match x.ExitCode with | 0 -> false |_ -> true)

    anyStaged

  let remoteUrl () =
      CreateProcess.fromRawCommand "git" ["remote"; "get-url"; "origin"] |> run

  let relativePath remoteUrl =
    match remoteUrl with
    | ParseRegex "https:\/\/(.+)@([^\/]+)\/(.+).git" [_; _; path] -> path
    | ParseRegex "https:\/\/([^\/]+)\/(.+)" [_;path] -> path 
    | ParseRegex "git@(.+):(.+).git" [_; path] -> path
    | _ -> failwith $"Cannot extract Git path from remote URL: {remoteUrl}"

  let matches pattern (relativePath:string) =
    match relativePath with
    | ParseRegex pattern _ -> true
    | _ -> false

  let remotePathMatches pattern =
      (remoteUrl >> relativePath >> matches pattern) ()
