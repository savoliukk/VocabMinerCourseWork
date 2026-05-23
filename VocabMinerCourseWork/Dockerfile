# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY VocabMinerCourseWork.Api.csproj ./
RUN dotnet restore VocabMinerCourseWork.Api.csproj

COPY . ./
RUN dotnet publish VocabMinerCourseWork.Api.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "VocabMinerCourseWork.Api.dll"]
