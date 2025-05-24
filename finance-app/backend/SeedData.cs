using Backend.Models;
using System.Globalization;
using CsvHelper;

public static class SeedData
{
    public static void SeedEmployeesFromCsv(FinanceAppDbContext context, string csvPath)
    {
        if (context.Employees.Any()) return; // Zaten veri varsa atla

        using (var reader = new StreamReader(csvPath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var employees = csv.GetRecords<Employee>().ToList();
            context.Employees.AddRange(employees);
            context.SaveChanges();
        }
    }
}