namespace Mntnr
open System.Text.RegularExpressions

[<RequireQualifiedAccess>]
module TagMatching =
  let isMatch (repoTags: string list) (matchExpressions: string list) =
    match (repoTags, matchExpressions) with
     | _, [".*"] -> true
     | _, [] -> false
     | _ -> query {
              for s in matchExpressions do
              all (
                query {
                  for r in repoTags do
                  exists (r = s || Regex.IsMatch(r,s))
                }
              )
            }
