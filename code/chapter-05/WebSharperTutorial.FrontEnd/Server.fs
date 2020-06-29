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

