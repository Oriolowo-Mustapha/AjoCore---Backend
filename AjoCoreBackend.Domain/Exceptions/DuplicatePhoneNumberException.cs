namespace AjoCoreBackend.Domain.Exceptions
{
    public class DuplicatePhoneNumberException : DomainException
    {
        public DuplicatePhoneNumberException(string phoneNumber) 
            : base($"An account with phone number '{phoneNumber}' already exists.") { }
    }
}
