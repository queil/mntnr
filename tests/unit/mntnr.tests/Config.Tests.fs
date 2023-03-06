namespace Mntnr.Tests

open System.IO
open System.Text
open Expecto
open Mntnr.Model.Types

module Config =

  let writeLines path lines = File.WriteAllLines(path, lines |> Seq.toList,  UTF8Encoding(false))
  
  [<Tests>]
  let tests =
    testList "Config" [

      test "Should read version and tags" {
        let fileName = ".maintenance.tmp"
        [
          "version:"
          "  /: 1"
          "tags:"
          "- dotnet"
          "- app"
          "labels:"
          "  my.app.com/test: ooo"
        ] |> writeLines fileName 
        let cfg = MntnrFileInfo(fileName).Load()
        "Version should be 1" |> Expect.equal cfg.version["/"] 1
        "Tags should be: dotnet and app" |> Expect.equal cfg.CombinedTags ["dotnet"; "app"; "my.app.com/test: ooo"]
      }

      test "Should read legacy version and tags" {
        let fileName = ".maintenance2.tmp"
        [
          "version: 1"
          "tags:"
          "- dotnet"
          "- app"
          "labels:"
          "  my.app.com/test: ooo"
        ] |> writeLines fileName 
        let cfg = MntnrFileInfo(fileName).Load()
        "Version should be 1" |> Expect.equal cfg.version["/"] 1
        "Tags should be: dotnet and app" |> Expect.equal cfg.CombinedTags ["dotnet"; "app"; "my.app.com/test: ooo"]
      }
      
      test "Should set version legacy" {
        let expected =
          [
            "version:"
            "  /: 972"
            "tags:"
            "- a"
            "- b"
            "labels:"
            "  my.app.com/test: ooo"
          ]
        
        let fileName = ".maintenance3.tmp"
        [
          "version: 1"
          "tags:"
          "- a"
          "- b"
          "labels:"
          "  my.app.com/test: ooo"
        ] |> writeLines fileName
        let file = MntnrFileInfo(fileName)
        file.SetVersion { path = "/"; version = 972 }
        let config2 = File.ReadLines(fileName) |> Seq.toList
        "Version should be 927" |> Expect.equal config2 expected
      }
      
      test "Should set version" {
        let expected =
          [
            "version:"
            "  /: 972"
            "  /some/path: 122"
            "tags:"
            "- a"
            "- b"
            "labels:"
            "  my.app.com/test: ooo"
          ]
        
        let fileName = ".maintenance4.tmp"
        [
          "version:"
          "  /: 1"
          "tags:"
          "- a"
          "- b"
          "labels:"
          "  my.app.com/test: ooo"
        ] |> writeLines fileName
        let file = MntnrFileInfo(fileName)
        file.SetVersion { path = "/"; version = 972 }
        file.SetVersion { path = "/some/path"; version = 122 }
        let config2 = File.ReadLines(fileName) |> Seq.toList
        "Version should be 927" |> Expect.equal config2 expected
      }
    ]
