namespace Mntnr.Tests

open Expecto
open Mntnr
open Model.Types

module Loader =

  [<Tests>]
  let tests =
    testList "Loader" [
      test "Should load scripts from a directory tree" {

        let scripts =
            {
              rootDir = "test-root"
              fileName = "maintenance.fsx" 
              moduleName = "Scripts"
              propertyName = "All"
            } |> Loader.findScriptFiles "sub-1/sub-2/empty/sub-3"
        "Collected scripts are different than expected"
        |> Expect.sequenceEqual (scripts |> Seq.map fst) [
            ScriptsPath("test-root", "test-root/maintenance.fsx")
            ScriptsPath("test-root", "test-root/sub-1/maintenance.fsx")
            ScriptsPath("test-root", "test-root/sub-1/sub-2/maintenance.fsx")
            ScriptsPath("test-root", "test-root/sub-1/sub-2/empty/sub-3/maintenance.fsx")
        ]
      }
      
      test "Should load global file" {
        let scripts =
           { 
              rootDir = "test-root"
              fileName = "maintenance.fsx" 
              moduleName = "Scripts"
              propertyName = "All"
            } |> Loader.findScriptFiles "whatever/not/matching/dirs" 
        "Collected scripts are different than expected"
        |> Expect.sequenceEqual (scripts |> Seq.map fst) [
             ScriptsPath("test-root", "test-root/maintenance.fsx")
        ]
      }
    ] |> testLabel "loader"
