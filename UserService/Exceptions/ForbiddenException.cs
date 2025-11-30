namespace UserService.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string m) : base(m, 403) { }
}
