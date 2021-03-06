- [Chapter 05 - The listing page](#sec-1)

# TODO Chapter 05 - The listing page<a id="sec-1"></a>

We are almost done. We have all the logic for routing, navigation and authentication put together. Now, it is time to create a few pages to emulate a data driven application.

The first task is to build a listing page. This page will make a remote call to the server to get some data and render it as a HTML table.

For each line, we are going to add a link to redirect the page to the form passing the respective record's code, so we can open it from there.

![img](./images/cookbook-chapter-05-image-01.png "The listing page")

Add two new files to the project:

-   templates/Page.Listing.html
-   Page.Listing.fs

For the template one, add the following code snippet:

```html
<table class="table table-striped">
    <thead>
        <tr>
            <th scope="col">#</th>
            <th scope="col">First</th>
            <th scope="col">Last</th>
            <th scope="col">Updated At</th>
        </tr>
    </thead>
    <tbody ws-hole="Rows">
        <tr ws-template="RowTemplate">
            <th scope="row">${Code}</th>
            <td><a class="btn btn-link" href="javascript:void(0)" ws-onclick="OnEdit">${Firstname}</a></td>
            <td>${Lastname}</td>
            <td>${UpdatedAt}</td>
        </tr>
    </tbody>
</table>

```

This snippet introduces a new attribute named `ws-template`. This attribute defines a inner template, which can be instantiated in F# code and composed with *Doc* elements as usual.

For the ***Page.Listing.fs*** file:

```fsharp
namespace WebSharperTutorial.FrontEnd.Pages

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html

open WebSharperTutorial.FrontEnd
open WebSharperTutorial.FrontEnd.Components

[<JavaScript>]
module PageListing =

    type private listingTemplate = Templating.Template<"templates/Page.Listing.html">

    let private buildTable router (users:DTO.User list) =
        let tableRows =
            users
            |> List.map(fun user ->
                listingTemplate.RowTemplate()
                    .Code(string user.Code)
                    .Firstname(user.Firstname)
                    .Lastname(user.Lastname)
                    .UpdatedAt(user.UpdateDate.ToShortDateString())
                    .OnEdit(fun _ -> Var.Set router (Routes.Form user.Code))
                    .Doc()
            )

        listingTemplate()
            .Rows(tableRows)
            .Doc()

    let Main router =
        async {
            let navBar =
                NavigationBar.Main router

            let! users =
                Server.GetUsers()

            let tableElement =
                buildTable router users

            return
                [
                    navBar
                    div [ attr.``class`` "container" ]
                        [
                          div [ attr.``class`` "row" ]
                              [ div [ attr.``class`` "col-12" ]
                                    [ tableElement ]
                              ]
                        ]
                ]
                |> Doc.Concat
        }
        |> Doc.Async
```

This page is getting data from the server through a RPC request and is rendering a table with its content.

For each line, it instantiate the `listingTemplate.RowTemplate` class and fill in its hole, while setup the `OnEdit` event, aswell.

The RPC function is asynchrounous, than it requires the `async` computation expression. The returning result is passed to the `Doc.Async` function which will start the async block and return a *Doc* abstraction as the final result.

For the Server side logic, let's add two new files to the project:

-   DTO.fs
-   Server.fs

as following:

```fsharp
namespace WebSharperTutorial.FrontEnd

open System

open WebSharper

[<JavaScript>]
module DTO =

    type User = {
        Code: int64
        Firstname: string
        Lastname: string
        UpdateDate: DateTime
    }

    let CreateUser code firstname lastname updateDate =
        {
            Code = code
            Firstname = firstname
            Lastname = lastname
            UpdateDate = updateDate
        }

```

This file contains the *Data Transfer Object* (DTO) types used to send and receive data between client and server side. The important aspect here is the use of `[<JavaScript>]` attribute, so the WebSharper compiler can transpile it to *Javascript*.

And for the ***Server.fs*** file:

```fsharp
namespace WebSharperTutorial.FrontEnd

open System

open WebSharper

open WebSharperTutorial.FrontEnd
open WebSharperTutorial.FrontEnd.DTO

module Server =

    let private dbUsers () =
        [
            CreateUser 1L "Firstname 1" "Lastname 1" (new DateTime(2020,3,17))
            CreateUser 2L "Firstname 2" "Lastname 2" (new DateTime(2019,6,21))
            CreateUser 3L "Firstname 3" "Lastname 3" (new DateTime(2019,8,14))
        ]

    [<Rpc>]
    let GetUsers () : Async<User list> =
        async {
            return dbUsers()
        }

```

We are generating dummy data for testing. The `RPC` attribute instructs *WebSharper*'s compiler to create all the RPC logic for this asynchrounous function, handling the conversion of its result to JSON.

Finally, edit the ***Main.fs*** file and reference the new listing page:

```fsharp
...
[<JavaScript>]
let RouteClientPage () =
    let router = Routes.InstallRouter ()

    router.View
    |> View.Map (fun endpoint ->
        match endpoint with
        ...
        | EndPoint.Listing ->
            PageListing.Main router // <-- replaced line
        ...
```

| [previous](./cookbook-chapter-04.md) | [up](../README.md) | [Chapter 06 - The form page](./cookbook-chapter-06.md) |
