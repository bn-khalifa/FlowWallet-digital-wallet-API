namespace FW.Domain;

public class DomainException(string message) : Exception(message);
public class NotFoundException(string resource, object id)
    : Exception($"{resource} with id '{id}' was not found.");

public class UnauthorizedException(string message = "Unauthorized access.")
    : Exception(message);

public class ConflictException(string message) : Exception(message);
