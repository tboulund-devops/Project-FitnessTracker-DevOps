namespace Backend.Application.Service.Interfaces;

public interface IDatabaseSeedingService
{
    public void Seed();
    public void SeedTables();
    public void SeedTestData();
}