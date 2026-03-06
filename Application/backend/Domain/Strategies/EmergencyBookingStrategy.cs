using backend.Domain.Entities;

namespace backend.Domain.Strategies;

public class EmergencyBookingStrategy : IBookingStrategy
{
    public void Apply(Job job)
    {
        job.MarkAsEmergency();
    }
    /*treba dodati za hitan posao:
        -možda ide prioritetno
        -možda menja cenu
        -možda menja status drugačije
        -možda ima posebna pravila*/
}
