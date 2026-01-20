using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetById(Guid id);
    Task Save(Job job);
}
