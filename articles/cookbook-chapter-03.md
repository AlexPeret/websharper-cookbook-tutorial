- [Chapter 03 - ASP.NET Authentication](#sec-1)
  - [Authentication middleware](#sec-1-1)
  - [Securing the EndPoints](#sec-1-2)
  - [Login and Logout features](#sec-1-3)
    - [The login pages](#sec-1-3-1)

# Chapter 03 - ASP.NET Authentication<a id="sec-1"></a>

WebSharper relies on ASP.NET Form Authentication for security. In this section, we are going to create a page with a form, so the user can provide his credentions to get logged in the application.

Also, we are going to setup the authentication middleware to protect the listing and form EndPoints.

## Authentication middleware<a id="sec-1-1"></a>

Edit the Startup.fs file and add the following entry in the ConfigureServices method:

```fsharp
member this.ConfigureServices(services: IServiceCollection) =
    services.AddSitelet(Site.Main)
        .AddWebSharperRemoting<Auth.RpcUserSession, Auth.RpcUserSessionImpl>() // <-- add this line
        .AddAuthentication("WebSharper")
        .AddCookie("WebSharper", fun options -> ())
    |> ignore

```

Add a new file to the project named Auth.js with the following content:

```fsharp
namespace WebSharperTutorial.FrontEnd

module Auth =
    open System

    open WebSharper
    open WebSharper.Web
    open WebSharper.AspNetCore
    open Microsoft.AspNetCore.Identity

    let GetLoggedInUser () =
        let ctx = Remoting.GetContext()
        ctx.UserSession.GetLoggedInUser()

    [<AbstractClass>]
    type RpcUserSession() =
        [<Rpc>]
        abstract GetLogin : unit -> Async<option<string>>
        [<Rpc>]
        abstract Login : login: string -> Async<unit>
        [<Rpc>]
        abstract Logout : unit -> Async<unit>
        [<Rpc>]
        abstract CheckCredentials : string -> string -> bool -> Async<Result<string,string>>


    [<AllowNullLiteral>]
    type ApplicationUser() =
        inherit IdentityUser()

    //type RpcUserSessionImpl(dbContext: Database.AppDbContext) =
    type RpcUserSessionImpl() =
        inherit RpcUserSession()

        let canGetLogged login password =
            login = "admin" && password = "admin"

        override this.GetLogin() =
            WebSharper.Web.Remoting.GetContext().UserSession.GetLoggedInUser()

        override this.Login(login: string) =
            //Validate email...
            WebSharper.Web.Remoting.GetContext().UserSession.LoginUser(login)

        override this.Logout() =
            WebSharper.Web.Remoting.GetContext().UserSession.Logout()

        override this.CheckCredentials(login:string) (password:string) (keepLogged:bool)
            : Async<Result<string,string>> =
            async {
                if canGetLogged login password then
                    do! WebSharper.Web.Remoting.GetContext().UserSession.LoginUser(login)
                    return Result.Ok "Welcome!"
                else
                    return Result.Error "Invalid credentials."
            }

```

This will require the following packages in your .fsproj file:

```xml
  ...
  <PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" Version="2.2.0" />
  <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="3.1.3" />
</ItemGroup>

```

## Securing the EndPoints<a id="sec-1-2"></a>

Now, we need to protect the EndPoints based on the user credentials.

WebSharper provides the Sitelet.Protect function to secure endpoints, but in this tutorial, we are going to do it manually.

Edit the Main.fs file and replace the Main value by the following one:

```fsharp
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
                    MainTemplate ctx EndPoint.Home "Login Page"
                        [ div [] [ text "Login Page - implementation pending" ] ]
                | EndPoint.AccessDenied ->
                    MainTemplate ctx EndPoint.Home "Access Denied Page"
                        [ div [] [ text "Access denied" ] ]
                | _ ->
                    Content.RedirectTemporary AccessDenied

            | Some (u) -> // user is authenticated. Allow all EndPoints
                match endpoint with
                | EndPoint.Home -> HomePage ctx
                | EndPoint.Login ->
                    MainTemplate ctx EndPoint.Home "Login Page"
                        [ div [] [ text "Login Page - implementation pending" ] ]
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

```

This is pretty straightforward. We are allowing all endpoints to the authenticated user and allowing a few endpoints for the not authenticated one.

## Login and Logout features<a id="sec-1-3"></a>

Next, we need to build the login page and add a temporary logout button into it for the sake of testing. In later section, we are going to move the logout button to a navbar at the header.

### The login pages<a id="sec-1-3-1"></a>

Add two new files to the project:

-   Page.Login.fs
-   templates/Page.Login.html

Remember to add both references to the .fsproj file.

The HTML page will have the following layout:

```html
<div class="p-md-5">
    <div class="text-center">
        <h1 class="h4 text-gray-900 mb-4">Provide your credentials</h1>
    </div>

    <replace ws-replace="AlertBox"></replace>

    <form class="user">
        <div class="form-group">
            <input type="email" class="form-control form-control-user" placeholder="Login (admin)" ws-var="Login">
        </div>
        <div class="form-group">
            <input type="password" class="form-control form-control-user" placeholder="Password (admin)" ws-var="Password">
        </div>
        <div class="form-group">
            <div class="custom-control custom-checkbox small">
                <input type="checkbox" class="custom-control-input" id="login-remember" ws-var="RememberMe">
                <label class="custom-control-label" for="login-remember">Get me logged!</label>
            </div>
        </div>
        <a href="javascript:void(0)" class="btn btn-primary btn-user btn-block" ws-onclick="OnLogin">
            Login
        </a>
        <a href="javascript:void(0)" class="btn btn-primary btn-user btn-block" ws-onclick="OnLogout">
            Logout
        </a>
    </form>
</div>

```

And this is the code for the Page.Login.fs file:

```fsharp
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

```

This code has a lot of WebSharper features that I want to highlight.

First, we are referencing the HTML template as we did before for the main HTML template.

WebSharper template system can transform an HTML file into the Doc abstraction. This is a great feature as it allows for composition, as you can see in the Main function.

Also, the template system can tie the Reactive Variables to the ws-var holes, making it possible to synchronize the value from the Reactive Variable with the respective DOM element.

Another cool feature is regarding the DOM element event handler. As you see, the template system provides a function for each ws-on\* attribute in the HTML template, so you can deal with the client events (refer to the OnLogin and OnLogout functions in the code).

There are two more features the worth highlighting. First one is regarded to the JQuery call. WebSharper has an extension system which provides bindings to existing Javascript libraries (although, the JQuery is built-in into the WebSharper's core), through the WIG language.

The second one, is the View. The AlertBox function derives a Doc abstraction based on the current state of the rvStatusMsg parameter, a Reactive Variable.

Reactive Variables has a inner property to expose a View from it, which will change whenever the Reactive Variable's content change. A View is intended to be used at the DOM. In the AlertBox function, we are build the HTML dynamically, according to the rvStatusMsg content.

Finally, change the Main.fs again to load this page:

```fsharp
...
module Site =
    open WebSharper.UI.Html
    open WebSharper.UI.Client // required by the Doc.EmbedView
    open WebSharperTutorial.FrontEnd.Routes
    open WebSharperTutorial.FrontEnd.Pages // <-- add this line
    ...
    // add a new function to render the login page
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
    ...

    // replace all EndPoint.Login blocks by the following one
        | EndPoint.Login ->
            LoginPage ctx endpoint
    ...

```

The client function will render WebSharper.UI.Client code at the server side.

In the block above, we are installing the router and passing it to the PageLogin. If you check the PageLogin code again, you will notice the [<JavaScript>] attribute at the module level. This is required whenever you are using the client function.

Now, build the project again and load the /login page at the bar address to test the page. Use admin/admin as login and password.

After getting logged, you will notice the URL address will be replace by the /private/listing one, while the page content remains the same. But if you reload the page, you might see the Listing page content.

This happens because we didn't installed the router for the Listing EndPoint. We are going to fix that in the coming sections.

By the way, test the logout button at the Login page, as well.

| [previous](./cookbook-chapter-02.md) | [up](../README.md) | [next](./cookbook-chapter-04.md) |
