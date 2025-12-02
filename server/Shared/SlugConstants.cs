namespace RSPWebAPI.Shared
{
  public static class SlugConstants
  {
    public static class Messages
    {
      public const string UserIdNotFoundInToken = "User ID not found in token";
      public const string RandomSlugGeneratedSuccessfully = "Random slug generated successfully";
      public const string SlugAlreadyTaken = "This slug is already taken by another user.";
      public const string FailedToGenerateRandomSlug = "Failed to generate random slug";
    }
    
    public static class Validation
    {
      public const string SlugRequired = "Slug is required";
      public const string SeasonSlugRequired = "Season slug is required";
    }
  }
}