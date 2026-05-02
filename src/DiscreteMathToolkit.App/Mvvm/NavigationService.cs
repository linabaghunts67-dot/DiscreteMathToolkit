using Microsoft.Extensions.DependencyInjection;

namespace DiscreteMathToolkit.App.Mvvm;

public interface INavigationService
{
    IPageViewModel? CurrentPage { get; }
    event Action<IPageViewModel>? PageChanged;
    void NavigateTo<TPage>() where TPage : IPageViewModel;
    void NavigateTo(Type pageType);
}

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;

    public IPageViewModel? CurrentPage { get; private set; }
    public event Action<IPageViewModel>? PageChanged;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public void NavigateTo<TPage>() where TPage : IPageViewModel =>
        NavigateTo(typeof(TPage));

    public void NavigateTo(Type pageType)
    {
        if (!typeof(IPageViewModel).IsAssignableFrom(pageType))
            throw new ArgumentException($"Type {pageType} does not implement {nameof(IPageViewModel)}.", nameof(pageType));

        var page = (IPageViewModel)_services.GetRequiredService(pageType);
        CurrentPage = page;
        PageChanged?.Invoke(page);
    }
}
