FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y python3 python3-pip python3-venv \
    && rm -rf /var/lib/apt/lists/*
RUN python3 -m venv /opt/venv
ENV PATH="/opt/venv/bin:$PATH"
RUN pip install scikit-learn numpy
COPY Infrastructure/Scripts/model.pkl /app/
COPY Infrastructure/Scripts/predict.py /app/

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ManutencaoPreditiva.Api.csproj", "."]
RUN dotnet restore "./ManutencaoPreditiva.Api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ManutencaoPreditiva.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ManutencaoPreditiva.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV PATH="/opt/venv/bin:$PATH"
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ManutencaoPreditiva.Api.dll"]
