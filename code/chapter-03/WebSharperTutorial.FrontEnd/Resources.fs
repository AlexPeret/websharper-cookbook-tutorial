namespace WebSharperTutorial.FrontEnd

open System
open WebSharper
open WebSharper.Resources

module AppResources =

    module Bootstrap =
        [<Require(typeof<JQuery.Resources.JQuery>)>]
        type Js() =
            inherit BaseResource("/vendor/bootstrap/js/bootstrap.bundle.min.js")
        type Css() =
            inherit BaseResource("/vendor/bootstrap/css/bootstrap.min.css")

    module FrontEndApp =
        type Css() =
            inherit BaseResource("/app/css/common.css")

        type Js() =
            inherit BaseResource("/app/js/common.js")

    [<assembly:Require(typeof<Bootstrap.Js>);
      assembly:Require(typeof<Bootstrap.Css>);
      assembly:Require(typeof<FrontEndApp.Css>);
      assembly:Require(typeof<FrontEndApp.Js>);
      >]
    do()
