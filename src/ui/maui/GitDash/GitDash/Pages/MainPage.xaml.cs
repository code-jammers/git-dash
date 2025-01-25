using GitDash.Models;
using GitDash.PageModels;
using System.ComponentModel;

namespace GitDash.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;

            bool isVarEditorOpen = false;
            string selection = "";

            var editor = new Editor
            {
                Text = "my/example/path/File.cs",
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
                        await ShowVarEditPopup(selection);
                        isVarEditorOpen = false; editor.SelectionLength = 0; selection = ""; lastCheckedSelectionLength = 0;
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

            // Layout
            var layout = new VerticalStackLayout
            {
                Padding = new Thickness(20),
                Children = { editor }
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

        private async Task ShowVarEditPopup(string selectedText)
        {
            string varName = await DisplayPromptAsync(
                "Add A Shortened Path Variable",
                $"Path value: \"{selectedText}\"",
                initialValue: string.Empty,
                placeholder: "var_name"
            );
            if (string.IsNullOrWhiteSpace(varName)) return;
            if (varName.ToCharArray().Any(c => ('\t' + Environment.NewLine + " {}").Contains(c))) return;

            // todo:store varName and value
        }
    }
}