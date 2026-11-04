namespace QTKD.Api.Exceptions;

/// <summary>
/// Ngoại lệ đại diện cho các vi phạm quy tắc nghiệp vụ hệ thống (Business Rules: CTR-01, CTR-02, CTR-03,...)
/// </summary>
public class BusinessRuleException : Exception
{
    public List<string> Errors { get; }

    public BusinessRuleException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }

    public BusinessRuleException(string message, List<string> errors) : base(message)
    {
        Errors = errors ?? new List<string> { message };
    }
}

