namespace UserService.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string m) : base(m, 401) { }
}
