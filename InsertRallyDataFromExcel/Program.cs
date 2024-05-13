using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

class Program
{
    static void Main(string[] args)
    {
        using (var dbContext = new YourDbContext())
        {
            // Read Excel file and insert rows into the database
            using (var workbook = new XLWorkbook("/Users/tobiaskjaer/Dropbox/Mac (3)/Desktop/ObstaclesDb.xlsx"))
            {
                var worksheet = workbook.Worksheet("Obstacles");
                var rows = worksheet.RowsUsed().Skip(1); // Skip header row
                foreach (var row in rows)
                {
                    string name = row.Cell(3).Value.ToString();
                    string description = row.Cell(4).Value.ToString();
                    string signFilename = row.Cell(5).Value.ToString();
                    int categoryId = int.Parse(row.Cell(6).Value.ToString());

                    // Insert data into the database
                    var obstacle = new Obstacle
                    {
                        Name = name,
                        Description = description,
                        CategoryId = categoryId
                        // Set SignUrl and ImageData to null for now
                    };

                    dbContext.Obstacles.Add(obstacle);
                }

                dbContext.SaveChanges(); // Save changes to insert rows into the database
            }

            // Iterate through the inserted records to insert images
            foreach (var obstacle in dbContext.Obstacles)
            {
                foreach (string filePath in  Directory.GetFiles(@"/Users/tobiaskjaer/Repoes/Sign"))
                {
                    byte[] bytes = File.ReadAllBytes(filePath);
                    string file = Convert.ToBase64String(bytes);
                    obstacle.SignUrl = file;
                }
            }

            dbContext.SaveChanges(); // Save changes to insert images into the database
        }
    }
}

public class YourDbContext : DbContext
{
    public DbSet<Obstacle> Obstacles { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configure your database connection string here
        optionsBuilder.UseSqlServer("Server=10.56.8.36; Database=DB_F23_TEAM_13; User Id=DB_F23_TEAM_13; Password=TEAMDB_DB_13; TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Define your model configuration here if needed
        modelBuilder.Entity<Obstacle>()
            .HasOne(o => o.Category)
            .WithMany()
            .HasForeignKey(o => o.CategoryId)
            .OnDelete(DeleteBehavior.Cascade); // Or any other delete behavior you prefer
    }
}

public class Obstacle
{
    public int ObstacleId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string SignUrl { get; set; } // Path to the image file
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; }
}


