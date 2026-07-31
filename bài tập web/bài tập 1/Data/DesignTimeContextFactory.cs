using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace bài_tập_1.Data
{
    public class DesignTimeContextFactory :
        IDesignTimeDbContextFactory<bài_tập_1Context>
    {
        public bài_tập_1Context CreateDbContext(string[] args)
        {
            const string connectionString =
                "Server=(localdb)\\MSSQLLocalDB;" +
                "Database=QuanLyThuVien;" +
                "Trusted_Connection=True;" +
                "MultipleActiveResultSets=true;" +
                "TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<bài_tập_1Context>()
                .UseSqlServer(connectionString)
                .Options;

            return new bài_tập_1Context(options);
        }
    }
}
