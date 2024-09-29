namespace RSPWebAPI.Tests.Shared;

public class TestsHelper
{
  protected const string DummyDiscordId = "Jason#1234";
  protected const string DummyEmail = "abc@gmail.com";
  protected const string DummyProfileImage = "https://profile-image.com";
  protected const string DummyImageUrl = "https://image-url.com";
  protected const string DummyName = "John Doe";
  protected DateTime DummyStartDate = DateTime.Now;
  protected DateTime DummyEndDate = DateTime.Now.AddDays(10);
  protected const string DummyLocation = "Sydney, Australia";
  protected Guid DummyGuid = new Guid();
}