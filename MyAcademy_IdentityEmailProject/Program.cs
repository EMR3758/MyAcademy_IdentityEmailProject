using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyAcademy_IdentityEmailProject.Context;
using MyAcademy_IdentityEmailProject.Entities;
using MyAcademy_IdentityEmailProject.Services;
using MyAcademy_IdentityEmailProject.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EmailContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity: UserManager<AppUser>, SignInManager<AppUser> ve "Identity.Application" cookie şemasını kaydeder
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Aynı e-postayla ikinci hesap açılmasını engeller (FindByEmailAsync tek kayıt bekler)
    options.User.RequireUniqueEmail = true;

    // E-postası doğrulanmamış kullanıcı giriş yapamaz → PasswordSignInAsync sonucu IsNotAllowed döner
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<EmailContext>()
.AddDefaultTokenProviders();

// "Smtp" bölümünü SmtpSettings sınıfına bağla (gerçek şifre User Secrets'ta)
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login/SignIn";
    options.AccessDeniedPath = "/ErrorPages/Page403";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

