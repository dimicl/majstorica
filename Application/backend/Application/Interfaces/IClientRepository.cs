using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IClientRepository
{
    Task Save(Guid userId, ClientProfile clientProfile);
    Task<ClientProfile?> GetById(Guid userId);
    Task<ClientProfile?> GetByUserId(Guid userId);
}
