﻿using ActivityControl.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ActivityControl.DataContext.Context;

public class ActivityControlContext : IdentityDbContext<IdentityUser>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ActivityControlContext(DbContextOptions<ActivityControlContext> options, IHttpContextAccessor httpContextAccessor) : base(options) 
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public DbSet<Activity> Activitys { get; set; }
    public DbSet<HoursUsed> HoursUseds { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        #region Activity
        builder.Entity<Activity>()
                .HasKey(x => x.Id);

        builder.Entity<Activity>()
            .Property(x => x.Description)
            .IsRequired()
            .HasColumnType("varchar(200)");

        builder.Entity<Activity>()
            .Property(x => x.Observations)
            .IsRequired()
            .HasColumnType("varchar(500)");

        builder.Entity<Activity>()
            .ToTable("Atividades");
        #endregion

        #region HoursUsed
        builder.Entity<HoursUsed>()
                .HasKey(x => x.Id);

        builder.Entity<HoursUsed>()
            .Property(x => x.Start)
            .IsRequired()
            .HasColumnType("varchar(200)");

        builder.Entity<HoursUsed>()
            .Property(x => x.End)
            .IsRequired()
            .HasColumnType("varchar(500)");

        builder.Entity<HoursUsed>()
            .ToTable("HorasUtilizadas");
        #endregion

        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var history in this.ChangeTracker.Entries()
            .Where(e => e.Entity is BaseModel && (e.State == EntityState.Added))
            .Select(e => e.Entity as BaseModel)
            )
        {
            history.CriadoPor = _httpContextAccessor.HttpContext.User.Identity.Name;
            history.CriadoEm = DateTime.Now;
            history.AlteradoPor = _httpContextAccessor.HttpContext.User.Identity.Name;
            history.AlteradoEm = DateTime.Now;            
        }

        foreach (var history in this.ChangeTracker.Entries()
                                        .Where(e => e.Entity is BaseModel && e.State == EntityState.Modified)
                                        .Select(e => e.Entity as BaseModel))
        {
            history.AlteradoPor = _httpContextAccessor.HttpContext.User.Identity.Name;
            history.AlteradoEm = DateTime.Now;
        }

        return await base.SaveChangesAsync();
    }
}
