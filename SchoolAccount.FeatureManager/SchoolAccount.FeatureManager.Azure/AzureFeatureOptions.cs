namespace SchoolAccount.FeatureManager.Azure;

public sealed class AzureFeatureOptions
{
    public string DefaultKeyPrefix { get; set; } = ".appconfig.featureflag/";
    
    public required string ConnectionString { get; set; }
    
    public string? Label { get; set; }
    
    public TimeSpan? CacheTimeout { get; set; }
}