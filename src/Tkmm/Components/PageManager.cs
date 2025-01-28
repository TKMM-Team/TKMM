using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using Tkmm.Models;

namespace Tkmm.Components;

public enum Page
{
    Home,
    Profiles,
    Tools,
    ShopParam,
    GbMods,
    About,
    Logs,
    NetworkSettings,
    Settings,
}

public partial class PageManager : ObservableObject
{
    private static readonly Lazy<PageManager> _shared = new(
        () => new PageManager()
    );
    
    public static PageManager Shared => _shared.Value;

    [ObservableProperty]
    private PageModel? _current;

    [ObservableProperty]
    private Page _default;

    private readonly Dictionary<Page, (int Index, bool IsFooter)> _lookup = [];
    public ObservableCollection<PageModel> Pages { get; } = [];
    public ObservableCollection<PageModel> FooterPages { get; } = [];

    public PageModel this[Page page] {
        get {
            (int index, bool isFooter) = _lookup[page];
            return (isFooter ? FooterPages : Pages)[index];
        }
    }

    public void Register(Page page, TkLocale title, object? content, Symbol icon, TkLocale description, bool isDefault = false, bool isFooter = false)
    {
        ObservableCollection<PageModel> source = isFooter ? FooterPages : Pages;
        _lookup[page] = (source.Count, isFooter);

        source.Add(new PageModel {
            Title = Locale[title],
            Content = content,
            Description = Locale[description],
            Icon = icon
        });

        if (isDefault) {
            Default = page;
        }
    }

    public void Focus(Page page)
    {
        Current = this[page];
    }

    public T Get<T>(Page page) where T : ObservableObject
    {
        (int index, bool isFooter) = _lookup[page];
        if ((isFooter ? FooterPages : Pages)[index].Content is UserControl { DataContext: T value }) {
            return value;
        }

        throw new InvalidOperationException(
            $"Invalid ViewModel type for '{page}'");
    }
}
