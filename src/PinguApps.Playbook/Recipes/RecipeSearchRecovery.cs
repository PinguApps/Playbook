namespace PinguApps.Playbook.Recipes;

internal sealed record RecipeSearchRecovery
{
    public required string Reason { get; init; }
    public required string Message { get; init; }
    public required RecipeSearchFilters Filters { get; init; }
    public required RecipeSearchRequest? SuggestedRequest { get; init; }
}
