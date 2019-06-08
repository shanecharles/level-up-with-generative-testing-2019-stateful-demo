module TestModel 

    type ModelState =
        | OffFloor
        | OnFloor
        | OnBreak


    let getState = function
        | OffFloor -> "OffFloor"
        | OnFloor  -> "OnFloor"
        | OnBreak  -> "OnBreak"


// Actions 
    let endBreak state =
        match state with  
        | OnBreak -> OnFloor
        | _       -> state




    let endFloor _ = OffFloor

    let startFloor = function
        | OffFloor -> OnFloor
        | state    -> state    

    let startBreak = function
        | OnFloor -> OnBreak
        | state   -> state