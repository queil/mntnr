FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

COPY *.fsproj ./
RUN dotnet restore --locked-mode

COPY . ./
RUN dotnet publish -c Release -o out

FROM build

COPY --from=build /app/out /usr/local/bin
RUN chmod +x /usr/local/bin/mntnr
