namespace UserService.Exceptions;

public class BadRequestException : AppException { public BadRequestException(string m) : base(m, 400) { } }
