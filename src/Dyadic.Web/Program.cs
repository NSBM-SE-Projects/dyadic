using Dyadic.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Dyadic.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Dyadic.Infrastructure.Data;
using Dyadic.Application.Services;
using Dyadic.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<ISupervisorProfileService, SupervisorProfileService>();
builder.Services.AddScoped<IAdminService, AdminService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
using (var scope = app.Services.CreateScope()) {
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

app.UseAuthorization();

app.MapRazorPages();

await app.RunAsync();
