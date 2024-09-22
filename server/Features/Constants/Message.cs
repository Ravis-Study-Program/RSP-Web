namespace RSPWebAPI.Features.Constants;

public static class Message
{
  public const string UserEmailExists = "A user with this email already exists.";
  public const string UserEmailDoesNotExists = "No user found using the email provided.";
  public const string UserCreatedSuccessfully = "User created successfully.";
  public const string UserCreationUnexpectedError = "An unexpected error has occurred during user creation.";
  public const string UserUpdateUnexpectedError = "An unexpected error has occurred during user update.";
  public const string UserDeletionUnexpectedError = "An unexpected error has occurred during user deletion.";
  public const string UserUpdatedSuccessfully = "User updated successfully.";
  public const string UserDeletedSuccessfully = "User deleted successfully.";

  public const string SeasonExists = "A season with this SeasonId already exists.";
  public const string SeasonDoesNotExists = "No season found using the SeasonId provided.";
  public const string SeasonCreatedSuccessfully = "Season created successfully.";
  public const string SeasonCreationUnexpectedError = "An unexpected error has occurred during season creation.";
  public const string SeasonUpdateUnexpectedError = "An unexpected error has occurred during season update.";
  public const string SeasonDeletionUnexpectedError = "An unexpected error has occurred during season deletion.";
  public const string SeasonUpdatedSuccessfully = "Season updated successfully.";
  public const string SeasonDeletedSuccessfully = "Season deleted successfully.";
}