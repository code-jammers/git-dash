using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace GitDash.Services;

public class GitStatusReport
{
    public string CurrentBranchName { get; set; } = "";
    public string FilesAsString { get; set; } = "";
    public bool HasUpstreamDevelopmentBranchChanges { get; set; } = false;
    public string[] ParentChildCommitPair { get; set; } = new string[2];
    public string LastCommitMessageShort { get; set; } = "";
    public void Refresh(string repositoryPath)
    {
        this.FilesAsString = "";
        using var repo = new Repository(repositoryPath);
        CurrentBranchName = repo.Branches.FirstOrDefault(b => b.IsCurrentRepositoryHead)?.FriendlyName ?? "";

        var commits = repo.Branches.FirstOrDefault(b => b.IsCurrentRepositoryHead)?
            .Commits
            .ToList();
        if (commits != null && commits.Count() >= 2)
        {
            ParentChildCommitPair[0] = string.Join(',', commits[0]
                .Parents
                .Select(c => c.Id.ToString().Substring(0,4))
                .ToList());
            ParentChildCommitPair[1] = commits[0].Id.ToString().Substring(0, 4);
            LastCommitMessageShort = commits[0].MessageShort;
        }
        Commands.Fetch(repo, "origin",
            repo.Network.Remotes["origin"].FetchRefSpecs.Select(r => r.Specification),
            options: new FetchOptions() { }, "Fetching remote");
        string developmentBranch = "master";
        foreach (var branch in repo.Branches)
        {
            if (developmentBranch != "develop" && branch.FriendlyName == "main")
            {
                developmentBranch = "main";
            } else if (branch.FriendlyName == "develop")
            {
                developmentBranch = "develop";
            }

        }
        foreach (var branch in repo.Branches)
        {
            if (branch.FriendlyName == developmentBranch)
            {
                HasUpstreamDevelopmentBranchChanges = branch.TrackingDetails.BehindBy != 0;
            }
        }
        foreach (var fileStatus in repo.RetrieveStatus())
        {
            if (fileStatus.State == FileStatus.ModifiedInWorkdir)
            {
                this.FilesAsString += (this.FilesAsString.Length == 0)
                    ? "M? " + fileStatus.FilePath
                    : Environment.NewLine + "M? " + fileStatus.FilePath;
            }

            if (fileStatus.State == FileStatus.ModifiedInIndex)
            {
                this.FilesAsString += (this.FilesAsString.Length == 0)
                    ? "M " + fileStatus.FilePath
                    : Environment.NewLine + "M " + fileStatus.FilePath;
            }

            if (fileStatus.State == FileStatus.NewInWorkdir)
            {
                this.FilesAsString += (this.FilesAsString.Length == 0)
                    ? "?? " + fileStatus.FilePath
                    :   Environment.NewLine + "?? " + fileStatus.FilePath;
            }

            if (fileStatus.State == FileStatus.NewInIndex)
            {
                this.FilesAsString += (this.FilesAsString.Length == 0)
                    ? "? " + fileStatus.FilePath
                    : Environment.NewLine + "? " + fileStatus.FilePath;
            }

            if (fileStatus.State == FileStatus.DeletedFromWorkdir)
            {
                this.FilesAsString += (this.FilesAsString.Length == 0)
                    ? "D? " + fileStatus.FilePath
                    : Environment.NewLine + "D? "+ fileStatus.FilePath;
            }

            if (fileStatus.State == FileStatus.DeletedFromIndex)
            {
                this.FilesAsString += (this.FilesAsString.Length == 0)
                    ? "D " + fileStatus.FilePath
                    : Environment.NewLine + "D " + fileStatus.FilePath;
            }
        }
    }
}
