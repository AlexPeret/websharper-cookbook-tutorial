namespace WebSharperTutorial.FrontEnd

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server

module Site =
    open WebSharper.UI.Html
    open WebSharper.UI.Client // required by the Doc.EmbedView
    open WebSharperTutorial.FrontEnd.Routes
    open WebSharperTutorial.FrontEnd.Pages

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

    let LoginPage ctx endpoint =
        let body =
            client
                <@ let router = Routes.InstallRouter ()

                   router.View
                   |> View.Map (fun endpoint ->
                       PageLogin.Main router
                   )
                   |> Doc.EmbedView
                @>
        MainTemplate ctx endpoint "Login" [ body ]

    [<Website>]
    let Main =
        Sitelet.New
            SiteRouter
            (fun ctx endpoint ->
                let loggedUser =
                    async {
                        return! ctx.UserSession.GetLoggedInUser()
                    } |> Async.RunSynchronously

                match loggedUser with
                | None -> // user is not authenticated. Allow only public EndPoints
                    match endpoint with
                    | EndPoint.Home -> HomePage ctx
                    | EndPoint.Login ->
                        LoginPage ctx endpoint
                    | EndPoint.AccessDenied ->
                        MainTemplate ctx EndPoint.Home "Access Denied Page"
                            [ div [] [ text "Access denied" ] ]
                    | _ ->
                        Content.RedirectTemporary AccessDenied

                | Some (u) -> // user is authenticated. Allow all EndPoints
                    match endpoint with
                    | EndPoint.Home -> HomePage ctx
                    | EndPoint.Login ->
                        LoginPage ctx endpoint
                    | EndPoint.Listing ->
                        MainTemplate ctx EndPoint.Home "Listing Page"
                            [ div [] [ text "Listing Page - implementation pending" ] ]
                    | EndPoint.Form code ->
                        MainTemplate ctx EndPoint.Home "Form Page"
                            [ div [] [ text "Form Page - implementation pending" ] ]
                    | _ ->
                        MainTemplate ctx EndPoint.Home "not implemented"
                            [ div [] [ text "implementation pending" ] ]
            )
