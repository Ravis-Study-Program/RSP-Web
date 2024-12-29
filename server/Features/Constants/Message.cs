namespace RSPWebAPI.Features.Constants;

// TODO: cleanup and refactor
public static class Message
{
  public const string EndDateMustBeGreaterThanStartDate =
    "End date must be greater than start date.";

  public const string UserEmailExists = "A user with this email already exists.";
  public const string UserEmailDoesNotExists = "No user found using the email provided.";
  public const string UserCreatedSuccessfully = "User created successfully.";
  public const string UserCreationUnexpectedError =
    "An unexpected error has occurred during user creation.";
  public const string UserUpdateUnexpectedError =
    "An unexpected error has occurred during user update.";
  public const string UserDeletionUnexpectedError =
    "An unexpected error has occurred during user deletion.";
  public const string UserUpdatedSuccessfully = "User updated successfully.";
  public const string UserDeletedSuccessfully = "User deleted successfully.";
  public const string UserListSuccessfully = "List of Users retrieved successfully.";
  public const string UserListUnexpectedError =
    "An unexpected error has occurred during users list.";

  public const string SeasonExists = "A season with this SeasonId already exists.";
  public const string SeasonDoesNotExists = "No season found using the SeasonId provided.";
  public const string SeasonCreatedSuccessfully = "Season created successfully.";
  public const string SeasonCreationUnexpectedError =
    "An unexpected error has occurred during season creation.";
  public const string SeasonUpdateUnexpectedError =
    "An unexpected error has occurred during season update.";
  public const string SeasonListUnexpectedError =
    "An unexpected error has occurred during season list.";
  public const string SeasonDeletionUnexpectedError =
    "An unexpected error has occurred during season deletion.";
  public const string SeasonUpdatedSuccessfully = "Season updated successfully.";
  public const string SeasonDeletedSuccessfully = "Season deleted successfully.";
  public const string SeasonListSuccessfully = "List of Season retrieved successfully.";

  public const string GraduatesListSuccessfully = "List of Graduates retrieved successfully.";
  public const string GraduatesListUnexpectedError =
    "An unexpected error has occurred during graduates list.";

  public const string SeasonUsersListSuccessfully = "List of SeasonUsers retrieved successfully.";
  public const string SeasonUsersListUnexpectedError =
    "An unexpected error has occurred during season users list.";

  public const string EnrollmentExists = "A enrollment with this EnrollmentId already exists.";
  public const string EnrollmentDoesNotExists =
    "No enrollment found using the EnrollmentId provided.";
  public const string EnrollmentCreatedSuccessfully = "Enrollment created successfully.";
  public const string EnrollmentCreationUnexpectedError =
    "An unexpected error has occurred during enrollment creation.";
  public const string EnrollmentUpdateUnexpectedError =
    "An unexpected error has occurred during enrollment update.";
  public const string EnrollmentDeletionUnexpectedError =
    "An unexpected error has occurred during enrollment deletion.";
  public const string EnrollmentUpdatedSuccessfully = "Enrollment updated successfully.";
  public const string EnrollmentDeletedSuccessfully = "Enrollment deleted successfully.";
  public const string EnrollmentListSuccessfully = "List of Enrollment retrieved successfully.";
  public const string EnrollmentListUnexpectedError =
    "An unexpected error has occurred during enrollment list.";
  public const string EnrollmentIsUserEnrolledError =
    "Checked if user is enrolled in a given season error";
  public const string EnrollmentStudentMustHaveAppropriateRolePromotion =
    "A student role must have a valid Role Promotion";
  public const string EnrollmentMentorOrCoordinatorMustNotHaveRolePromotion =
    "A mentor or coordinator role must not have Role Promotion";

  public const string KickStudentCurrentUserEnrollmentDoesNotExists =
    "No enrollment found using the provided current user details.";
  public const string KickStudentMenteeDoesntExist =
    "No student enrollment found using the provided mentee details.";
  public const string KickStudentSuccessfully = "Kick student successfully";
  public const string KickStudentUnexpectedError =
    "An unexpected error has occurred during kick student process.";

  public const string UpdateStudentRolePromotionEnrollmentDoesNotExists =
    "No enrollment found using the provided current user details.";
  public const string UpdateStudentRolePromotionMenteeDoesntExist =
    "No student enrollment found using the provided mentee details.";
  public const string UpdateStudentRolePromotionSuccessfully =
    "Updated role promotion successfully";
  public const string UpdateStudentRolePromotionUnexpectedError =
    "An unexpected error has occurred during update role promotion process.";

