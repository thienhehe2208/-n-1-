using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using bài_tập_1.Data;

namespace bài_tập_1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
            var connectionString = builder.Configuration.GetConnectionString("bài_tập_1Context")
                ?? throw new InvalidOperationException("Connection string 'bài_tập_1Context' not found.");

            builder.Services.AddDbContext<bài_tập_1Context>(options =>
            {
                if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                    options.UseSqlServer(connectionString);
                else
                    options.UseSqlite(connectionString);
            });

            // ---- ĐĂNG KÝ Identity ----
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<bài_tập_1Context>()
            .AddDefaultTokenProviders();

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(
                    Path.Combine(builder.Environment.ContentRootPath, ".keys")));

            // ---- CẤU HÌNH COOKIE PATH ----
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();   // ---- phải đặt TRƯỚC UseAuthorization ----
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // ---- THÊM MỚI: Seed 3 Role, đặt ngay trước app.Run() ----
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<bài_tập_1Context>();
                if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                    await context.Database.MigrateAsync();
                else
                    await context.Database.EnsureCreatedAsync();
                await DatabaseSeeder.SeedLibraryDataAsync(context);

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roles = { "Admin", "NhanVien", "DocGia" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            // -----------------------------------------------------------
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                string adminEmail = "admin@thuvien.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                    await userManager.CreateAsync(admin, "Admin@123");
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
            app.Run();
        }
    }
}
