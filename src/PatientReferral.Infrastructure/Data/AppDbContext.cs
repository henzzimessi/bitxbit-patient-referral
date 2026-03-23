using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PatientReferral.Application.Entities;

namespace PatientReferral.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Referral> Referrals => Set<Referral>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            d => d.ToDateTime(TimeOnly.MinValue),
            d => DateOnly.FromDateTime(d));

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(p => p.PatientId);
            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.DateOfBirth).HasConversion(dateOnlyConverter).HasColumnType("date");
            entity.Property(p => p.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Referral>(entity =>
        {
            entity.HasKey(r => r.ReferralId);
            entity.Property(r => r.ReferralSource).IsRequired().HasMaxLength(200);
            entity.Property(r => r.ReferralType).IsRequired().HasMaxLength(100);
            entity.Property(r => r.ReferralNote).HasColumnType("nvarchar(max)");
            entity.Property(r => r.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(r => r.Patient)
                .WithMany(p => p.Referrals)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
