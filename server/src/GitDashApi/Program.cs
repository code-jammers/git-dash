
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/repos", () =>
{
    string thisRepoPath; // -> { repo selector (ui), git status report refresh }
    {
        var upPath = @"..\..\..".Replace(@"\", Path.DirectorySeparatorChar + "");
        var thisSourceFileDirPath = Path.GetDirectoryName(SP.SourcePath())!;
        thisRepoPath = Path.GetFullPath(Path.Combine(thisSourceFileDirPath, upPath));
    }

    //var gitDashRepoName = new DirectoryInfo(thisRepoPath!).Name;
    var parentDirectory = Directory.GetParent(thisRepoPath!);
    var directories = Directory.GetDirectories(parentDirectory.ToString())
        .Where(d => d != thisRepoPath);
    //Console.WriteLine(gitDashRepoName);
    //Console.WriteLine(thisRepoPath);
    return new RepositoryGroup
    {
        Default = thisRepoPath,
        Siblings = new(directories!)
    };

}).WithName("Repos")
.WithOpenApi();

app.MapGet("/git-status-report", () =>
{
    string thisRepoPath; // -> { repo selector (ui), git status report refresh }
    {
        var upPath = @"..\..\..".Replace(@"\", Path.DirectorySeparatorChar + "");
        var thisSourceFileDirPath = Path.GetDirectoryName(SP.SourcePath())!;
        thisRepoPath = Path.GetFullPath(Path.Combine(thisSourceFileDirPath, upPath));
    }
    var report = new GitDash.Services.GitStatusReport();
    report.Refresh(thisRepoPath);
    return report;
}).WithName("GitStatusReport")
.WithOpenApi();

app.MapPost("/variable", (Variable pathVar) =>
{
    // Console.WriteLine(pathVar.Name);
    // Console.WriteLine(pathVar.Value);
    new GitDash.Services.VariableStorage().Add(pathVar.Name, pathVar.Value);
    return Results.Ok(new{});
})
.WithName("Variable")
.WithOpenApi();

app.Run();


public static class SP
{
    public static string SourcePath([System.Runtime.CompilerServices.CallerFilePath] string path = null)
    {
        return path;
    }
}

public class RepositoryGroup
{
    public string Default { get; set; } = null!;
    public List<string> Siblings { get; set; }
}

public class Variable
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}
