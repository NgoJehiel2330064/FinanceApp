FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FinanceApp/FinanceApp.csproj", "FinanceApp/"]
RUN dotnet restore "FinanceApp/FinanceApp.csproj"
COPY . .
WORKDIR "/src/FinanceApp"
RUN dotnet build "FinanceApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinanceApp.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5153
ENV PORT=5153
ENV ASPNETCORE_URLS=http://+:${PORT}
ENTRYPOINT ["dotnet", "FinanceApp.dll"]
