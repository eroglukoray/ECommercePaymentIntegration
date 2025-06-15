## --- 1) Build a�amas� (SDK imaj�) ---
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#WORKDIR /src
#
## 1.1) Sadece runtime�a ihtiyac� olan projelerin csproj�lar�n� kopyala
#COPY ECommerce.API/ECommerce.API.csproj           ECommerce.API/
#COPY ECommerce.Application/ECommerce.Application.csproj   ECommerce.Application/
#COPY ECommerce.Infrastructure/ECommerce.Infrastructure.csproj ECommerce.Infrastructure/
#COPY ECommerce.Domain/ECommerce.Domain.csproj     ECommerce.Domain/
#
## 1.2) API projesini restore et (hi� test.csproj aranmayacak)
#RUN dotnet restore ECommerce.API/ECommerce.API.csproj
#
## 1.3) T�m kaynak kodunu kopyala
#COPY . .
#
## 1.4) API�y� publish et
#WORKDIR /src/ECommerce.API
#RUN dotnet publish -c Release -o /app/publish
#
## --- 2) Runtime a�amas� ---
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
#WORKDIR /app
#
## 2.1) Yay�nlanan dosyalar� al
#COPY --from=build /app/publish .
#
#EXPOSE 80
#ENTRYPOINT ["dotnet","ECommerce.API.dll"]
#

# 1) Build a�amas�
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1.1) Sadece API i�in gerekli proje dosyalar�n� kopyala
COPY ECommerce.API/*.csproj           ./ECommerce.API/
COPY ECommerce.Application/*.csproj   ./ECommerce.Application/
COPY ECommerce.Infrastructure/*.csproj ./ECommerce.Infrastructure/
COPY ECommerce.Domain/*.csproj        ./ECommerce.Domain/
# (Test projelerine ihtiya� yok, burada kopyalam�yoruz)

# 1.2) API projesini restore et
RUN dotnet restore ./ECommerce.API/ECommerce.API.csproj

# 1.3) T�m kaynak kodu kopyala ve publish et
COPY . .
WORKDIR /src/ECommerce.API
RUN dotnet publish -c Release -o /app/publish

# 2) Runtime a�amas�
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 2.1) Build a�amas�ndan ��kan yay�nlanm�� dosyalar� al
COPY --from=build /app/publish .

# 2.2) Container i�inde 80 portunu dinle
EXPOSE 80

# 2.3) Uygulamay� ba�lat
ENTRYPOINT ["dotnet", "ECommerce.API.dll"]