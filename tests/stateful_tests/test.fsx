#r "../packages/FsCheck/lib/net452/FsCheck.dll"
#r "../packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
#r "../packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#load "./TestModel.fs"

open System.Net
open FsCheck
open Newtonsoft.Json
open FSharp.Data
open FSharp.Data.HttpRequestHeaders
open TestModel


System.Net.ServicePointManager.ServerCertificateValidationCallback <- 
  (fun _ _ _ _ -> true)


type TagRequest = { Tag : string }
type ActionRequest = { Tag : string; Action: string }
type StatusResult = { Status: string; }



type FactoryFloor (tag) =
    let baseUrl = "https://localhost:5001/"
    let shiftUrl = baseUrl + "api/shift"

    let getState () = 
        let data = Http.RequestString(sprintf "%s/status/%s" shiftUrl tag)
        let result = JsonConvert.DeserializeObject<StatusResult>(data)
        result.Status

    let runAction action =
        let data = JsonConvert.SerializeObject({Tag=tag;Action=action})

        let response = Http.Request(shiftUrl, httpMethod = "POST", 
                        headers = [ ContentType HttpContentTypes.Json ],
                        body = TextRequest data,
                        silentHttpErrors = true)
        
        if response.StatusCode >= 500 then 
            failwith (sprintf "Status Code: %A" response.StatusCode)
    
    let reset () =
        let data = JsonConvert.SerializeObject({Tag=tag})
        let apiUrl = baseUrl + "api/rfid"
        seq { 
              yield Http.Request(apiUrl, httpMethod = "DELETE",
                        headers = [ ContentType HttpContentTypes.Json ],
                        body = TextRequest data, 
                        silentHttpErrors = true)
              yield Http.Request(apiUrl, httpMethod = "POST",
                        headers = [ ContentType HttpContentTypes.Json ],
                        body = TextRequest data, 
                        silentHttpErrors = true)
        } |> Seq.forall (fun resp -> resp.StatusCode = 200)
                                  
    do 
        reset () |> ignore
        

    member __.Get () = 
        getState ()
    member __.Reset () =
        reset ()
    
    // Actions
    member __.EndBreak () = 
        runAction "EndBreak"
    member __.StartBreak () =
        runAction "StartBreak"
    member __.StartFloor () =
        runAction "StartFloor" 
    member __.EndFloor () = 
        runAction "EndFloor"



let spec = 
    let startFloor = { new Command<FactoryFloor, TestModel.ModelState>() with 
                            override __.RunActual a = a.StartFloor(); a 
                            override __.RunModel m = startFloor m
                            override __.Post(a, m) = 
                                let actual = a.Get() 
                                let model = getState m
                                actual = model |@ sprintf "startFloor - model: %s <> actual: %s" model actual
                            override __.ToString() = "startFloor" }

    let endFloor = { new Command<FactoryFloor, ModelState>() with 
                            override __.RunActual a = a.EndFloor(); a 
                            override __.RunModel m = endFloor m
                            override __.Post(a, m) = 
                                let actual = a.Get() 
                                let model = getState m
                                actual = model |@ sprintf "endFloor - model: %s <> actual: %s" model actual
                            override __.ToString() = "endFloor" }

    let endBreak = { new Command<FactoryFloor, ModelState>() with 
                            override __.RunActual a = a.EndBreak(); a
                            override __.RunModel m = endBreak m
                            override __.Post(a, m) = 
                                let actual = a.Get() 
                                let model = getState m
                                actual = model |@ sprintf "endBreak - model: %s <> actual: %s" model actual
                            override __.ToString() = "endBreak" }

    let startBreak = { new Command<FactoryFloor, ModelState>() with 
                            override __.RunActual a = a.StartBreak(); a
                            override __.RunModel m = startBreak m
                            override __.Post(a, m) = 
                                let actual = a.Get() 
                                let model = sprintf "%A" m
                                actual = model |@ sprintf "startBreak - model: %s <> actual: %s" model actual
                            override __.ToString() = "startBreak" }

    { new ICommandGenerator<FactoryFloor, ModelState> with 
        member __.InitialActual = FactoryFloor("test")
        member __.InitialModel = OffFloor
        member __.Next model = Gen.elements [startBreak; startFloor; endBreak; endFloor ] }


let config = {
    Config.Verbose with 
        MaxTest = 10
        EndSize = 15
    }


Check.One("Stateful Testing", config, (Command.toProperty spec))