  public const string MentorshipExists = "A mentorship with this MentorshipId already exists.";
  public const string MentorshipDoesNotExists =
    "No mentorship found using the MentorshipId provided.";
  public const string MentorshipCreatedSuccessfully = "Mentorship created successfully.";
  public const string MentorshipCreationUnexpectedError =
    "An unexpected error has occurred during mentorship creation.";
  public const string MentorshipUpdateUnexpectedError =
    "An unexpected error has occurred during mentorship update.";
  public const string MentorshipDeletionUnexpectedError =
    "An unexpected error has occurred during mentorship deletion.";
  public const string MentorshipUpdatedSuccessfully = "Mentorship updated successfully.";
  public const string MentorshipDeletedSuccessfully = "Mentorship deleted successfully.";
  public const string MentorshipListSuccessfully = "List of Mentorship retrieved successfully.";
  public const string MentorshipListUnexpectedError =
    "An unexpected error has occurred during mentorship list.";
  public const string MentorshipNotPermittedDueToNullMentorOrMentee =
    "Mentor provided or Mentee provided do not exists";
  public const string MentorshipNotPermittedDueToDifferentSeason =
    "Mentor season and Mentee season are not the same";

  public const string ProblemAttemptCreatedSuccessfully = "ProblemAttempt created successfully.";
  public const string ProblemAttemptDoesNotExists =
    "No problem attempt found using the Problem Attempt Id provided.";
  public const string ProblemAttemptUpdatedSuccessfully = "ProblemAttempt updated successfully.";
  public const string ProblemAttemptCreationUnexpectedError =
    "An unexpected error has occurred during problem attempt creation.";
  public const string ProblemAttemptDeletionUnexpectedError =
    "An unexpected error has occurred during prblem attempt deletion.";
  public const string ProblemAttemptUpdateUnexpectedError =
    "An unexpected error has occurred during prblem attempt update.";
  public const string ProblemAttemptDeletedSuccessfully = "ProblemAttempt deleted successfully.";
  public const string ProblemAttemptListSuccessfully =
    "List of ProblemAttempt retrieved successfully.";
  public const string ProblemAttemptListUnexpectedError =
    "An unexpected error has occurred during problem attempts list.";
  public const string ProblemAttemptOutOfSeasonDateRange =
    "Problem attempt date is out of season date range.";

  public const string LeetcodeProblemsListSuccessfully =
    "List of LeetcodeProblems retrieved successfully.";
  public const string LeetcodeProblemsListUnexpectedError =
    "An unexpected error has occurred during leetcode problems list.";

  public const string MockInterviewCreatedSuccessfully = "Mock Interview created successfully.";
  public const string MockInterviewDoesNotExists =
    "No mock interview found using the Mock Interview Id provided.";
  public const string MockInterviewUpdatedSuccessfully = "Mock Interview updated successfully.";
  public const string MockInterviewCreationUnexpectedError =
    "An unexpected error has occurred during mock interview creation.";
  public const string MockInterviewDeletionUnexpectedError =
    "An unexpected error has occurred during mock interview deletion.";
  public const string MockInterviewUpdateUnexpectedError =
    "An unexpected error has occurred during mock interview update.";
  public const string MockInterviewDeletedSuccessfully = "Mock Interview deleted successfully.";
  public const string MockInterviewListSuccessfully =
    "List of Mock Interview retrieved successfully.";
  public const string MockInterviewListUnexpectedError =
    "An unexpected error has occurred during mock interviews list.";
  public const string MockInterviewOutOfSeasonDateRange =
    "Mock interview date is out of season date range.";

  public const string SeasonWeekCreatedSuccessfully = "Season Week created successfully.";
  public const string SeasonWeekWeekNumberAlreadyExists = "Season Week Week Number already exists.";
  public const string SeasonWeekDatesNotWithinSeasonDates =
    "Season Week dates do not fall under Season dates";
  public const string SeasonWeekListSuccessfully = "List of SeasonWeek retrieved successfully.";
  public const string SeasonWeekUpdateSuccessfully = "Season Week updated successfully.";
  public const string SeasonWeekCreationUnexpectedError =
    "An unexpected error has occurred during season week creation.";
  public const string SeasonWeekListUnexpectedError =
    "An unexpected error has occurred during season week list.";
  public const string SeasonWeekDeletionUnexpectedError =
    "An unexpected error has occurred during season week deletion.";
  public const string SeasonWeekUpdateUnexpectedError =
    "An unexpected error has occurred during season week update.";
  public const string SeasonWeekDoesNotExists =
    "No mentorship found using the Season Week Id provided.";
  public const string SeasonWeekDeletedSuccessfully = "Season Week deleted successfully.";
}
