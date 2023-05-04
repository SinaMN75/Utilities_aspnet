namespace Utilities_aspnet.Utilities;

public class GenericResponse<T> : GenericResponse
{
    public GenericResponse(T result, UtilitiesStatusCodes status = UtilitiesStatusCodes.Success, string message = "")
    {
        Result = result;
        Status = status;
        Message = message;
    }

    public T? Result { get; }
}

public class GenericResponse
{
    public GenericResponse(UtilitiesStatusCodes status = UtilitiesStatusCodes.Success, string message = "")
    {
        Status = status;
        Message = message;
    }

    public UtilitiesStatusCodes Status { get; protected set; }
    public int? PageSize { get; set; }
    public int? PageCount { get; set; }
    public int? TotalCount { get; set; }
    protected string Message { get; set; }
}

public static class BoolExtension
{
    public static bool IsTrue(this bool? value) => value != null && value != false;
}

public static class EnumerableExtension
{
    public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? list) => list != null && list.Any();
    public static bool IsNotNull<T>(this IEnumerable<T>? list) => list != null;
}

public static class NumberExtension
{
    public static int ToInt(this double value) => (int)value;
    public static int ToInt(this double? value) => (int)(value ?? 0.0);
}

public static class StringExtension
{
    public static bool IsEmail(this string email) => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$", RegexOptions.IgnoreCase);
    public static bool IsNotNullOrEmpty(this string? s) => s is { Length: > 0 };
    public static bool IsNullOrEmpty(this string? s) => string.IsNullOrEmpty(s);
}

public class Utils
{
    public static int Random(int codeLength)
    {
        Random rnd = new();
        int otp = codeLength switch
        {
            4 => rnd.Next(1001, 9999),
            5 => rnd.Next(1001, 99999),
            _ => rnd.Next(1001, 999999)
        };

        return otp;
    }

    public static Tuple<bool,UtilitiesStatusCodes> IsBlockedUser(UserEntity? reciever, UserEntity? sender)
    {
        bool isBlocked = false;
        UtilitiesStatusCodes utilCode = UtilitiesStatusCodes.Success;
        if (reciever is not null && sender is not null)
        {
            if (reciever.BlockedUsers.IsNotNullOrEmpty())
            {
                isBlocked = reciever.BlockedUsers.Contains(sender.Id);
                if(isBlocked) utilCode = UtilitiesStatusCodes.UserSenderBlocked;
            }
            else if (sender.BlockedUsers.IsNotNullOrEmpty() && !isBlocked)
            {
                isBlocked = sender.BlockedUsers.Contains(reciever.Id);
                if (isBlocked) utilCode = UtilitiesStatusCodes.UserRecieverBlocked;                
            }
        }
        return new Tuple<bool, UtilitiesStatusCodes>(isBlocked,utilCode);
    }
}