namespace WebSharperTutorial.FrontEnd.Pages

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.JavaScript

open WebSharperTutorial.FrontEnd
open WebSharperTutorial.FrontEnd.Components

[<JavaScript>]
module PageForm =

    type private formTemplate = Templating.Template<"templates/Page.Form.html">

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

    let private spinner msg =
      div
        [ attr.``class`` "spinner-border text-warning"
          Attr.Create "role" "status"
        ]
        [ span [ attr.``class`` "sr-only" ] [ text msg ]
        ]

    let private frameContent navBar content =
        [
            navBar
            div [ attr.``class`` "container" ]
                [
                  div [ attr.``class`` "row" ]
                      [ div [ attr.``class`` "col-12" ]
                            [ content ]
                      ]
                ]
        ]
        |> Doc.Concat

    let Main router code =
        let rvStatusMsg = Var.Create None
        let statusMsgBox = AlertBox rvStatusMsg

        let rvModel = Var.CreateWaiting<DTO.User>()
        let submitter =
            Submitter.CreateOption<DTO.User> rvModel.View

        let loadModel() =
            async {
                let! modelR =
                    Server.GetUser code

                match modelR with
                | Error error ->
                    Var.Set rvStatusMsg (Some error)

                | Ok model ->
                    Var.Set rvModel model
                    submitter.Trigger()

                return ()
            }

        let navBar =
            NavigationBar.Main router

        let content =
            submitter.View
            |> View.Map (fun modelO ->
                match modelO with
                | None -> spinner "loading..."
                | Some model ->
                    let rvCode =
                         rvModel.Lens
                             (fun model -> string model.Code)
                             (fun model value -> { model with Code = int64 value })

                    formTemplate()
                        .AlertBox(statusMsgBox)
                        .Code(rvCode)
                        .Firstname(Lens(rvModel.V.Firstname))
                        .Lastname(Lens(rvModel.V.Lastname))
                        .UpdatedAt(model.UpdateDate.ToShortDateString())
                        .OnSave(fun evt ->
                            async {
                                let! modelR =
                                    Server.SaveUser rvModel.Value

                                match modelR with
                                | Error error ->
                                    Var.Set rvStatusMsg (Some error)

                                | Ok model ->
                                    Var.Set rvModel model
                                    Var.Set rvStatusMsg (Some "Saved!")
                                    submitter.Trigger()
                            }
                            |> Async.Start
                      )
                      .OnBack(fun _ ->
                          Var.Set router Routes.Listing
                      )
                      .Doc()

            )
            |> Doc.EmbedView

        loadModel()
        |> Async.Start

        frameContent navBar content

