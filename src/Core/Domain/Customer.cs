using Core.Dto;

namespace Core.Domain;

public sealed class Customer
{
    public string Id { get; }
    public string Name { get; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }

    private Customer(
        string id,
        string name,
        string? email,
        string? phone,
        string? address)
    {
        Id = id;
        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
    }

    public static Customer Create(
        string id,
        string name,
        string? email = null,
        string? phone = null,
        string? address = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "Ідентифікатор клієнта не може бути порожнім",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Ім'я клієнта не може бути порожнім",
                nameof(name));
        }

        string? normalizedEmail = NormalizeOptional(email);
        string? normalizedPhone = NormalizeOptional(phone);
        ValidateContacts(normalizedEmail, normalizedPhone);

        return new Customer(
            id.Trim(),
            name.Trim(),
            normalizedEmail,
            normalizedPhone,
            NormalizeOptional(address));
    }

    public void UpdateContacts(string? email, string? phone)
    {
        string? normalizedEmail = NormalizeOptional(email);
        string? normalizedPhone = NormalizeOptional(phone);
        ValidateContacts(normalizedEmail, normalizedPhone);

        Email = normalizedEmail;
        Phone = normalizedPhone;
    }

    public void ChangeAddress(string? address)
    {
        Address = NormalizeOptional(address);
    }

    public CustomerDto ToDto() =>
        new(Id, Name, Email, Phone, Address);

    public static Customer FromDto(CustomerDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return Create(dto.Id, dto.Name, dto.Email, dto.Phone, dto.Address);
    }

    public override string ToString() =>
        $"{Id} {Name} — {Email ?? Phone}";

    private static void ValidateContacts(string? email, string? phone)
    {
        if (email is null && phone is null)
        {
            throw new ArgumentException(
                "Потрібно вказати email або телефон клієнта");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
