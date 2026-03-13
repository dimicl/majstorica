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

    /// <summary>Konstruktor za učitavanje iz persistence.</summary>
    public ClientProfile(
        string? preferredContactPhone,
        string? notes,
        int totalJobsPosted,
        int completedJobs)
        : this(preferredContactPhone, notes)
    {
        if (totalJobsPosted < 0)
            throw new DomainException("Total jobs posted cannot be negative.");
        if (completedJobs < 0)
            throw new DomainException("Completed jobs cannot be negative.");
        SetStats(totalJobsPosted, completedJobs);
    }

    public string? PreferredContactPhone { get; private set; }

    public string? Notes { get; private set; }

    public int TotalJobsPosted { get; private set; }

    public int CompletedJobs { get; private set; }

    internal void SetStats(int totalJobsPosted, int completedJobs)
    {
        if (totalJobsPosted >= 0) TotalJobsPosted = totalJobsPosted;
        if (completedJobs >= 0) CompletedJobs = completedJobs;
    }

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