# Dot Net Command Log

## Initial dotnet setup steps

```sh
git clone git@github.com:code-jammers/git-dash.git

cd git-dash/server

dotnet new sln --name GitDash

cd src/
dotnet new webapi --name GitDashApi

cd ../
dotnet sln GitDash.sln src/GitDashApi/GitDashApi.csproj

dotnet build GitDash.sln
dotnet build src/GitDashApi/GitDashApi.csproj

cd src/GitDashApi
dotnet run
```

## Add initial packages

```sh
cd git-dash/server
dotnet add src/GitDashApi/GitDashApi.csproj package LibGit2Sharp
dotnet add src/GitDashApi/GitDashApi.csproj package Microsoft.Data.Sqlite
dotnet add src/GitDashApi/GitDashApi.csproj package System.Runtime.CompilerServices.Unsafe
```
