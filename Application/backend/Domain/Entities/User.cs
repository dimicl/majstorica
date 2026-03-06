using backend.Domain.Enums;
using backend.Shared.Exceptions;
using backend.Shared.Helpers;

namespace backend.Domain.Entities;


/*USER PREDSTAVLJA OSNOVNI NALOG KORISNIKA U SISTEMU, 
Služi za: registraciju i login, JWT autentifikaciju, proveru role korisnika, promenu lozinke, promenu osnovnih podataka

Koristi se u:
-AuthService za registraciju i login
-JwtHelper za pravljenje tokena
-UserService za izmene profila
-repozitorijumima za čuvanje i učitavanje user-a iz baze

U praksi:
-korisnik se registruje → pravi se User
-korisnik se loguje → čita se User, proverava lozinka, izdaje token
-kad menja profil ili lozinku → pozivaju se domenske metode nad User

trenutno treba da se pogleda treba li da se prosiri i mislim da treba i da se doda i novi entitet ADMIN
*/
public class User
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Username { get; private set; } = default!;

    public string PasswordHash { get; private set; } = default!;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public string? Phone { get; private set; }
    public string? DeliveryAddress { get; private set; }

    //protected da ne moze neko spolja da popuni polja, za ORM
    protected User() { }

    //kreiranje novog korisniika
    public User(
        string firstName,
        string lastName,
        string email,
        string username,
        string plainPassword,
        UserRole role,
        string? phone = null,
        string? deliveryAddress = null)
    {
        Id = Guid.NewGuid();

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Username = username;

        PasswordHash = PasswordHasher.Hash(plainPassword);

        Role = role;
        IsActive = true;
        Phone = phone;
        DeliveryAddress = deliveryAddress;
    }

    // ---------------- DOMENSKE OPERACIJE ----------------
    //ove operacije pravimo ovde jer entitet treba da zna pravila svog ponasanja, te metode kasnije koriste servisi, ove metode su ovde jer je to poslovna logika 

    public void ChangePassword(string oldPassword, string newPassword)
    {
        if (!PasswordHasher.Verify(oldPassword, PasswordHash))
            throw new InvalidCredentialsException("Pogrešna lozinka.");

        PasswordHash = PasswordHasher.Hash(newPassword);
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void UpdateContact(string? phone, string? deliveryAddress)
    {
        Phone = phone;
        DeliveryAddress = deliveryAddress;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public static User Rehydrate(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string username,
        string passwordHash,
        UserRole role,
        bool isActive,
        string? phone = null,
        string? deliveryAddress = null)
    {
        return new User
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Username = username,
            PasswordHash = passwordHash,
            Role = role,
            IsActive = isActive,
            Phone = phone,
            DeliveryAddress = deliveryAddress
        };
    }
}
