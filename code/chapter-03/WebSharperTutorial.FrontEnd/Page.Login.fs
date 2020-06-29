namespace WebSharperTutorial.FrontEnd.Pages

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.JQuery
open WebSharper.JavaScript // require by the Remote<'T> type

open WebSharperTutorial.FrontEnd

[<JavaScript>]
module PageLogin =

    type private loginFormTemplate = Templating.Template<"templates/Page.Login.html">

    let private AlertBox (rvStatusMsg:Var<string option>) =
        rvStatusMsg.View
        |> View.Map (fun msgO ->
            match msgO with
            | None ->
                Doc.Empty
            | Some msg ->
                div [ attr.``class`` "alert alert-primary"
                      Attr.Create "role" "alert"
                    ]
                    [ text msg ]
        )
        |> Doc.EmbedView

    let private FormLogin (router:Var<Routes.EndPoint>) =
        let rvEmail = Var.Create ""
        let rvPassword = Var.Create ""
        let rvKeepLogged = Var.Create true
        let rvStatusMsg = Var.Create None

        let statusMsgBox = AlertBox rvStatusMsg

        loginFormTemplate()
            .AlertBox(statusMsgBox)
            .Login(rvEmail)
            .Password(rvPassword)
            .RememberMe(rvKeepLogged)
            .OnLogin(fun _ ->
                JQuery.Of("form").One("submit", fun elem ev -> ev.PreventDefault()).Ignore
                async {
                    let! response =
                        Remote<Auth.RpcUserSession>.CheckCredentials rvEmail.Value rvPassword.Value rvKeepLogged.Value
                    match response with
                    | Result.Ok c ->
                        rvEmail.Value <- ""
                        rvPassword.Value <- ""
                        rvStatusMsg.Value <- None
                        router.Value <- Routes.Listing

                    | Result.Error error ->
                        rvStatusMsg.Value <- Some error
                }
                |> Async.Start
            )
            .OnLogout(fun _ ->
                async {
                    do! Remote<Auth.RpcUserSession>.Logout ()
                    Var.Set router Routes.Home
                }
                |> Async.Start
            )
            .Doc()

    let Main router =
        let formLogin = FormLogin router

        div [ attr.``class`` "container" ]
            [
              div [ attr.``class`` "row" ]
                  [ div [ attr.``class`` "col-xs-12 col-sm-6 mx-auto" ] [ formLogin ]
                  ]
            ]

