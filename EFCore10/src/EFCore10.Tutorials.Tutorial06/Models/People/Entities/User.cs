namespace EFCore10.Tutorials.Tutorial06.Models;

public abstract class User : Person
{
    private UserState _state = new ActiveUserState();

    protected User()
    {
    }

    protected User(
        PersonName name,
        Cpf document,
        Address address,
        Contact contact,
        UserName userName,
        PasswordHash passwordHash)
        : base(name, document, address, contact)
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
