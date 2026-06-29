using Microsoft.AspNetCore.Http;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Traditional;

/// <summary>
/// Shows a compilable null-heavy implementation that the tutorial later replaces with LanguageExt.Core.
/// </summary>
public static class TraditionalNullRegistrationExample
{
    /// <summary>
    /// Raw request with nullable properties and no domain meaning.
    /// </summary>
    public sealed record TraditionalRegisterUserRequest(
        string? Name,
        string? Email,
        string? PhoneNumber);

    /// <summary>
    /// Traditional email wrapper that still appears as Email? even when the business says it is required.
    /// </summary>
    public sealed record Email(string Value);

    /// <summary>
    /// Traditional phone wrapper that can still be absent as PhoneNumber?.
    /// </summary>
    public sealed record PhoneNumber(string Value);

    /// <summary>
    /// Entity that can be constructed partially invalid because optional state is implicit.
    /// </summary>
    public sealed class TraditionalUser
    {
        /// <summary>
        /// Gets or sets the user id after persistence.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a required name, but the type still allows null.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets a required email through a nullable object.
        /// </summary>
        public Email? Email { get; set; }

        /// <summary>
        /// Gets or sets an optional phone through a nullable object.
        /// </summary>
        public PhoneNumber? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Simulates a procedural service that mixes validation, duplicate checks, throws, and null returns.
    /// </summary>
    public sealed class TraditionalRegistrationService
    {
        private readonly List<TraditionalUser> users = [];

        /// <summary>
        /// Registers a user using exceptions for validation failures and null for duplicate conflicts.
        /// </summary>
        public Task<TraditionalUser?> RegisterAsync(
            TraditionalRegisterUserRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Nome obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new InvalidOperationException("Email obrigatório.");
            }

            if (!request.Email.Contains('@', StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Email inválido.");
            }

            if (users.Any(user => user.Email?.Value == request.Email))
            {
                return Task.FromResult<TraditionalUser?>(null);
            }

            PhoneNumber? phone = null;
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                if (request.PhoneNumber.Length < 10)
                {
                    throw new InvalidOperationException("Telefone inválido.");
                }

                phone = new PhoneNumber(request.PhoneNumber);
            }

            var user = new TraditionalUser
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = new Email(request.Email),
                PhoneNumber = phone
            };

            users.Add(user);

            return Task.FromResult<TraditionalUser?>(user);
        }
    }

    /// <summary>
    /// Simulates a controller that must infer meaning from null, exceptions, and object shape.
    /// </summary>
    public sealed class TraditionalRegistrationController(TraditionalRegistrationService service)
    {
        /// <summary>
        /// Translates implicit service states into HTTP responses through defensive if/else.
        /// </summary>
        public async Task<IResult> RegisterAsync(
            TraditionalRegisterUserRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var user = await service.RegisterAsync(request, cancellationToken).ConfigureAwait(false);

                if (user is null)
                {
                    return Results.Conflict(new { Error = "Email duplicado." });
                }

                return Results.Created($"/users/{user.Id}", user);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new { Error = exception.Message });
            }
        }
    }
}
