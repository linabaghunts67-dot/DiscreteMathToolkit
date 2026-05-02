using CommunityToolkit.Mvvm.ComponentModel;

namespace DiscreteMathToolkit.App.Mvvm;

/// <summary>Base for all view models. Inherits property change notifications from CommunityToolkit.</summary>
public abstract class ViewModelBase : ObservableObject
{
}

/// <summary>Marker interface for view models that represent a navigable page.</summary>
public interface IPageViewModel
{
    string Title { get; }
}
