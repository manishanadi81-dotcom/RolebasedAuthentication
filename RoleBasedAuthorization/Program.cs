using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoleBasedAuthorization.Data;
using RoleBasedAuthorization.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization();
builder.Services.AddSession();

//DbContext
builder.Services.AddDbContext<AppDbContext>(item => item.UseSqlServer(builder.Configuration.GetConnectionString("str")));

//Identity Setup
builder.Services.AddIdentity<Users, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

// Configures authentication cookie settings for user login and access denied redirection.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Create Scope for Access Services
using(var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<Users>>();

        // Creating Roles
        string[] arr = { "Admin", "User","SuperAdmin" };
        foreach(var r in arr)
        {
            var roleExists = await roleManager.RoleExistsAsync(r);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole(r));
            }
        }
        string suName = "CEO";
        string suEmail = "CEO@gmail.com";
        string suPassword = "StrongCEO077@";
        // 3. Check BOTH Email and Username to prevent duplicate conflicts
        var userByEmail = await userManager.FindByEmailAsync(suEmail);
        var userByName = await userManager.FindByNameAsync(suName);
        if (userByEmail == null && userByName == null)
        {
            Users superUser = new Users()
            {
                UserName = suName,
                Email = suEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(superUser, suPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superUser, "SuperAdmin");
            }
            else
            {
                // This will print the exact reason to your console/logs if it fails again
                var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create seed user. Errors: {errorMessages}");
            }
        }
        // Create User
        string Name = "Head";
        string UserEmail = "Rushi@gmail.com";
        string Password = "Rushi81@";
        var userExists = await userManager.FindByEmailAsync(UserEmail);
        if (userExists == null)
        {
            Users u = new Users()
            {
                UserName = Name,
                Email = UserEmail,
                EmailConfirmed = true
            };
            var res = await userManager.CreateAsync(u,Password);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(u, "Admin");   // Assiging role to user
            }
        }
        string Emp_Name = "Employee";
        string Emp_Email = "Emeployee@gmail.com";
        string Emp_Password = "Pass81@";
        var EmpExists = await userManager.FindByEmailAsync(Emp_Email);
        if(EmpExists == null)
        {
            Users u = new Users()
            {
                UserName = Emp_Name,
                Email = Emp_Email,
                EmailConfirmed = true
            };
            var res = await userManager.CreateAsync(u, Emp_Password);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(u, "User");
            }
        }
    }
    catch(Exception ex)
    {
        Console.WriteLine("Unable to Create Admin user" + ex.ToString());
    }
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
