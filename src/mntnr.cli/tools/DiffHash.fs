namespace Mntnr
open Fake.Core
open System.Security.Cryptography
open System.Text
open System

[<RequireQualifiedAccess>]
module DiffHash =
  
  let compute (s:string) =
    use sha256 = SHA256.Create()
    s |> Encoding.UTF8.GetBytes
      |> sha256.ComputeHash
      |> BitConverter.ToString
      |> String.replace "-" ""
      |> (fun x -> x.Remove(16).ToLower())
