using backend.Domain.Entities;

namespace backend.Domain.Strategies;

public interface IBookingStrategy
{
    void Apply(Job job);
}
