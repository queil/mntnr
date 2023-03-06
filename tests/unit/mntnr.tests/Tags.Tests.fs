namespace Mntnr.Tests

open Expecto
open Mntnr

module Tags =

  [<Tests>]
  let tests =
    testList "Tags" [

      test "Should not match if there is no intersection between script tags and repo tags" {
        let repoTags = ["build-image: dotnet-nodejs"; "tilt"]
        let scriptTags = ["build-image: python"]
        "Tags shouldn't match" |> Expect.equal (TagMatching.isMatch repoTags scriptTags) false
      }

      test "Should not match if scripts tags set is empty" {
        let repoTags = ["build-image: dotnet-nodejs"; "tilt"]
        let scriptTags = []
        "" |> Expect.equal (TagMatching.isMatch repoTags scriptTags) false
      }

      test "Should not match if scipts tags is a superset of repo tags" {
        let repoTags = ["build-image: dotnet-nodejs"; "tilt"]
        let scriptTags = ["build-image: dotnet-nodejs"; "tilt"; "only-this"]
        "The 'only-this' tag should prevent from matching" |> Expect.equal (TagMatching.isMatch repoTags scriptTags) false
      }

      test "Should match if script tags are a subset of repo tags" {
        let repoTags = ["build-image: dotnet-nodejs"; "tilt"]
        let scriptTags = ["build-image: dotnet-nodejs"]
        "The tags should match" |> Expect.equal (TagMatching.isMatch repoTags scriptTags) true
      }

      test "Should match if script tags are a subset of repo tags with regexp match" {
        let repoTags = ["build-image: dotnet-nodejs"; "tilt"]
        let scriptTags = ["build-image: dotnet-.*"; "tilt"]
        "The tags should match" |> Expect.equal (TagMatching.isMatch repoTags scriptTags) true
      }

      test "Should match if repo has no tags and scripts is a wildcard" {
        let repoTags = []
        let scriptTags = [".*"]
        "The tags should match" |> Expect.equal (TagMatching.isMatch repoTags scriptTags) true
      }
    ]
