namespace RSPWebAPI.Features.Constants;

public static class Message
{
  public const string EndDateMustBeGreaterThanStartDate =
    "The end date should be later than the start date.";

  public const string UserIsNotStudentInSeason =
    "This user is not registered as a student for the specified season.";
  public const string UserIsNotMentorInSeason =
    "This user is not registered as a mentor for the specified season.";
  public const string UserIsNotCoordinatorInSeason =
    "This user is not registered as a coordinator for the specified season.";

  public const string DummyDataGeneratedSuccessfully =
    "Dummy data were all generated successfully.";
  public const string DummyDataUnexpectedError = "An error occurred while generating dummy data.";

  public const string UnexpectedError = "An unexpected error occurred.";

  public const string LeetcodeProblemRecommenderCreatedSuccessfully =
    "The Leetcode problem recommendation was set up successfully.";
  public const string LeetcodeProblemRecommenderUnexpectedError =
    "An error occurred while generating the Leetcode recommendation.";
  public const string LeetcodeProblemRecommenderNoLeetcodeProblemLeft =
    "No recommendations available because all Leetcode problems have been completed.";

  public const string UserEmailExists = "A user with this email is already registered.";
  public const string GetUserSuccessfully = "User found successfully.";
  public const string UserEmailDoesNotExists = "No user was found with the provided email.";
  public const string UserIdDoesNotExists = "No user was found with the given user id.";
  public const string UserCreatedSuccessfully = "User created successfully.";
  public const string UserCreationUnexpectedError = "An error occurred while creating the user.";
  public const string UserUpdateUnexpectedError = "An error occurred while updating the user.";
  public const string UserDeletionUnexpectedError = "An error occurred while deleting the user.";
  public const string UserUpdatedSuccessfully = "User updated successfully.";
  public const string UserDeletedSuccessfully = "User deleted successfully.";
  public const string UserListSuccessfully = "Users list retrieved successfully.";
  public const string UserListUnexpectedError =
    "An error occurred while retrieving the users list.";

  public const string SeasonExists = "A season with this SeasonId already exists.";
  public const string SeasonDoesNotExists = "No season was found with the given SeasonId.";
  public const string SeasonCreatedSuccessfully = "Season created successfully.";
  public const string SeasonCreationUnexpectedError =
    "An error occurred while creating the season.";
  public const string SeasonUpdateUnexpectedError = "An error occurred while updating the season.";
  public const string SeasonListUnexpectedError =
    "An error occurred while retrieving the season list.";
  public const string SeasonDeletionUnexpectedError =
    "An error occurred while deleting the season.";
  public const string SeasonUpdatedSuccessfully = "Season updated successfully.";
  public const string SeasonDeletedSuccessfully = "Season deleted successfully.";
  public const string SeasonListSuccessfully = "Season list retrieved successfully.";
  public const string GraduatesListSuccessfully = "Graduates list retrieved successfully.";
  public const string GraduatesListUnexpectedError =
    "An error occurred while retrieving the graduates list.";

  public const string EnrollmentUsersListSuccessfully =
    "Enrollment users list retrieved successfully.";
  public const string EnrollmentUsersListUnexpectedError =
    "An error occurred while retrieving the enrollment users list.";

  public const string EnrollmentExists = "An enrollment with this EnrollmentId already exists.";
  public const string EnrollmentDoesNotExists =
    "No enrollment was found with the given EnrollmentId.";
  public const string EnrollmentCreatedSuccessfully = "Enrollment created successfully.";
  public const string EnrollmentCreationUnexpectedError =
    "An error occurred while creating the enrollment.";
  public const string EnrollmentUpdateUnexpectedError =
    "An error occurred while updating the enrollment.";
  public const string EnrollmentDeletionUnexpectedError =
    "An error occurred while deleting the enrollment.";
  public const string EnrollmentUpdatedSuccessfully = "Enrollment updated successfully.";
  public const string EnrollmentDeletedSuccessfully = "Enrollment deleted successfully.";
  public const string EnrollmentListSuccessfully = "Enrollment list retrieved successfully.";
  public const string EnrollmentListUnexpectedError =
    "An error occurred while retrieving the enrollment list.";
  public const string EnrollmentStudentMustHaveAppropriateRolePromotion =
    "Students must have a valid role promotion.";
  public const string EnrollmentMentorOrCoordinatorMustNotHaveRolePromotion =
    "Mentors and coordinators should not have a role promotion.";

  public const string KickStudentMenteeDoesntExist =
    "No student enrollment was found for the provided details.";
  public const string KickStudentSuccessfully = "Student successfully removed.";
  public const string KickStudentUnexpectedError = "An error occurred while removing the student.";

