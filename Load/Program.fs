﻿open System
open System.Net

open Suave
open Suave.Web
open Suave.Http
open Suave.Types
open Suave.Http.Applicatives
open Suave.Http.Files
open Suave.Log

let logger = Loggers.sane_defaults_for Verbose

let config : SuaveConfig = 
  { default_config with 
      bindings = [ HttpBinding.Create(HTTP, "127.0.0.1", 8082) ]
      ; buffer_size = 2048
      ; max_ops = 10000
      ; logger = logger }

let listening, server = web_server_async config (choose [ GET >>= browse ])
Async.Start server

// wait for the server to start listening
listening |> Async.RunSynchronously |> ignore

async {
    while true do
      for _ in [1 .. 20] do
        use wc = new WebClient()
        wc.DownloadStringAsync(Uri("http://localhost:8082/test.html"))
      do! Async.Sleep 1000
} |> Async.Start

Console.Read() |> ignore