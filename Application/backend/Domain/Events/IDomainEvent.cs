namespace backend.Domain.Events;

//ovo pravimo jer samo ovde kazemo sta se desilo u domenu, da ne zna za signalR.., ali da bi omogucili da neko odreaguje na promene u domenu
public interface IDomainEvent
{
    //trenutak kada se desila promena u domenu
    DateTime OccurredAt { get; }
}
