# git-dash

Simple dashboard for git

## planned file hierarchy (separate ui and server applications)

```
./server/GitDash.sln
./server/src/*/*.cs
./ui/src/*/*{.ext}
```

## API (w.i.p.)

```
/repos
/git-status-report
/variable # POST { name, value }
```

### Post example:

Unsubstituted:

```
?? server/src/GitDashApi/GitDashApi.csproj
```


```sh
curl -X POST http://localhost:5100/variable -d '{"name": "api", "value": "server/src/GitDashApi"}' -H "Content-Type: application/json"
```

Substituted Result:

```
?? {api}/GitDashApi.csproj
```
