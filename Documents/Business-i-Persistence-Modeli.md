# Business model (model podataka) i Persistence model (model perzistencije)

## Pregled

- **Business model (model podataka)** = domenski entiteti u `Domain/Entities/`. Koriste ih Application sloj (servisi, use case-ovi). Ne zavise od baze (Neo4j, Redis, MongoDB). Čiste C# klase sa poslovnom logikom.
- **Persistence model (entity model)** = strukture koje se direktno perzistiraju u bazi. Žive u **Infrastructure** (npr. `Infrastructure/Persistence/Redis/Entities/`, `.../MongoDb/`, Neo4j čvorovi). Zavise od tehnologije (Redis.OM atribute, MongoDB Bson, Neo4j property-ji).
- **Biblioteka za snimanje/učitavanje** = sloj perzistencije: **Repository** (interfejs u Application, implementacija u Infrastructure) + **Mapper** (Infrastructure): Business → Persistence pri snimanju, Persistence → Business pri učitavanju. Obrazac: **Data Mapper** (Fowler).

---

## Koji entiteti treba da budu u domenu

| Entitet              | U domenu?    | Objašnjenje                                                                                                                  |
| -------------------- | ------------ | ---------------------------------------------------------------------------------------------------------------------------- |
| **User**             | Da           | Nalog korisnika – čist poslovni koncept.                                                                                     |
| **Client**           | Da           | Profil klijenta (UserId, Phone, DeliveryAddress).                                                                            |
| **Master**           | Da           | Profil majstora (UserId, Bio, Categories, Rating, itd.).                                                                     |
| **Job**              | Da           | Posao – jezgro domene.                                                                                                       |
| **Review**           | Da           | Recenzija posla.                                                                                                             |
| **ChatConversation** | Da           | Razgovor između klijenta i majstora u kontekstu posla – poslovni koncept.                                                    |
| **ChatMessage**      | Da           | Jedna poruka u razgovoru – poslovni koncept.                                                                                 |
| **UserSession**      | Da (opciono) | Trenutna sesija/kontekst korisnika (ko je ulogovan, šta gleda). Može da ostane u domenu kao koncept, ali bez Redis atributa. |

