namespace RSPWebAPI.Shared;

public class Error(string code, string message)
{
    public string Code { get; } = code;
    public string Message { get; } = message;
    
    public static readonly Error None = new(string.Empty, string.Empty);

    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

    public static readonly Error ConditionNotMet = new("Error.ConditionNotMet", "The specified condition was not met.");
}