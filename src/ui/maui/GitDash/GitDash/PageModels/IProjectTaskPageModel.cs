using CommunityToolkit.Mvvm.Input;
using GitDash.Models;

namespace GitDash.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}