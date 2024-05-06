using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
var builder = WebApplication.CreateBuilder();
var people = new List<Person>
{
    new Person("aaa", "111", "Admin"),
    new Person("bbb", "222", "User")
};
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => options.LoginPath = "/login");
builder.Services.AddAuthorization();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/login", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    string loginForm = @"<!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8' />
        <title>Form</title>
    </head>
    <body>
        <h2>Login Form</h2>
        <form method='post'>
            <p>
                <label>Email</label><br />
                <input name='email' />
            </p>
            <p>
                <label>Password</label><br />
                <input type='password' name='password' />
            </p>
            <input type='submit' value='Login' />
        </form>
    </body>
    </html>";
    await context.Response.WriteAsync(loginForm);
});
app.MapPost("/login", async (string? returnUrl, HttpContext context) =>
{
    var form = context.Request.Form;
    if (!form.ContainsKey("email") || !form.ContainsKey("password"))
        return Results.BadRequest("Email и/или пароль не установлены");

    string email = form["email"];
    string password = form["password"];

    if (people.Count(p => p.Email == email && p.Password == password) == 0) return Results.Redirect("/login");

    Person? person = people.FirstOrDefault(p => p.Email == email && p.Password == password);
    var claims = new List<Claim> { new Claim(ClaimTypes.Name, person.Email), new Claim(ClaimTypes.Role, person.Role) };
    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");

    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    if (person.Role == "Admin") return Results.Redirect("/p1");
    return Results.Redirect("/p2");
});
app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});
app.Map("/", [Authorize] () => $"Hello World!");
app.Map("/p1", [Authorize(Roles = "Admin")] () => $"Hello Admin!");
app.Map("/p2", [Authorize(Roles = "User")] () => $"Hello User!");
app.Run();
record class Person(string Email, string Password, string Role);
