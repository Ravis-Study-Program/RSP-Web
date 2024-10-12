namespace RSPWebAPI.Features.Constants;

// TODO: cleanup and refactor
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
  public const string UserListSuccessfully = "List of Users retrieved successfully.";
  public const string UserListUnexpectedError = "An unexpected error has occurred during users list.";
  
  public const string SeasonExists = "A season with this SeasonId already exists.";
  public const string SeasonDoesNotExists = "No season found using the SeasonId provided.";
  public const string SeasonCreatedSuccessfully = "Season created successfully.";
  public const string SeasonCreationUnexpectedError = "An unexpected error has occurred during season creation.";
  public const string SeasonUpdateUnexpectedError = "An unexpected error has occurred during season update.";
  public const string SeasonListUnexpectedError = "An unexpected error has occurred during season list.";
  public const string SeasonDeletionUnexpectedError = "An unexpected error has occurred during season deletion.";
  public const string SeasonUpdatedSuccessfully = "Season updated successfully.";
  public const string SeasonDeletedSuccessfully = "Season deleted successfully.";
  public const string SeasonListSuccessfully = "List of Season retrieved successfully.";

  public const string RoleExists = "A role with this RoleId already exists.";
  public const string RoleDoesNotExists = "No role found using the RoleId provided.";
  public const string RoleCreatedSuccessfully = "Role created successfully.";
  public const string RoleCreationUnexpectedError = "An unexpected error has occurred during role creation.";
  public const string RoleUpdateUnexpectedError = "An unexpected error has occurred during role update.";
  public const string RoleDeletionUnexpectedError = "An unexpected error has occurred during role deletion.";
  public const string RoleUpdatedSuccessfully = "Role updated successfully.";
  public const string RoleDeletedSuccessfully = "Role deleted successfully.";
  public const string RoleListSuccessfully = "List of Role retrieved successfully.";
  public const string RoleListUnexpectedError = "An unexpected error has occurred during role list.";

  public const string EnrollmentExists = "A enrollment with this EnrollmentId already exists.";
  public const string EnrollmentDoesNotExists = "No enrollment found using the EnrollmentId provided.";
  public const string EnrollmentCreatedSuccessfully = "Enrollment created successfully.";
  public const string EnrollmentCreationUnexpectedError = "An unexpected error has occurred during enrollment creation.";
  public const string EnrollmentUpdateUnexpectedError = "An unexpected error has occurred during enrollment update.";
  public const string EnrollmentDeletionUnexpectedError = "An unexpected error has occurred during enrollment deletion.";
  public const string EnrollmentUpdatedSuccessfully = "Enrollment updated successfully.";
  public const string EnrollmentDeletedSuccessfully = "Enrollment deleted successfully.";
  public const string EnrollmentListSuccessfully = "List of Enrollment retrieved successfully.";
  public const string EnrollmentListUnexpectedError = "An unexpected error has occurred during enrollment list.";
  public const string EnrollmentIsUserEnrolledError = "Checked if user is enrolled in a given season error";
  
  public const string KickStudentCurrentUserEnrollmentDoesNotExists = "No enrollment found using the provided current user details.";
  public const string KickStudentMenteeDoesntExist = "No student enrollment found using the provided mentee details.";
  public const string KickStudentSuccessfully = "Kick student successfully";
  public const string KickStudentUnexpectedError = "An unexpected error has occurred during kick student process.";
  
  public const string MentorshipExists = "A mentorship with this MentorshipId already exists.";
  public const string MentorshipDoesNotExists = "No mentorship found using the MentorshipId provided.";
  public const string MentorshipCreatedSuccessfully = "Mentorship created successfully.";
  public const string MentorshipCreationUnexpectedError = "An unexpected error has occurred during mentorship creation.";
  public const string MentorshipUpdateUnexpectedError = "An unexpected error has occurred during mentorship update.";
  public const string MentorshipDeletionUnexpectedError = "An unexpected error has occurred during mentorship deletion.";
  public const string MentorshipUpdatedSuccessfully = "Mentorship updated successfully.";
  public const string MentorshipDeletedSuccessfully = "Mentorship deleted successfully.";
  public const string MentorshipListSuccessfully = "List of Mentorship retrieved successfully.";
  public const string MentorshipListUnexpectedError = "An unexpected error has occurred during mentorship list.";
  public const string MentorshipNotPermittedDueToNullMentorOrMentee = "Mentor provided or Mentee provided do not exists";
  public const string MentorshipNotPermittedDueToDifferentSeason = "Mentor season and Mentee season are not the same";
}