Svi ovi entiteti u **Domain/Entities/** treba da budu **bez** referenci na Redis, Neo4j, MongoDB (nema `using Redis.OM`, nema `INode`, nema atribute za bazu).

---

## Business model (Domain) – šta imamo

- **User**, **Client**, **Master**, **Job**, **Review**, **ChatConversation** – čisti (Rehydrate/FromPersistence uz primitivne tipove).
- **ChatMessage**, **UserSession** – treba ukloniti Redis.OM atribute; ostaju samo property-ji (Id, ConversationId, itd.).

---

## Persistence model (Infrastructure) – šta treba

| Gde se čuva | Persistence entitet (entity model)                                                                                                                                            | Odgovara business entitetu                          |
| ----------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------- |
| **MongoDB** | **UserDocument**, **ClientDocument**, **MasterDocument**, **JobDocument**, **ConversationDocument**, **ReviewDocument** (ili interni u repozitorijumu). Pun sadržaj entiteta. | User, Client, Master, Job, ChatConversation, Review |
| **Neo4j**   | Samo **graf**: minimalni čvorovi User (id, role), Job (id) i relacije (INVITED, ACCEPTED_BY itd.). Nema punih podataka – samo za relacije, preporuke, složene pretrage.       | –                                                   |
| **Redis**   | **ChatMessageDocument**, **UserSessionDocument** (Redis.OM). Sesije, real-time chat, keš, distributed locking.                                                                | ChatMessage, UserSession                            |

Mapiranje: **Mapper** u Infrastructure prevodi Business ↔ Persistence (npr. `UserMapper.ToDocument(User)` → `UserDocument`, `UserMapper.ToDomain(UserDocument)` → `User`).

---

## Data Layer obrazac (Data Mapper)

1. **Repository** (Application.Interfaces): `Task Save(ChatMessage message);` `Task<List<ChatMessage>> GetByConversationId(Guid conversationId);` – radi sa **business** tipovima.
2. **Repository implementacija** (Infrastructure): prima/vraća business tip. Unutra: **Mapper.ToEntity(message)** → persistence dokument, upis u Redis; čitanje iz Redis → **Mapper.ToDomain(doc)** → vraća `ChatMessage`.
3. **Mapper** (Infrastructure): `ChatMessageDocument ToEntity(ChatMessage);` `ChatMessage ToDomain(ChatMessageDocument);` – bez logike, samo kopiranje polja.

Tako se ispoštuje zahtev: „biblioteka sa funkcijama za snimanje i učitavanje business modela“ = repozitorijumi; „mapiranje business → entity model koji se perzistira“ = mapperi u Infrastructure.

---

## Implementacija u projektu

### Redis (chat, sesije)

- **Domain:** `ChatMessage`, `UserSession` – čiste klase, bez Redis.OM.
- **Persistence:** `Redis/Entities/ChatMessageDocument.cs`, `UserSessionDocument.cs` (Redis.OM).
- **Mapper:** `Redis/Mappers/ChatMessageMapper.cs`, `UserSessionMapper.cs`.
- **Repozitorijum:** `RedisMessageRepository`, `RedisSessionRepository`.

### MongoDB (pun sadržaj entiteta)

- **Domain:** User, Client, Master, Job, ChatConversation, Review.
- **Persistence:** `MongoDb/Entities/UserDocument.cs`, `ClientDocument.cs`, `MasterDocument.cs`, `JobDocument.cs`, `ConversationDocument.cs`; Review koristi interni dokument u `ReviewRepository`.
- **Mapper:** `MongoDb/Mappers/UserMapper.cs`, `ClientMapper.cs`, `MasterMapper.cs`, `JobMapper.cs`, `ConversationMapper.cs`.
- **Repozitorijum:** `UserRepository`, `ClientRepository`, `MasterRepository`, `MongoJobRepository` + composite `JobRepository`, `ConversationRepository`, `ReviewRepository`.

### Neo4j (samo graf – minimalni čvorovi i relacije)

- **Ne čuva pun sadržaj.** Samo čvorovi User (id, role) i Job (id) + relacije INVITED, ACCEPTED_BY itd.
- **IUserGraphSync** / **Neo4jUserGraphRepository:** `SyncUserNode(userId, role)` – MERGE User čvor (id, role). Poziva se nakon Save(user) u AuthService/UserService.
- **IJobGraphRepository** / **Neo4jJobGraphRepository:** `MergeJobNode(jobId)`, `InviteMasters`, `GetInvitedMasters`, `AcceptMaster` – graf poslova i majstora.
- **IJobRepository** implementira **JobRepository** (composite): GetById/Save iz MongoDB preko `MongoJobRepository`, a InviteMasters/GetInvitedMasters/AcceptMaster + MergeJobNode iz Neo4j.

---

## Mapa putanja (gde je šta)

Sve putanje su relativne u odnosu na koren GIT repozitorijuma (`Majstorica/`). Backend projekat je u `Application/backend/`.

### Business model (Domain)

| Entitet          | Putanja                                                   |
| ---------------- | --------------------------------------------------------- |
| User             | `Application/backend/Domain/Entities/User.cs`             |
| Client           | `Application/backend/Domain/Entities/Client.cs`           |
| Master           | `Application/backend/Domain/Entities/Master.cs`           |
| Job              | `Application/backend/Domain/Entities/Job.cs`              |
| Review           | `Application/backend/Domain/Entities/Review.cs`           |
| ChatConversation | `Application/backend/Domain/Entities/ChatConversation.cs` |
| ChatMessage      | `Application/backend/Domain/Entities/ChatMessage.cs`      |
| UserSession      | `Application/backend/Domain/Entities/UserSession.cs`      |

Domenski enumi: `Domain/Enums/` (UserRole, JobStatus). Statičke fabrike za rekonstrukciju: `Rehydrate(...)` / `FromPersistence(...)` u entitetima.

### Persistence model (Entity model) i mapperi

| Baza        | Entity (Document/Node)   | Mapper                          | Putanja entiteta                                                                  | Putanja mappera                             |
| ----------- | ------------------------ | ------------------------------- | --------------------------------------------------------------------------------- | ------------------------------------------- |
| **MongoDB** | UserDocument             | UserMapper                      | `Application/backend/Infrastructure/Persistence/MongoDb/Entities/UserDocument.cs` | `.../MongoDb/Mappers/UserMapper.cs`         |
| MongoDB     | ClientDocument           | ClientMapper                    | `.../MongoDb/Entities/ClientDocument.cs`                                          | `.../MongoDb/Mappers/ClientMapper.cs`       |
| MongoDB     | MasterDocument           | MasterMapper                    | `.../MongoDb/Entities/MasterDocument.cs`                                          | `.../MongoDb/Mappers/MasterMapper.cs`       |
| MongoDB     | JobDocument              | JobMapper                       | `.../MongoDb/Entities/JobDocument.cs`                                             | `.../MongoDb/Mappers/JobMapper.cs`          |
| MongoDB     | ConversationDocument     | ConversationMapper              | `.../MongoDb/Entities/ConversationDocument.cs`                                    | `.../MongoDb/Mappers/ConversationMapper.cs` |
| MongoDB     | ReviewDocument (interni) | –                               | Unutar `.../MongoDb/Repositories/ReviewRepository.cs` (private class)             | Mapiranje inline u repozitorijumu           |
| **Redis**   | ChatMessageDocument      | ChatMessageMapper               | `.../Persistence/Redis/Entities/ChatMessageDocument.cs`                           | `.../Redis/Mappers/ChatMessageMapper.cs`    |
| Redis       | UserSessionDocument      | UserSessionMapper               | `.../Redis/Entities/UserSessionDocument.cs`                                       | `.../Redis/Mappers/UserSessionMapper.cs`    |
| **Neo4j**   | UserNode, JobNode        | UserGraphMapper, JobGraphMapper | `.../Persistence/Neo4j/Entities/`                                                 | `.../Neo4j/Mappers/`                        |

### Biblioteka za snimanje i učitavanje (Data Mapper sloj)

- **Interfejsi (Application sloj)** – rade isključivo sa business tipovima (`User`, `Client`, `Job`, itd.):  
  `Application/backend/Application/Interfaces/`

  - `IUserRepository.cs`, `IClientRepository.cs`, `IMasterRepository.cs`, `IJobRepository.cs`, `IReviewRepository.cs`, `IConversationRepository.cs`, `IMessageRepository.cs`, `ISessionRepository.cs`, `IJobGraphRepository.cs`, `IUserGraphSync.cs`

- **Implementacije (Infrastructure)** – primaju/vraćaju business tipove; unutra koriste Mapper (Business ↔ Entity) i upis/čitanje u bazu:
  - MongoDB: `Application/backend/Infrastructure/Persistence/MongoDb/Repositories/`
    - `UserRepository.cs`, `ClientRepository.cs`, `MasterRepository.cs`, `MongoJobRepository.cs`, `JobRepository.cs` (composite), `ConversationRepository.cs`, `ReviewRepository.cs`
  - Redis: `.../Persistence/Redis/Repositories/`
    - `RedisMessageRepository.cs`, `RedisSessionRepository.cs`
  - Neo4j: `.../Persistence/Neo4j/Repositories/`
    - `Neo4jUserGraphRepository.cs`, `Neo4jJobGraphRepository.cs`

Obrazac: **Data Mapper** (Fowler) – repozitorijum + mapper odvajaju business model od persistence modela; aplikacija koristi samo business entitete.
