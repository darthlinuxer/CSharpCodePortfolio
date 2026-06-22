namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed class User : Person<UserId>
{
    private UserState _state = new ActiveUserState();

    private User()
    {
    }

    private User(
        PersonName name,
        Cpf document,
        Address address,
        Contact contact,
        UserName userName,
        PasswordHash passwordHash)
        : base(UserId.NewId(), name, document, address, contact)
    {
        UserName = userName;
        PasswordHash = passwordHash;
    }

    public UserName UserName { get; private set; } = null!;

    public PasswordHash PasswordHash { get; private set; } = null!;

    public bool CanLogin => _state.CanLogin;

    public string UserStateName => _state.GetType().Name.Replace("UserState", string.Empty, StringComparison.Ordinal);

    private string StateKey
    {
        get => _state.Key;
        set => _state = UserStateRegistry.FromKey(value);
    }

    public void Activate() => _state = _state.Activate();

    public void Deactivate() => _state = _state.Deactivate();

    public void Lock() => _state = _state.Lock();

    public static User Register(
        PersonName name,
        Cpf document,
        Address address,
        Contact contact,
        UserName userName,
        PasswordHash passwordHash)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(userName);
        ArgumentNullException.ThrowIfNull(passwordHash);

        var user = new User(name, document, address, contact, userName, passwordHash);
        user.Raise(new UserRegisteredDomainEvent(user.Id, DateTime.UtcNow));

        return user;
    }

    public void ChangeUserName(UserName userName)
    {
        ArgumentNullException.ThrowIfNull(userName);
        UserName = userName;
    }

    public void ChangePassword(PasswordHash passwordHash)
    {
        ArgumentNullException.ThrowIfNull(passwordHash);
        PasswordHash = passwordHash;
    }
}
