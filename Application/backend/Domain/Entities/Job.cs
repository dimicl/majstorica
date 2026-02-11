using backend.Domain.Enums;
using backend.Domain.Events;
using backend.Domain.States;

namespace backend.Domain.Entities;

public class Job
{
    public Guid Id { get; private set; }

    public Guid ClientId { get; private set; }
    public Guid? MasterId { get; private set; }

    public string Description { get; private set; } = default!;
    public decimal? Price { get; private set; }

    public bool IsEmergency { get; private set; }

    public JobStatus Status { get; private set; }

    //_state je objekat(ne enum) koji cuva trenutno stanje posla
    //tip IJobState jer ne treba da zna koje je konkretno stanje vec da zna da to stanje moze nesto da uradi
    //sve state klase nasledjuju IJobState i implemetiraju njegove metode  
    //private jer spoljasnji kod ne sme da menja stanje direktno vec samo job i njegove state klase
    private IJobState _state = default!;

    //_domainEvents lista dogadjaj koji su se desili nad ovim job-om (job je presao created -> pending, job je prihvacen...)
    //IDomainEvent interfejs koji nam kaze da se desilo nesto u domenu (poruka o dogadjaju)
    //private samo job nam prica da se desilo nesto nad njim ne sme od spolja
    //readonly jer se ne sme mennjati referenca na listu, ali se mogu dodavati elementi
    //new() da job ima spremnu listu da je ne pravi svaki put kad se nesto desi jer bi morali da pazimo da nije null
    //cuvamo domenske dogadjaje u entitetu jer da slje signalR poruke, rabbitMQ evente..., samo kaze da se nesto desilo
    //a onda Application/Srevices cita DomainEvents i radi sta je potrebno
    private readonly List<IDomainEvent> _domainEvents = new();
    //izlaze dogadjaje spolja samo za citanje, spolja moze da se vidi koje su al ne mogu da se promene
    //da service sloj vidi sta se desilo, ali da ne moze da utice na to sta ce   da se desi
    //tip IReadOnlyCollection<IDomainEvent> a ne List<IDomainEvent> da ne bi mogao da koristi Add i Clear funkcije nad IDomainEvent
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    /*event-driven pristup*/

    //ovo nam sluzi za funkcije u ovom kodu, za job.Rehdrate da ucita job iz baze bez da definise nove vr za opis, cenu...
    //nije public da neko ne napise job.Accept(masterid) nastace job bez opisa, validnog statusa..., a nije private jer bi onemogucio Rehydrate
    protected Job() { }

    //pravimo ga ovde kako application deo ne bi znao pravila joba, jer domen garantuje sva pravila joba
    //jedini nacin da nastane posao, kada klijent napravi zahtev za posao ovde se kreira
    public Job(Guid clientId, string description, decimal? price = null)
    {
        Id = Guid.NewGuid();
        ClientId = clientId;
        Description = description;
        Price = price;

        Status = JobStatus.Created;

        //da pokazes kako se taj posao ponasa kad je kreiran
        _state = new CreatedState();
    }

    //uzima podatke iz baze i vraca job entitet bez da mu promeni nesto
    //poziva se u Infrastructure iz koje baze vracamo job
    //static da pripada klasi da mozemo da je pozovemo i napravimo objekat jer ne mozemo da je pozovemo nad nepostojecim objektom
    public static Job Rehydrate(Guid id, Guid clientId, Guid? masterId, string description, decimal? price, bool isEmergency, string status)
    {
        var job = new Job
        {
            Id = id,
            ClientId = clientId,
            MasterId = masterId,
            Description = description,
            Price = price,
            IsEmergency = isEmergency,
            Status = Enum.Parse<JobStatus>(status)
        };
        job.SetStateFromString(status);
        return job;
    }

    //postavlja status na enum i state na ponasanje jer u suprotnom se cita kao string
    //internal da ne moze da se zove iz services i controllers da neko ne pozove job.SetStateFromStrinh("Completed") i narusi sistem
    //poziva se u Rehydrate
    internal void SetStateFromString(string status)
    {
        Status = Enum.Parse<JobStatus>(status);

        _state = Status switch
        {
            JobStatus.Created => new CreatedState(),
            JobStatus.Pending => new PendingState(),
            JobStatus.Accepted => new AcceptedState(),
            JobStatus.InProgress => new InProgressState(),
            JobStatus.Completed => new CompletedState(),
            _ => throw new Exception("Nepoznat status posla")
        };
    }

    // ------------------ DOMENSKE OPERACIJE ------------------
    //ove operacije su potrebne kad se menjaju stanja da se nesto izvrsi, sto samo se u domenu zna sta treba da se izvrsi

    public void SendRequests()
    {
        //poziva se funkcija iz stanja i ono odlucuje sta moze da se desi
        _state.SendRequests(this);
        //dodaje se event ako je moguce
        AddEvent(new JobUpdatedEvent(Id));
    }

    public void Accept(Guid masterId)
    {
        _state.Accept(this, masterId);
        AddEvent(new JobUpdatedEvent(Id));
    }

    public void Start()
    {
        _state.Start(this);
        AddEvent(new JobUpdatedEvent(Id));
    }

    public void Complete()
    {
        _state.Complete(this);
        AddEvent(new JobUpdatedEvent(Id));
    }

    public void ChangeDescription(string description)
    {
        _state.ChangeDescription(this, description);
        AddEvent(new JobUpdatedEvent(Id));
    }

    public void ChangePrice(decimal? price)
    {
        _state.ChangePrice(this, price);
        AddEvent(new JobUpdatedEvent(Id));
    }


    // ------------------ INTERNAL HELPERS ------------------
    //da bi dozvolio promene onima koji znaju pravila (state) a zabranio ostalim

    internal void SetMaster(Guid masterId)
    {
        MasterId = masterId;
    }

    internal void SetStatus(JobStatus status)
    {
        Status = status;
        //poziva metodu u JobStateFactory jos jedan obrazac koji nam omogucava da ne moramo svaki put da pisemo switch
        _state = JobStateFactory.Create(status);
    }

    internal void ChangeDescriptionInternal(string description)
    {
        Description = description;
    }

    internal void ChangePriceInternal(decimal? price) 
    { 
        Price = price;
    }

    private void AddEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearEvents()
    {
        _domainEvents.Clear();
    }

    internal void MarkAsEmergency()
    {
        IsEmergency = true;
    }

}
