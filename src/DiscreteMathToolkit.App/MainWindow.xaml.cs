using System.Windows;
using System.Windows.Input;
using DiscreteMathToolkit.App.ViewModels;
using DiscreteMathToolkit.App.ViewModels.Pages;

namespace DiscreteMathToolkit.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        PreviewKeyDown += OnPreviewKeyDown;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Don't intercept while a TextBox or other text-entry control has focus
        if (Keyboard.FocusedElement is System.Windows.Controls.TextBox) return;
        if (Keyboard.FocusedElement is System.Windows.Controls.Primitives.TextBoxBase) return;

        if (DataContext is not MainViewModel mvm) return;
        var page = mvm.CurrentPage;
        if (page == null) return;

        switch (e.Key)
        {
            case Key.Space:
                if (page is GraphTheoryViewModel g && g.TogglePlayCommand.CanExecute(null)) { g.TogglePlayCommand.Execute(null); e.Handled = true; }
                else if (page is TreesViewModel t && t.TogglePlayCommand.CanExecute(null)) { t.TogglePlayCommand.Execute(null); e.Handled = true; }
                break;
            case Key.Right:
                if (page is GraphTheoryViewModel g2 && g2.StepNextCommand.CanExecute(null)) { g2.StepNextCommand.Execute(null); e.Handled = true; }
                else if (page is TreesViewModel t2 && t2.StepNextCommand.CanExecute(null)) { t2.StepNextCommand.Execute(null); e.Handled = true; }
                break;
            case Key.Left:
                if (page is GraphTheoryViewModel g3 && g3.StepPreviousCommand.CanExecute(null)) { g3.StepPreviousCommand.Execute(null); e.Handled = true; }
                else if (page is TreesViewModel t3 && t3.StepPreviousCommand.CanExecute(null)) { t3.StepPreviousCommand.Execute(null); e.Handled = true; }
                break;
            case Key.R:
                if (page is GraphTheoryViewModel g4 && g4.ResetPlaybackCommand.CanExecute(null)) { g4.ResetPlaybackCommand.Execute(null); e.Handled = true; }
                else if (page is TreesViewModel t4 && t4.ResetPlaybackCommand.CanExecute(null)) { t4.ResetPlaybackCommand.Execute(null); e.Handled = true; }
                break;
        }
    }
}
