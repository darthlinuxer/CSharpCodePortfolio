using EFCore10.Tutorials.Tutorial06.Extensions;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record Address
{
    private Address()
    {
    }

    private Address(string street, string number, string city, StateCode state, ZipCode zipCode)
    {
        Street = street.NormalizeLength(nameof(Street), 3, 200);
        Number = number.NormalizeLength(nameof(Number), 1, 30);
        City = city.NormalizeLength(nameof(City), 2, 100);
        State = state;
        ZipCode = zipCode;
    }

    public string Street { get; private set; } = null!;

    public string Number { get; private set; } = null!;

    public string City { get; private set; } = null!;

    public StateCode State { get; private set; } = null!;

    public ZipCode ZipCode { get; private set; } = null!;

    public static Address Create(string street, string number, string city, StateCode state, ZipCode zipCode)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(zipCode);

        return new Address(street, number, city, state, zipCode);
    }
}
