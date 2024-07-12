
namespace HealthTracker.Configuration.Messages
{
   public class ErrorMessages
   {
     public static class Generic
    {
        public static string TypeBadRequest = "Bad Request";
        public static string TypeNotFound = "User not found";
        public static string InvalidPayload = "Invalid payload";
        public static string InvalidRequest = "Invalid request";
        public static string ObjectNotFound = "Object not found";
        public static string UnableToProcess = "Unable to process request";
        public static string SomethingWentWrong = "Something went wrong, please try again later";
    }

    public static class Profile
    {
        public static string UserNotFound = "User not found";
    }

    public static class User
    {
        public static string UserNotFound = "User not found";
    }
   }
}