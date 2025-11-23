namespace RSPWebAPI.Features.Constants;

public static class Messages
{
  // Common
  public static class Common
  {
    public const string UnexpectedError = "An unexpected error occurred.";
    public const string EndDateMustBeGreaterThanStartDate =
      "The end date should be later than the start date.";
  }

  // Entities
  public static readonly EntityMessages Season = new GenericMessages("Season");
  public static readonly UserMessages User = new();
  public static readonly EnrollmentMessages Enrollment = new();
  public static readonly MentorshipMessages Mentorship = new();
  public static readonly ProblemAttemptMessages ProblemAttempt = new();
  public static readonly MockInterviewMessages MockInterview = new();
  public static readonly SeasonWeekMessages SeasonWeek = new();

  // No CRUD entities
  public static class LeetcodeProblem
  {
    public const string Listed = "Leetcode problems list retrieved successfully.";
    public const string ScrapedSuccessfully = "Leetcode problems list scraped successfully.";
  }

  public static class LeetcodeProblemRecommendation
  {
    public const string Created = "The Leetcode problem recommendation was set up successfully.";
    public const string CreationError =
      "An error occurred while generating the Leetcode recommendation.";
    public const string NoProblemsLeft =
      "No recommendations available because all Leetcode problems have been completed.";
  }

  public static class DummyData
  {
    public const string Generated = "Dummy data were all generated successfully.";
    public const string GenerationError = "An error occurred while generating dummy data.";
  }

  public static class Graduates
  {
    public const string Listed = "Graduates list retrieved successfully.";
    public const string ListError = "An error occurred while retrieving the graduates list.";
  }

  public static class Student
  {
    public const string KickMenteeDoesntExist =
      "No student enrollment was found for the provided details.";
    public const string KickSuccess = "Student successfully removed.";
    public const string KickError = "An error occurred while removing the student.";
    public const string UpdateRolePromotionMenteeDoesntExist =
      "No student enrollment was found for the provided details.";
    public const string UpdateRolePromotionSuccess = "Role promotion updated successfully.";
    public const string UpdateRolePromotionError =
      "An error occurred while updating the role promotion.";
  }
}

// Base
public abstract class EntityMessages
{
  private protected EntityMessages(string entityName, string idSuffix = "Id")
  {
    EntityName = entityName;
    IdSuffix = idSuffix;
  }

  protected string EntityName { get; }
  protected string IdSuffix { get; }
  private string IdLabel => $"{EntityName}{IdSuffix}";

  public string Exists => $"A {EntityName} with this {IdLabel} already exists.";
  public string DoesNotExist => $"No {EntityName} was found with the given {IdLabel}.";
  public string Created => $"{EntityName} created successfully.";
  public string Updated => $"{EntityName} updated successfully.";
  public string Deleted => $"{EntityName} deleted successfully.";
  public string Listed => $"{EntityName} list retrieved successfully.";
  public string CreationError => $"An error occurred while creating the {EntityName}.";
  public string UpdateError => $"An error occurred while updating the {EntityName}.";
  public string DeletionError => $"An error occurred while deleting the {EntityName}.";
  public string ListError => $"An error occurred while retrieving the {EntityName} list.";
}

// Generic entity
internal sealed class GenericMessages : EntityMessages
{
  internal GenericMessages(string name, string idSuffix = "Id")
    : base(name, idSuffix) { }
}

// Concrete entities
public sealed class UserMessages : EntityMessages
{
  internal UserMessages()
    : base("User") { }

  public string EmailExists = "A user with this email is already registered.";
  public string EmailDoesNotExist = "No user was found with the provided email.";
  public string IdDoesNotExist = "No user was found with the given user id.";
  public string Found = "User found successfully.";

  public string NotInSeasonRole(string role) =>
    $"This user is not registered as a {role} for the specified season.";
}

public sealed class EnrollmentMessages : EntityMessages
{
  internal EnrollmentMessages()
    : base("Enrollment") { }

  public string UsersListed = "Enrollment users list retrieved successfully.";
  public string UsersListError = "An error occurred while retrieving the enrollment users list.";
  public string StudentMustHaveRolePromotion = "Students must have a valid role promotion.";
  public string MentorOrCoordinatorMustNotHaveRolePromotion =
    "Mentors and coordinators should not have a role promotion.";
}

public sealed class MentorshipMessages : EntityMessages
{
  internal MentorshipMessages()
    : base("Mentorship") { }

  public string NotPermittedDueToNullMentorOrMentee =
    "The provided mentor or mentee does not exist.";
  public string NotPermittedDueToDifferentSeason =
    "The mentor and mentee belong to different seasons.";
}

public sealed class ProblemAttemptMessages : EntityMessages
{
  internal ProblemAttemptMessages()
    : base("Problem attempt", idSuffix: "") { }

  public string OutOfSeasonDateRange =
    "The problem attempt date falls outside the season's date range.";
}

public sealed class MockInterviewMessages : EntityMessages
{
  internal MockInterviewMessages()
    : base("Mock interview", idSuffix: "") { }

  public string UpdateOnlyInterviewerAllowed =
    "Only interviewers are allowed to update the mock interview.";
  public string DeletionOnlyInterviewerAllowed =
    "Only interviewers are allowed to delete the mock interview.";
  public string InterviewerOrIntervieweeCannotBeFound =
    "The interviewer or interviewee could not be found.";
  public string OutOfSeasonDateRange =
    "The mock interview date falls outside the season's date range.";
  
  // Round review messages
  public string RoundReviewUpdated = "Review status updated successfully";
  public string CustomRoundReviewUpdated = "Custom round review status updated successfully";
  public string LeetcodeRoundReviewUpdated = "Leetcode round review status updated successfully";
  public string OnlyIntervieweeCanUpdateReview = "Only the interviewee can update the review status";
  public string CustomRoundNotFound = "Custom mock interview round not found";
  public string LeetcodeRoundNotFound = "Leetcode mock interview round not found";
  public string RoundUpdateError = "Error updating mock interview round review status";
  public string CustomRoundUpdateError = "Error updating custom mock interview round review status";
  public string LeetcodeRoundUpdateError = "Error updating leetcode mock interview round review status";
}

public sealed class SeasonWeekMessages : EntityMessages
{
  internal SeasonWeekMessages()
    : base("Season week") { }

  public string DatesNotWithinSeasonDates =
    "The season week dates do not align with the season dates.";
}
