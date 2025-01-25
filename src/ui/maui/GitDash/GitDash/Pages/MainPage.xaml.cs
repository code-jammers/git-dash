using Microsoft.Data.Sqlite;
using System.Runtime.CompilerServices;

namespace GitDash.Pages
{
    public partial class MainPage : ContentPage
    {
        private static string SourcePath([CallerFilePath] string path = null)
        {
            return path;
        }
        public MainPage(MainPageModel model, GitStatusReport gitStatusReport, VariableStorage variableStorage, VariableSubstitution variableSubstitution)
        {
            InitializeComponent();
            BindingContext = model;

            string thisRepoPath; // -> { repo selector (ui), git status report refresh }
            {
                var upPath = @"..\..\..\..\..\..".Replace(@"\", Path.DirectorySeparatorChar + "");
                var thisSourceFileDirPath = Path.GetDirectoryName(SourcePath())!;
                thisRepoPath = Path.GetFullPath(Path.Combine(thisSourceFileDirPath, upPath));
            }

            gitStatusReport.Refresh(thisRepoPath);

            bool isVarEditorOpen = false;
            string selection = "";
            var repositoryPathEditor = new Editor
            {
                Text = thisRepoPath,
                FontSize = 12,
                TextColor = Colors.WhiteSmoke,
                BackgroundColor = Colors.DarkSlateGray,
                AutoSize = EditorAutoSizeOption.TextChanges,
                MaximumHeightRequest = 25
            };

            var editor = new Editor
            {
                Text = variableSubstitution.Substitute(gitStatusReport.FilesAsString, variableStorage.GetAll()),
                FontSize = 16,
                TextColor = Colors.Black,
                BackgroundColor = Colors.LightGray,
                IsReadOnly = true,
                AutoSize = EditorAutoSizeOption.TextChanges
            };
            editor.TextChanged += (s, e) =>
            {
                editor.Text = e.OldTextValue;
            };




            var lastCheckedSelectionLength = 0;
            var selectionDoneTimer = new System.Timers.Timer();
            selectionDoneTimer.Interval = 700;
            selectionDoneTimer.Elapsed += (s, e) =>
            {
                if (isVarEditorOpen) return;
                if (selection.Length == 0) return;
                if (selection.Contains("{") || selection.Contains("}")) return; // can't nest variables
                if (selection.Contains(Environment.NewLine)) return; // can't turn multiple paths into 1 variable
                if (lastCheckedSelectionLength == selection.Length)
                {
                    isVarEditorOpen = true;
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await ShowVarEditPopup(selection, variableStorage);
                        isVarEditorOpen = false; editor.SelectionLength = 0; selection = ""; lastCheckedSelectionLength = 0;
                        variableStorage.GetAll();
                        editor.Text = variableSubstitution.Substitute(gitStatusReport.FilesAsString, variableStorage.GetAll());
                    });
                }
                else
                {
                    lastCheckedSelectionLength = selection.Length;
                }
            };
            selectionDoneTimer.Start();

            editor.PropertyChanged += (s, e) =>

            {
                if (e.PropertyName != nameof(Editor.SelectionLength)) return;
                if (isVarEditorOpen || editor.SelectionLength == 0) return;

                var selectedText = GetSelectedText(editor);
                selection = selectedText;
            };

            var scrollView = new ScrollView();
            scrollView.Content = editor;
            scrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Always;
            scrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;

            var imageButton = new ImageButton();
            imageButton.IsEnabled = false;
            imageButton.WidthRequest = 25;
            imageButton.HeightRequest = 25;
            if (gitStatusReport.HasUpstreamDevelopmentBranchChanges)
            {
                imageButton.Source = ImageSource.FromUri(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/6/67/Go-down_Gion.svg/1024px-Go-down_Gion.svg.png"));
            }
            else
            {
                imageButton.Source = ImageSource.FromUri(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/8/8f/Checkmark.svg/540px-Checkmark.svg.png"));
            }

            var branchLabel = new Label
            {
                Text = gitStatusReport.CurrentBranchName
            };
            var commitLabel = new Label
            {
                Text = gitStatusReport.ParentChildCommitPair[1] + " Parent(s): " + gitStatusReport.ParentChildCommitPair[0],
                FontSize = 12,
                MaximumHeightRequest = 25
            };
            var commitMessageLabel = new Label
            {
                Text = gitStatusReport.LastCommitMessageShort,
                FontSize = 12,
                MaximumHeightRequest = 25
            };



            var refreshTimer = new System.Timers.Timer();
            refreshTimer.Interval = 60 * 1000;
            refreshTimer.Elapsed += (s, e) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    gitStatusReport.Refresh(thisRepoPath);
                    editor.Text = variableSubstitution.Substitute(gitStatusReport.FilesAsString, variableStorage.GetAll());
                    commitLabel.Text = gitStatusReport.ParentChildCommitPair[1] + " Parent(s): " + gitStatusReport.ParentChildCommitPair[0];
                    commitMessageLabel.Text = gitStatusReport.LastCommitMessageShort;
                    branchLabel.Text = gitStatusReport.CurrentBranchName;
                    if (gitStatusReport.HasUpstreamDevelopmentBranchChanges)
                    {
                        imageButton.Source = ImageSource.FromUri(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/6/67/Go-down_Gion.svg/1024px-Go-down_Gion.svg.png"));
                    }
                    else
                    {
                        imageButton.Source = ImageSource.FromUri(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/8/8f/Checkmark.svg/540px-Checkmark.svg.png"));
                    }
                });
            };
            refreshTimer.Start();

            var topBarLayout = new HorizontalStackLayout
            {
                Padding = new Thickness(20),
                Children = {
                    new VerticalStackLayout { Children = { imageButton, branchLabel } },
                    new VerticalStackLayout { Children = { repositoryPathEditor, commitLabel, commitMessageLabel } }
                }
            };
            // Layout
            var layout = new VerticalStackLayout
            {
                Padding = new Thickness(20),
                Children = { topBarLayout, scrollView }
            };

            Content = layout;
        }
        private string GetSelectedText(Editor editor)
        {
            if (editor.CursorPosition < editor.Text.Length)
            {
                int selectionStart = editor.CursorPosition;
                int selectionLength = editor.SelectionLength;

                if (selectionLength > 0)
                {
                    return editor.Text.Substring(selectionStart, selectionLength);
                }
            }
            return string.Empty;
        }

        private async Task ShowVarEditPopup(string selectedText, VariableStorage varStorage)
        {
            string varName = await DisplayPromptAsync(
                "Add A Shortened Path Variable",
                $"Path value: \"{selectedText}\"",
                initialValue: string.Empty,
                placeholder: "var_name"
            );
            if (string.IsNullOrWhiteSpace(varName)) return;
            if (varName.ToCharArray().Any(c => ('\t' + Environment.NewLine + " {}").Contains(c))) return;

            varStorage.Add(varName, selectedText);
        }
    }
}