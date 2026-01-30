using backend.Domain.Entities;

namespace backend.Domain.Strategies;

public class NormalBookingStrategy : IBookingStrategy
{
    public void Apply(Job job)
    {
        // Normalno zakazivanje nema dodatnih pravila, Job ostaje u Created stanju sa postojećim podacima.
    }
}
