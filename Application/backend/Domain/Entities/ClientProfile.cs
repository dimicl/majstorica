using backend.Domain.Exceptions;

namespace backend.Domain.Entities;

public class ClientProfile
{
    private ClientProfile()
    {
        // potrebno za serializer / mapper
    }

    public ClientProfile(string? preferredContactPhone, string? notes)
    {
        SetPreferredContactPhone(preferredContactPhone);
        SetNotes(notes);

        TotalJobsPosted = 0;
        CompletedJobs = 0;
    }

    public string? PreferredContactPhone { get; private set; }

    public string? Notes { get; private set; }

    public int TotalJobsPosted { get; private set; }

    public int CompletedJobs { get; private set; }

    public void UpdateProfile(string? preferredContactPhone, string? notes)
    {
        SetPreferredContactPhone(preferredContactPhone);
        SetNotes(notes);
    }

    public void IncrementJobsPosted()
    {
        TotalJobsPosted++;
    }

    public void IncrementCompletedJobs()
    {
        CompletedJobs++;
    }

    private void SetPreferredContactPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            PreferredContactPhone = null;
            return;
        }

        PreferredContactPhone = phone.Trim();
    }

    private void SetNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            Notes = null;
            return;
        }

        if (notes.Length > 1000)
            throw new DomainException("Notes cannot be longer than 1000 characters.");

        Notes = notes.Trim();
    }
}