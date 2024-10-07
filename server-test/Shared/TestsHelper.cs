namespace RSPWebAPI.Tests.Shared;

public class TestsHelper
{
  protected const string DummyDiscordId = "Jason#1234";
  protected const string DummyEmail = "abc@gmail.com";
  protected const string DummyProfileImage = "https://profile-image.com";
  protected const string DummyImageUrl = "https://image-url.com";
  protected const string DummyName = "John Doe";
  protected const string DummySlug = "ADL-2023-2024";
  protected DateTime DummyStartDate = DateTime.Now;
  protected DateTime DummyEndDate = DateTime.Now.AddDays(10);
  protected const string DummyLocation = "Sydney, Australia";
  protected Guid DummyGuid = Guid.Parse("b2c2e6c9-9d50-4c76-9159-6fba0f7fd355");
  protected Guid DummyGuid2 = Guid.Parse("a3d1f5b7-1c84-4b97-9fc1-8a3d5a9b3f42");
  protected Guid DummyGuid3 = Guid.Parse("6f9619ff-8b86-d011-b42d-00cf4fc964ff");
}
