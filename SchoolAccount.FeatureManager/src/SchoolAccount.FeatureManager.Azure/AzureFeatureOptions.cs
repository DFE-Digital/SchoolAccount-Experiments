namespace SchoolAccount.FeatureManager.Azure;

public sealed class AzureFeatureOptions
{
    public required string SourceName { get; set; } = "Azure";
    
    public required string ConnectionString { get; set; }
    
    public string DefaultKeyPrefix { get; set; } = ".appconfig.featureflag/";
    
    public string? Group { get; set; }
    
    public TimeSpan? CacheTimeout { get; set; }
}