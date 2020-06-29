
# Table of Contents

1.  [Chapter 04 - Site Navigation and more about the routing system](#org6104a67)
    1.  [Navigation](#org6a004f6)
        1.  [Navigation bar component](#org7985499)
        2.  [Using the navigation base component](#org468f448)
    2.  [Routing system revisited - client side routing](#org7af9846)
        1.  [Client side routing](#org1830470)


<a id="org6104a67"></a>

# Chapter 04 - Site Navigation and more about the routing system


<a id="org6a004f6"></a>

## Navigation

By now, we have the routing schema for our application built, but we need to add
some UI elements to allow the user to navigate between the pages.

In this section we are going to add a navigation bar to the application. As we
have public and private areas, it would be great to have separed navigation bars
for when the user is authenticated and unauthenticated.

As the Home page can be accessed while authenticated and unauthenticated, it
would be good to replace it accordingly.

So, let's create two versions of the navigation bar.


<a id="org7985499"></a>

### Navigation bar component

This component has only a single item pointing to the Login page.

We are going the build it using only the Doc abstraction without relying on the
template system this time, as this is a short one.

Add a new file to the project named Component.NavigationBar.fs with the
following content:

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

This component is building a Bootstrap Navbar, based on the user authentication
status.

It just ask for the user login and build the private or public items for the
navbar, accordingly.

For each item, we are passing a label and a callback anonymous function. Most of
them are just changing the routing value, except for the Logout one. For the
latter, we must do a remote call to the Auth engine to finish the user session. As
an alternative, we could had created a Endpoint for the logout logic, but this
would require a new route.


<a id="org468f448"></a>

### Using the navigation base component

The next step is to add the component on each page. Edit the Page.Login.fs and
add the component to the resulting content in the Main function:

    ...
    open WebSharperTutorial.FrontEnd
    open WebSharperTutorial.FrontEnd.Components // <-- add this line
        ... 
        let Main router =
            let formLogin = FormLogin router
            let navBar =
                NavigationBar.Main router
    
            [
                navBar
                div [ attr.``class`` "container" ]
                    [
                      div [ attr.``class`` "row" ]
                          [ div [ attr.``class`` "col-xs-12 col-sm-6 mx-auto" ] [ formLogin ]
                          ]
                    ]
            ]
            |> Doc.Concat

Note: as we want to prepend the navigation bar component to the content, we
created a list and use the Doc.Concat to transform this list of Doc into a
single Doc element¹.

Now, you can go ahead and remove the logout button from the login form, if you
want.

Note¹: there is a drawback for this approach. The Doc type has a specialized
type named Elt, which provide several functions and fields. By using Doc.Concat,
we cannot downcast it to Elt type anymore.


<a id="org7af9846"></a>

## Routing system revisited - client side routing

In prior sections, we saw how to setup the routing system for the application.

We started by adding routing support to the server side at first, and later,
adding support to the client side. But it is not working properly, at the
moment.

Let's fix it.


<a id="org1830470"></a>

### Client side routing

The WebSharper Router.InstallRouter creates a Reactive Variable, whose inner
type is the EndPoint, a discriminated union defined in the Routes.fs file.

This Reactive Variable is used to control the EndPoint navigation and can be
changed by just setting its value as follows:

router.Value <- EndPointOption

or 

Var.Set router <- EndPointOption

The router, as a Reactive Variable, provides a field to expose a View and the
client side router relies on it to refresh the DOM content (look for the
Doc.EmbedView line, in the Main.fs file).

But just changing its value won't work, unless we take care of all endpoints.

We are going to refactor the Main.fs file and add two new functions to the
Main.fs file and to help us handling the page's content update, whenever the
router has its value changed.

But before that, let's create a new file named PageHome.fs to better organize
our code:

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

And now, edit the Main.fs file and add the following functions before the Main value:

    ...
    [<JavaScript>]
    let RouteClientPage () =
        let router = Routes.InstallRouter ()
    
        router.View
        |> View.Map (fun endpoint ->
            match endpoint with
            | EndPoint.Home ->
                PageHome.Main router
    
            | EndPoint.Login ->
                PageLogin.Main router
    
            | EndPoint.Listing ->
                div [] [ text "Listing Page - implementation pending" ]
    
            | EndPoint.Form _ ->
                div [] [ text "Form Page - implementation pending" ]
    
            | _ ->
                div [] [ text "implementation pending" ]
        )
        |> Doc.EmbedView
    
    let LoadClientPage ctx title endpoint =
        let body = client <@ RouteClientPage() @>
        MainTemplate ctx endpoint title [ body ]
    
    ...

And this is the new Main value content:

    [<Website>]
    let Main =
        Sitelet.New
            SiteRouter
            (fun ctx endpoint ->
                let loggedUser =
                    async {
                        return! ctx.UserSession.GetLoggedInUser()
                    }
                    |> Async.RunSynchronously
    
                match loggedUser with
                | None -> // user is not authenticated. Allow only public EndPoints
                    match endpoint with
                    | EndPoint.Home ->
                        LoadClientPage ctx "Home" endpoint
    
                    | EndPoint.Login ->
                        LoadClientPage ctx "Login" endpoint
    
                    | EndPoint.AccessDenied ->
                        MainTemplate ctx endpoint "Access Denied Page"
                            [ div [] [ text "Access denied" ] ]
                    | _ ->
                        Content.RedirectTemporary AccessDenied
    
                | Some (u) -> // user is authenticated. Allow all EndPoints
                    match endpoint with
                    | EndPoint.Home ->
                        LoadClientPage ctx "Home" endpoint
    
                    | EndPoint.Login ->
                        LoadClientPage ctx "Login" endpoint
    
                    | EndPoint.Listing ->
                        LoadClientPage ctx "Listing Page" endpoint
    
                    | EndPoint.Form code ->
                        LoadClientPage ctx "Form Page" endpoint
    
                    | _ ->
                        MainTemplate ctx endpoint "not implemented"
                            [ div [] [ text "implementation pending" ] ]
            )

We are done for now. Rebuild the project and test it again.

