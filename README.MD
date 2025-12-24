Push Messaging Case

Bu proje, saniyede 1000 RPS alabilen, JWT ile korunan ve mesajları RabbitMQ üzerinden Firebase’e push eden bir sistem örneğidir.

Amaç; yük ayrımı, rate limiting, cache kullanımı ve async processing yaklaşımlarını göstermektir.

--------------------------------------------------
GENEL MİMARİ
--------------------------------------------------

Client
  |
  v
API (JWT + Rate Limit)
  |
  v
RabbitMQ
  |
  v
Consumer (Firebase Push)

- API: CPU-bound
- Consumer: IO-bound
- Yükler bilinçli olarak ayrılmıştır

--------------------------------------------------
PROJE YAPISI
--------------------------------------------------

src/
 ├── Push.Messaging.Api
 ├── Push.Messaging.Application
 ├── Push.Messaging.Infrastructure
 ├── Push.Messaging.Data
 ├── Push.Messaging.Consumer
 └── Push.Messaging.Shared

--------------------------------------------------
KULLANILAN TEKNOLOJİLER
--------------------------------------------------

- .NET 8
- ASP.NET Core Web API
- JWT (HS256)
- Redis (cache + rate limit)
- RabbitMQ
- Firebase HTTP API
- Docker Compose

--------------------------------------------------
AUTHENTICATION
--------------------------------------------------

- JWT, framework built-in AddJwtBearer ile doğrulanır
- Token üretimi TokenService üzerinden yapılır
- Token içeriği:
  - sub -> userId
  - unique_name -> username

--------------------------------------------------
RATE LIMITING (RPS)
--------------------------------------------------

- Redis + Lua script
- Limit: 1000 request / saniye / kullanıcı
- Lua sayesinde atomik işlem ve race condition önlenir

--------------------------------------------------
CACHE STRATEJİSİ
--------------------------------------------------

- Kullanıcılar uygulama açılışında cache’e yüklenir
- Cache refresh: background worker ile 5 dakikada bir
- DB’ye gitmeden cache üzerinden kontrol yapılır

--------------------------------------------------
MESSAGING
--------------------------------------------------

- API mesajı RabbitMQ kuyruğuna publish eder
- Consumer:
  - Prefetch ile kontrollü paralellik
  - Ack / Nack yönetimi
  - Firebase’e HTTP push

--------------------------------------------------
ÇALIŞTIRMA
--------------------------------------------------

docker compose up -d

API ve Consumer projeleri ayrı process’lerdir, birlikte çalıştırılmalıdır.

--------------------------------------------------
NOTLAR
--------------------------------------------------

- Logging, retry, DLQ gibi konular bilinçli olarak sade tutulmuştur