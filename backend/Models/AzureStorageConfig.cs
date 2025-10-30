namespace RecipeAPI.Models
{
    public class AzureStorageConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string ContainerName { get; set; } = "recipes";
        public string PublicContainerName { get; set; } = "public-recipes";
    }
}