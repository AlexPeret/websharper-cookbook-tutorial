namespace WebSharperTutorial.FrontEnd

open System

open WebSharper

[<JavaScript>]
module DTO =

    type User = {
        Code: int64
        Firstname: string
        Lastname: string
        UpdateDate: DateTime
    }

    let CreateUser code firstname lastname updateDate =
        {
            Code = code
            Firstname = firstname
            Lastname = lastname
            UpdateDate = updateDate
        }
