namespace Mntnr.Model

open System
open System.IO
open FsYaml
open YamlDotNet.RepresentationModel
open YamlDotNet.Serialization

module Errors =
  exception MntnrFileNotFound of path: string
  exception PreviewModeDirtyRepo
  exception NoScriptsFound

module Types =

  type ScriptsVersion =
    {
      path: string
      version: int
    }
   
  type MntnrFileInfo(path: string) =
      
      [<Obsolete("To be removed in version 3")>]
      let convertFromLegacyVersion () =
          if File.Exists path
          then
            use reader = File.OpenText(path)
            let yaml = YamlStream()
            yaml.Load(reader)
            let doc = yaml.Documents[0].RootNode :?> YamlMappingNode
            match doc["version"] with
            | :? YamlScalarNode ->
              let ver = (doc["version"] :?> YamlScalarNode).Value
              let versions = YamlMappingNode()
              versions.Add("/", ver)
              doc.Children["version"] <- versions
            | _ -> ()
            use writer = new StreamWriter(path)
            let output = Serializer().Serialize(doc)
            File.WriteAllText(path, output)
            
      member x.Load () =
        if not <| File.Exists path then raise (Errors.MntnrFileNotFound path)
        convertFromLegacyVersion ()
        Yaml.load<MntnrFile> (File.ReadAllText(path))
        
      member x.SetVersion (newVersion:ScriptsVersion) =
        convertFromLegacyVersion ()
        use reader = File.OpenText(path)
        let yaml = YamlStream()
        yaml.Load(reader)
        let doc = yaml.Documents[0].RootNode :?> YamlMappingNode
        let versions = doc["version"] :?> YamlMappingNode
        if versions.Children.ContainsKey(newVersion.path) then
          let version = versions[newVersion.path] :?> YamlScalarNode
          version.Value <- newVersion.version |> string
        else
          versions.Add(newVersion.path, newVersion.version |> string)
         
        use writer = new StreamWriter(path)
        let output = Serializer().Serialize(doc)
        File.WriteAllText(path, output)
      
  and  
    MntnrFile = {
      version: Map<string, int>
      tags: string list option
      labels: Map<string, string> option
  }
  with
    member x.CombinedTags = 
      List.concat [
        match x.tags with Some s -> s | _-> []
        match x.labels with Some ls -> ls |> Map.toList |> List.map (fun (k, v) -> $"{k}: {v}") |_ -> []
      ]
    
    member x.Version path =
      match x.version.ContainsKey(path) with
      | true -> x.version[path]
      | _ -> 0


  type CommonOptions =
    {
      file: MntnrFileInfo
      config: MntnrFile
      discovery: DiscoverySettings
      verbose: bool
    }
  
  and DiscoverySettings =
    {
      rootDir: string
      fileName: string
      moduleName: string
      propertyName: string
    }

  type ScriptsPath = | ScriptsPath of rootDir: string * fullPath: string
    with
    
      member x.FullPath =
        let (ScriptsPath(_, fullPath)) = x
        fullPath
      
      member x.NormalizedDir =
        let (ScriptsPath(rootDir, fullPath)) = x
        let relative = Path.GetRelativePath(rootDir, fullPath)
        "/" + Path.GetDirectoryName(relative)
     
      member x.Dir =
        let (ScriptsPath(_, fullPath)) = x
        Path.GetDirectoryName(fullPath)
      
      member x.FileName =
        let (ScriptsPath(_, path)) = x
        Path.GetFileName(path)
