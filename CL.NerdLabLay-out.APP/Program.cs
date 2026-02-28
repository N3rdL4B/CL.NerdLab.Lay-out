using CL.NerdLabLay_out.APP.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Login/Login", "/login");
});

// Registrar HttpClient y ApiService
builder.Services.AddHttpClient<ApiService>();

// Configurar Session para guardar el Rol del usuario logueado
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(12); // ¡12 horas de sesión!
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor(); // <--- AGREGA ESTA LÍNEA AQUÍ

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Activar Session

app.UseAuthorization();

app.MapGet("/", context =>
{
    context.Response.Redirect("/login");
    return Task.CompletedTask;
});

app.MapRazorPages();

app.Run();