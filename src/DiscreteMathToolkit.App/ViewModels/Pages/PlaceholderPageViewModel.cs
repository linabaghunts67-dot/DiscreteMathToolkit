using DiscreteMathToolkit.App.Mvvm;

namespace DiscreteMathToolkit.App.ViewModels.Pages;

public sealed class PlaceholderPageViewModel : ViewModelBase, IPageViewModel
{
    public string Title => "Coming in Generation 3";
    public string Message => "This module's interactive page is being built and ships in the next generation. " +
                             "All underlying algorithms are already implemented and tested in the Core library.";
}
