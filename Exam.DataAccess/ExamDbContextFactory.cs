using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Exam.DataAccess
{
    public class ExamDbContextFactory : IDesignTimeDbContextFactory<ExamDbContext>
    {
        public ExamDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ExamDbContext>();

            const string connectionString =
                "Server=.;Database=Exam;Trusted_Connection=True;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString);

            return new ExamDbContext(optionsBuilder.Options);
        }
    }
}
