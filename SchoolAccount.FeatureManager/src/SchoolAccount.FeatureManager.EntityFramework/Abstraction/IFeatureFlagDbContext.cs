using Microsoft.EntityFrameworkCore;
using SchoolAccount.FeatureManager.EntityFramework.Entities;

namespace SchoolAccount.FeatureManager.EntityFramework.Abstraction;

public interface IFeatureFlagDbContext
{
    DbSet<FeatureFlagEntity> FeatureFlags { get; }
}