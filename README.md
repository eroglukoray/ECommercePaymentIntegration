# E-Commerce Platform API

Bu repo, .NET 8 tabanlı, MediatR + FluentValidation + Serilog + Polly + HealthChecks + Prometheus metrikleri + Correlation-ID + Docker desteği içeren bir e-ticaret backend servisini barındırır.

## ⚡️ Özellikler

- **Clean Architecture**: API / Application / Infrastructure / Domain katmanları  
- **MediatR** ile CQRS komut/query  
- **FluentValidation** ile model validasyonu  
- **Polly**: Timeout, Retry, Circuit Breaker, Fallback  
- **Serilog**: Request logging + Correlation-ID  
- **Health Checks**: liveness & readiness  
- **Prometheus**: HTTP metrikleri (`/metrics`)  
- **Docker** & **docker-compose** desteği  

## 🚀 Başlarken

### Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download)  
- [Docker](https://www.docker.com/get-started) & [Docker Compose](https://docs.docker.com/compose/)  

### Lokal Çalıştırma

```bash
# Çözüm kökünde
dotnet restore
dotnet build
dotnet run --project ECommerce.API/ECommerce.API.csproj
