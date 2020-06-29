namespace WebSharperTutorial.FrontEnd.Components

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.JQuery
open WebSharper.JavaScript // require by the Remote<'T> type

open WebSharperTutorial.FrontEnd
open WebSharperTutorial.FrontEnd.Routes

[<JavaScript>]
module NavigationBar =

    let private navItem label callback =
        li [ attr.``class`` "nav-item" ]
           [
             a [ attr.``class`` "nav-link"
                 on.click (fun _ _ -> callback())
               ]
               [ text label ]
           ]

    let private navBar items =
        nav
          [ attr.``class`` "navbar navbar-expand-lg navbar-light bg-light" ]
          [ a [ attr.``class`` "navbar-brand" ] [ text "W#" ]
            button
              [ attr.``class`` "navbar-toggler"
                attr.``type`` "button"
                attr.``data-`` "toggle" "collapse"
                attr.``data-`` "target" "#navbarSupportedContent"
                Attr.Create "aria-controls" "navbarSupportedContent"
                Attr.Create "aria-expanded" "false"
                Attr.Create "aria-label" "Toggle navigation"
              ]
              [ span [ attr.``class`` "navbar-toggler-icon" ] []]

            div
              [ attr.``class`` "collapse navbar-collapse"
                attr.id "navbarSupportedContent"
              ]
              [ ul [ attr.``class`` "navbar-nav mr-auto" ] items
              ]
          ]

    let private buildNavbar items =
        items
        |> List.map (fun (label,callback) -> navItem label callback)
        |> navBar

    let private logoff (router:Var<EndPoint>) =
        async {
            do! Remote<Auth.RpcUserSession>.Logout ()
            router.Value <- Login
        }
        |> Async.Start

    let Main (router:Var<EndPoint>) =
        async {
            let! loggedUser =
                Remote<Auth.RpcUserSession>.GetLogin()

            return
                match loggedUser with
                | None ->
                    [ "Login",(fun () -> router.Value <- Login)
                    ]
                    |> buildNavbar

                | Some _ ->
                    [ "Listing",(fun () -> router.Value <- Listing)
                      "Logout",(fun () -> logoff router)
                    ]
                    |> buildNavbar
        }
        |> Doc.Async

