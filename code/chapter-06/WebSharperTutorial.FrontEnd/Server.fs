namespace WebSharperTutorial.FrontEnd

open System

open WebSharper

open WebSharperTutorial.FrontEnd
open WebSharperTutorial.FrontEnd.DTO

module Server =

    let private dbUsers () =
        [
            CreateUser 1L "Firstname 1" "Lastname 1" (new DateTime(2020,3,17))
            CreateUser 2L "Firstname 2" "Lastname 2" (new DateTime(2019,6,21))
            CreateUser 3L "Firstname 3" "Lastname 3" (new DateTime(2019,8,14))
        ]

    [<Rpc>]
    let GetUsers () : Async<User list> =
        async {
            return dbUsers()
        }

    let private optionToResult msg o =
        match o with
        | None -> Result.Error msg
        | Some v -> Result.Ok v

    let private validateFirstname (user:User) =
        match user.Firstname with
        | null -> Error "No fistname found."
        | "" -> Error "Fistname is empty."
        | _ -> Ok user

    let private validateLastname (user:User) =
        match user.Firstname with
        | null -> Error "No lastname found."
        | "" -> Error "Lastname is empty."
        | _ -> Ok user

    let private validateRequest userResult =
        userResult
        |> Result.bind validateFirstname
        |> Result.bind validateLastname

    let private updateUser user =
        { user with
              UpdateDate = DateTime.Now
        }

    [<Rpc>]
    let SaveUser (dto:User) : Async<Result<User,string>> =
        async {
            return
                dto
                |> Ok
                |> validateRequest
                |> Result.map updateUser
        }

    [<Rpc>]
    let GetUser (code:int64) : Async<Result<User,string>> =
        async {
            // simulate delay
            do! Async.Sleep(2000)

            let userO =
                dbUsers()
                |> List.tryFind(fun u -> u.Code = code)
                |> optionToResult "User not found!"

            return userO
        }
