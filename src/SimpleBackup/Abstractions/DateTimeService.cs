namespace SimpleBackup.Abstractions;

public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
}
