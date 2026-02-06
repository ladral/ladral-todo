using Ladral.ToDo.WebApp.Bff.Components;
using Ladral.ToDo.WebApp.Bff.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add all services
builder.Services.AddWebAppServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Configure Blazor
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Ladral.ToDo.WebApp.Client._Imports).Assembly);


// Configure BFF endpoints
app.MapBffEndpoints();
app.MapReverseProxy();

app.Run();