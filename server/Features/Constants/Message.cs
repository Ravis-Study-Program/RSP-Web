namespace RSPWebAPI.Features.Constants;

public static class Message
{
  public const string UserEmailExists = "A user with this email already exists.";
  public const string UserEmailDoesNotExists = "No user found using the email provided.";
  public const string UserCreatedSuccessfully = "User created successfully.";
  public const string UserCreationUnexpectedError = "An unexpected error has occurred during user creation.";
  public const string UserUpdateUnexpectedError = "An unexpected error has occurred during user update.";
  public const string UserDeletionUnexpectedError = "An unexpected error has occurred during user creation.";
  public const string UserUpdatedSuccessfully = "User updated successfully.";
  public const string UserDeletedSuccessfully = "User deleted successfully.";
}