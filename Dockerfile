## --- 1) Build aþamasý (SDK imajý) ---
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#WORKDIR /src
#
## 1.1) Sadece runtime’a ihtiyacý olan projelerin csproj’larýný kopyala
#COPY ECommerce.API/ECommerce.API.csproj           ECommerce.API/
#COPY ECommerce.Application/ECommerce.Application.csproj   ECommerce.Application/
#COPY ECommerce.Infrastructure/ECommerce.Infrastructure.csproj ECommerce.Infrastructure/
#COPY ECommerce.Domain/ECommerce.Domain.csproj     ECommerce.Domain/
#
## 1.2) API projesini restore et (hiç test.csproj aranmayacak)
#RUN dotnet restore ECommerce.API/ECommerce.API.csproj
#
## 1.3) Tüm kaynak kodunu kopyala
#COPY . .
#
## 1.4) API’yý publish et
#WORKDIR /src/ECommerce.API
#RUN dotnet publish -c Release -o /app/publish
#
## --- 2) Runtime aþamasý ---
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
#WORKDIR /app
#
## 2.1) Yayýnlanan dosyalarý al
#COPY --from=build /app/publish .
#
#EXPOSE 80
#ENTRYPOINT ["dotnet","ECommerce.API.dll"]
#

# 1) Build aþamasý
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1.1) Sadece API için gerekli proje dosyalarýný kopyala
COPY ECommerce.API/*.csproj           ./ECommerce.API/
COPY ECommerce.Application/*.csproj   ./ECommerce.Application/
COPY ECommerce.Infrastructure/*.csproj ./ECommerce.Infrastructure/
COPY ECommerce.Domain/*.csproj        ./ECommerce.Domain/
# (Test projelerine ihtiyaç yok, burada kopyalamýyoruz)

# 1.2) API projesini restore et
RUN dotnet restore ./ECommerce.API/ECommerce.API.csproj

# 1.3) Tüm kaynak kodu kopyala ve publish et
COPY . .
WORKDIR /src/ECommerce.API
RUN dotnet publish -c Release -o /app/publish

# 2) Runtime aþamasý
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 2.1) Build aþamasýndan çýkan yayýnlanmýþ dosyalarý al
COPY --from=build /app/publish .

# 2.2) Container içinde 80 portunu dinle
EXPOSE 80

# 2.3) Uygulamayý baþlat
ENTRYPOINT ["dotnet", "ECommerce.API.dll"]