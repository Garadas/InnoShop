namespace UserService.Exceptions;

public class NotFoundException : AppException { public NotFoundException(string m) : base(m, 404) { } }
