using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            // Lấy chuỗi kết nối SQL Server từ appsettings.json
            var connectionString = builder.Configuration
                .GetConnectionString("bài_tập_1Context")
                ?? throw new InvalidOperationException(
                    "Connection string 'bài_tập_1Context' not found.");

            // Đăng ký DbContext và sử dụng SQL Server
            builder.Services.AddDbContext<bài_tập_1Context>(options =>
                options.UseSqlServer(connectionString));

            // Đăng ký Identity
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<bài_tập_1Context>()
            .AddDefaultTokenProviders();

            // Lưu khóa Data Protection vào thư mục .keys
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(
                    new DirectoryInfo(
                        Path.Combine(
                            builder.Environment.ContentRootPath,
                            ".keys")));

            // Cấu hình đường dẫn đăng nhập và từ chối truy cập
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // Đăng ký MVC
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Cấu hình pipeline xử lý request
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Authentication phải đứng trước Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Tự động cập nhật database, tạo role và tài khoản Admin
            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var context = serviceProvider
                    .GetRequiredService<bài_tập_1Context>();

                var roleManager = serviceProvider
                    .GetRequiredService<RoleManager<IdentityRole>>();

                var userManager = serviceProvider
                    .GetRequiredService<UserManager<IdentityUser>>();

                var logger = serviceProvider
                    .GetRequiredService<ILogger<Program>>();

                // Tự động áp dụng các migration chưa chạy
                await context.Database.MigrateAsync();

                // Tạo các role nếu chưa tồn tại
                string[] roles =
                {
                    "Admin",
                    "NhanVien",
                    "DocGia"
                };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var roleResult = await roleManager.CreateAsync(
                            new IdentityRole(role));

                        if (!roleResult.Succeeded)
                        {
                            var errors = string.Join(
                                "; ",
                                roleResult.Errors.Select(
                                    error => error.Description));

                            logger.LogError(
                                "Không thể tạo role {Role}: {Errors}",
                                role,
                                errors);
                        }
                    }
                }

                var adminEmail = builder.Configuration[
                        "AdminAccount:Email"]?
                    .Trim()
                    .ToLowerInvariant();

                var adminPassword = builder.Configuration[
                    "AdminAccount:Password"];

                if (string.IsNullOrWhiteSpace(adminEmail))
                {
                    logger.LogWarning(
                        "Chưa cấu hình AdminAccount:Email; " +
                        "bỏ qua việc tạo tài khoản Admin.");
                }
                else
                {
                    var admin = await userManager
                        .FindByEmailAsync(adminEmail);

                    // Chỉ tạo tài khoản khi chưa tồn tại
                    if (admin == null)
                    {
                        if (string.IsNullOrWhiteSpace(adminPassword))
                        {
                            logger.LogWarning(
                                "Chưa có tài khoản Admin và chưa cấu hình " +
                                "AdminAccount:Password. " +
                                "Hãy dùng User Secrets hoặc biến môi trường " +
                                "AdminAccount__Password.");
                        }
                        else
                        {
                            admin = new IdentityUser
                            {
                                UserName = adminEmail,
                                Email = adminEmail,
                                EmailConfirmed = true
                            };

                            var createResult = await userManager
                                .CreateAsync(admin, adminPassword);

                            if (!createResult.Succeeded)
                            {
                                var errors = string.Join(
                                    "; ",
                                    createResult.Errors.Select(
                                        error => error.Description));

                                logger.LogError(
                                    "Không thể tạo tài khoản Admin: {Errors}",
                                    errors);

                                admin = null;
                            }
                        }
                    }

                    if (admin != null)
                    {
                        // Gán role Admin nếu tài khoản chưa có role này
                        if (!await userManager.IsInRoleAsync(admin, "Admin"))
                        {
                            var addRoleResult = await userManager
                                .AddToRoleAsync(admin, "Admin");

                            if (!addRoleResult.Succeeded)
                            {
                                var errors = string.Join(
                                    "; ",
                                    addRoleResult.Errors.Select(
                                        error => error.Description));

                                logger.LogError(
                                    "Không thể gán role Admin: {Errors}",
                                    errors);
                            }
                        }

                        // Tạo hồ sơ nhân viên cho Admin nếu chưa có
                        var adminProfileExists = await context.NhanVien
                            .AnyAsync(n => n.UserId == admin.Id);

                        if (!adminProfileExists)
                        {
                            context.NhanVien.Add(
                                new Models.NhanVien
                                {
                                    UserId = admin.Id,
                                    HoTen = "Quản trị viên",
                                    GioiTinh = string.Empty,
                                    DiaChi = string.Empty,
                                    SoDienThoai =
                                        admin.PhoneNumber ?? string.Empty,
                                    Email =
                                        admin.Email ?? adminEmail,
                                    ChucVu = "Quản trị viên",
                                    NgayVaoLam = DateTime.Today
                                });

                            await context.SaveChangesAsync();

                            logger.LogInformation(
                                "Đã tạo hồ sơ nhân viên cho Admin.");
                        }
                    }
                }
            }

            app.Run();
        }
    }
}

