namespace WebSharperTutorial.FrontEnd

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server

type EndPoint =
    | [<EndPoint "/">] Home

module Site =
    open WebSharper.UI.Html

    type MainTemplate = Templating.Template<"templates/Main.html">

    let private MainTemplate ctx action (title: string) (body: Doc list) =
        Content.Page(
            MainTemplate()
                .Title(title)
                .Body(body)
                .Doc()
        )

    let HomePage ctx =
        MainTemplate ctx EndPoint.Home "Home" [
            h1 [] [text "It works!"]
            div [] [ text "Hi there!" ]
        ]

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
        )

