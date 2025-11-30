namespace UserService.Exceptions;

public class ConflictException : AppException { public ConflictException(string m) : base(m, 409) { } }
