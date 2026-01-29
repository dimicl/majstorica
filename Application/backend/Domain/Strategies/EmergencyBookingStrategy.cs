using backend.Domain.Entities;

namespace backend.Domain.Strategies;

public class EmergencyBookingStrategy : IBookingStrategy
{
    public void Apply(Job job)
    {
        job.MarkAsEmergency();
    }
}
