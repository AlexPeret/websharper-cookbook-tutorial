namespace WebSharperTutorial.FrontEnd.Pages

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.JQuery
open WebSharper.JavaScript // require by the Remote<'T> type

open WebSharperTutorial.FrontEnd
open WebSharperTutorial.FrontEnd.Components

[<JavaScript>]
module PageHome =

    let Main router =
        let navBar =
            NavigationBar.Main router

        [
            navBar
            div [ attr.``class`` "container" ]
                [
                  div [ attr.``class`` "row" ]
                      [ div [ attr.``class`` "col-xs-12 col-sm-6 mx-auto" ]
                            [ text "this is the home page" ]
                      ]
                ]
        ]
        |> Doc.Concat
