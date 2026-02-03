namespace backend.Infrastructure.Persistence.Neo4j.Entities;

/// <summary>
/// Minimalni podaci Job čvora u Neo4j grafu (samo id za relacije INVITED, ACCEPTED_BY).
/// Pun sadržaj posla je u MongoDB.
/// </summary>
public class JobNode
{
    public Guid Id { get; set; }
}
