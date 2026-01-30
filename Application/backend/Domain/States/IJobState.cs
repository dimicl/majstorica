using backend.Domain.Entities;

namespace backend.Domain.States;

public interface IJobState
{
    //fakticki dozvoljene akcije nad job-om
    //job kao param jer state ne cuva pod samo vodi racuna o pravilima
    void SendRequests(Job job);                 
    void Accept(Job job, Guid masterId);        
    void Start(Job job);                        
    void Complete(Job job);                     

    void ChangeDescription(Job job, string description);
    void ChangePrice(Job job, decimal? price);
}
