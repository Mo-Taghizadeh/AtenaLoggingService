namespace AtenaLoggingService.Core;

public enum EventType : byte
{
    Request = 1,
    Response = 2,
    Exception = 3,
    Custom = 4
}