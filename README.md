# mntnr

A simple repo maintenance system. Applies an ordered sequence of F# scripts (fsx) to a repository. The latest applied script index number is kept in the `.mntnr` file in each repo.

## Configuration

### Target repositories

Mntnr expects `.mntnr` file in the root of every target repository. It the file does not exist it gets created (with the defaults) on the first apply run.

Example files:

```yaml
version:
  /: 32
  /some/other/source: 15
labels:
  my.company/app/stack: dotnet/3.1
  my.company/app/os: linux/alpine
tags:
- tag1
- tag2
```

### Check

To check if there are any non-applied scripts:
```
mntnr /path/to/scripts/root check
```
Exit codes:
* 0 - there are pending scripts to apply
* 3 - the repository is up to date

### Apply

To run the maintenance:
```
mntnr /path/to/scripts/root apply
```
Exit codes:
* 0 - scripts applied and can auto-merge
* 3 - scripts applied but the result contains unexpected results (i.e. cannot auto-merge)

## Writing scripts

The scripts must be located in the `Scripts` module and declared as a list named `All`:

```fsharp
module Scripts

open Mntnr.Functions

let All = [
  1, "Description", ["some-tag"; "image: dotnet-.*"], "98f051e55251df33", fun () -> createFile "myfilename.txt" [
    "line 1"
    "line 2"
  ]
  2, "Yet another script", [".*"], "05e62513bc93bf9e", fun () -> () // do nothing
]
```

## Tags and labels

Tags and labels can both be used to attach metadata properties to repositories.
`Mntnr` can use them to only apply a script to a subset of repositories.

* **tags**  are simple string values

  ```yaml
  tags:
  - dotnet
  - alpine
  ```

* **labels** are key-value pairs and provide more structured metadata

  ```yaml
  labels:
    your.company/some/label: value
    your.company/another/label: value2 
  ```

You can use tags, labels, or both depending on your requirements.

### Match expressions

The corresponding match expressions attached to the scripts apply to both tags and labels. 

They have the following properties:

* scripts can specify multiple match expressions combined with `AND` logical operation
* the match expressions can be both literal values and regular expressions

Matching rules:

* if a script has no tags (`[]`) it won't be applied to any repository
* scripts with the following tag: `[".*"]` will be applied to all repositories
* scripts with match expressions `["a"; "b.*"; "label: some.*"]` will be applied to all repositories having both the `a` tag, any tag matching `b.*` regular expression, and any tag or label matching `label: some.*` regular expression

## Diff hashing

You can provide an expected git diff hash (first 16 chars of a SHA256 checksum) for every script. On applying, each script's changes are individually diffed and the diff gets hashed. Updating the version in `.mntnr` is done after the diff hash gets calculated so it has no influence on the hash. That hash is then compared with the expected hash and the current script's pending changes are committed. If all hashes in the current apply run match it indicates an automatic merge can be performed. 

The expected hash can be calculated as follows:

Note: the `index` line (which comes second) is removed before hashing as it varies between repos)

```sh
git --no-pager diff --unified=1 | sed -E '2d' | shasum -a 256 | head -c 16
```
