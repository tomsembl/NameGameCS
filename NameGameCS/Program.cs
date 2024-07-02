using NameGameCS;
using NameGameCS.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<NameGameDbContext>();
builder.Services.AddScoped<DBLogic>();
builder.Services.AddScoped<UserAuthenticationFilter>();
builder.Services.AddSingleton<SignalRHub>();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddMvc(options => {
    // If you decide to apply the filter globally, you can do so here
    // options.Filters.Add(typeof(UserAuthenticationFilter));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home}/{id?}"
);

app.UseEndpoints(endpoints => {
    endpoints.MapHub<SignalRHub>("/SignalRHub");
});

app.Run();