  public const string UpdateStudentRolePromotionMenteeDoesntExist =
    "No student enrollment was found for the provided details.";
  public const string UpdateStudentRolePromotionSuccessfully =
    "Role promotion updated successfully.";
  public const string UpdateStudentRolePromotionUnexpectedError =
    "An error occurred while updating the role promotion.";

  public const string MentorshipExists = "A mentorship with this MentorshipId already exists.";
  public const string MentorshipDoesNotExists =
    "No mentorship was found with the given MentorshipId.";
  public const string MentorshipCreatedSuccessfully = "Mentorship created successfully.";
  public const string MentorshipCreationUnexpectedError =
    "An error occurred while creating the mentorship.";
  public const string MentorshipUpdateUnexpectedError =
    "An error occurred while updating the mentorship.";
  public const string MentorshipDeletionUnexpectedError =
    "An error occurred while deleting the mentorship.";
  public const string MentorshipUpdatedSuccessfully = "Mentorship updated successfully.";
  public const string MentorshipDeletedSuccessfully = "Mentorship deleted successfully.";
  public const string MentorshipListSuccessfully = "Mentorship list retrieved successfully.";
  public const string MentorshipListUnexpectedError =
    "An error occurred while retrieving the mentorship list.";
  public const string MentorshipNotPermittedDueToNullMentorOrMentee =
    "The provided mentor or mentee does not exist.";
  public const string MentorshipNotPermittedDueToDifferentSeason =
    "The mentor and mentee belong to different seasons.";

  public const string ProblemAttemptCreatedSuccessfully = "Problem attempt created successfully.";
  public const string ProblemAttemptDoesNotExists =
    "No problem attempt was found with the given ID.";
  public const string ProblemAttemptUpdatedSuccessfully = "Problem attempt updated successfully.";
  public const string ProblemAttemptCreationUnexpectedError =
    "An error occurred while creating the problem attempt.";
  public const string ProblemAttemptDeletionUnexpectedError =
    "An error occurred while deleting the problem attempt.";
  public const string ProblemAttemptUpdateUnexpectedError =
    "An error occurred while updating the problem attempt.";
  public const string ProblemAttemptDeletedSuccessfully = "Problem attempt deleted successfully.";
  public const string ProblemAttemptListSuccessfully =
    "Problem attempts list retrieved successfully.";
  public const string ProblemAttemptListUnexpectedError =
    "An error occurred while retrieving the problem attempts list.";
  public const string ProblemAttemptOutOfSeasonDateRange =
    "The problem attempt date falls outside the season's date range.";

  public const string LeetcodeProblemsListSuccessfully =
    "Leetcode problems list retrieved successfully.";

  public const string MockInterviewCreatedSuccessfully = "Mock interview created successfully.";
  public const string MockInterviewDoesNotExists = "No mock interview was found with the given ID.";
  public const string MockInterviewUpdatedSuccessfully = "Mock interview updated successfully.";
  public const string MockInterviewCreationUnexpectedError =
    "An error occurred while creating the mock interview.";
  public const string MockInterviewDeletionUnexpectedError =
    "An error occurred while deleting the mock interview.";
  public const string MockInterviewUpdateUnexpectedError =
    "An error occurred while updating the mock interview.";
  public const string MockInterviewDeletedSuccessfully = "Mock interview deleted successfully.";
  public const string MockInterviewInterviewerOrIntervieweeCannotBeFound =
    "The interviewer or interviewee could not be found.";
  public const string MockInterviewListSuccessfully = "Mock interview list retrieved successfully.";
  public const string MockInterviewListUnexpectedError =
    "An error occurred while retrieving the mock interviews list.";
  public const string MockInterviewOutOfSeasonDateRange =
    "The mock interview date falls outside the season's date range.";

  public const string SeasonWeekCreatedSuccessfully = "Season week created successfully.";
  public const string SeasonWeekExists = "A season week with this SeasonWeekId already exists.";
  public const string SeasonWeekDatesNotWithinSeasonDates =
    "The season week dates do not align with the season dates.";
  public const string SeasonWeekListSuccessfully = "Season week list retrieved successfully.";
  public const string SeasonWeekUpdateSuccessfully = "Season week updated successfully.";
  public const string SeasonWeekCreationUnexpectedError =
    "An error occurred while creating the season week.";
  public const string SeasonWeekListUnexpectedError =
    "An error occurred while retrieving the season week list.";
  public const string SeasonWeekDeletionUnexpectedError =
    "An error occurred while deleting the season week.";
  public const string SeasonWeekUpdateUnexpectedError =
    "An error occurred while updating the season week.";
  public const string SeasonWeekDoesNotExists =
    "No season week was found with the given SeasonWeekId.";
  public const string SeasonWeekDeletedSuccessfully = "Season week deleted successfully.";
}
