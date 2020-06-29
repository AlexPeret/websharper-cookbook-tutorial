namespace WebSharperTutorial.FrontEnd

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server

module Site =
    open WebSharper.UI.Html
    open WebSharperTutorial.FrontEnd.Routes

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
            client <@ div [] [ text "Hi there!" ] @>
        ]

    [<Website>]
    let Main =
        Sitelet.New
            SiteRouter
            (fun ctx endpoint ->
                match endpoint with
                | EndPoint.Home -> HomePage ctx
                | _ ->
                    MainTemplate ctx EndPoint.Home "not implemented"
                        [ div [] [ text "implementation pending" ] ]
            )
