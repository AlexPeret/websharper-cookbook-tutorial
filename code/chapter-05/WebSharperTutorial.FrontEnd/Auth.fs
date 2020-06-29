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
