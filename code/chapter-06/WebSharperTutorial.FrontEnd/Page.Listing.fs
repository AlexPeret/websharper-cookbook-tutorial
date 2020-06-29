namespace WebSharperTutorial.FrontEnd.Pages

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html

open WebSharperTutorial.FrontEnd
open WebSharperTutorial.FrontEnd.Components

[<JavaScript>]
module PageListing =

    type private listingTemplate = Templating.Template<"templates/Page.Listing.html">

    let private buildTable router (users:DTO.User list) =
        let tableRows =
            users
            |> List.map(fun user ->
                listingTemplate.RowTemplate()
                    .Code(string user.Code)
                    .Firstname(user.Firstname)
                    .Lastname(user.Lastname)
                    .UpdatedAt(user.UpdateDate.ToShortDateString())
                    .OnEdit(fun _ -> Var.Set router (Routes.Form user.Code))
                    .Doc()
            )

        listingTemplate()
            .Rows(tableRows)
            .Doc()

    let Main router =
        async {
            let navBar =
                NavigationBar.Main router

            let! users =
                Server.GetUsers()

            let tableElement =
                buildTable router users

            return
                [
                    navBar
                    div [ attr.``class`` "container" ]
                        [
                          div [ attr.``class`` "row" ]
                              [ div [ attr.``class`` "col-12" ]
                                    [ tableElement ]
                              ]
                        ]
                ]
                |> Doc.Concat
        }
        |> Doc.Async
