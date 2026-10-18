namespace WorldState.Domain.Navigation;

public enum ScreenRoute
{
    MainMenu,
    CountrySelect,
    WorldMap,
    GovernmentCabinet,
    TasksAndObjectives,
    EconomyAndBudget,
    DiplomacyAndTreaties,
    MilitaryAndDefense,
    SecurityAndEspionage,
    EventsNewsfeed,
    Achievements,
    Settings
}

public class NavigationService
{
    private readonly Stack<ScreenRoute> _history = new();

    public ScreenRoute CurrentRoute { get; private set; } = ScreenRoute.MainMenu;
    public bool CanNavigateBack => _history.Count > 0;

    public event Action<ScreenRoute>? OnRouteChanged;

    public void NavigateTo(ScreenRoute newRoute)
    {
        if (newRoute == CurrentRoute)
        {
            return;
        }

        _history.Push(CurrentRoute);
        CurrentRoute = newRoute;
        OnRouteChanged?.Invoke(CurrentRoute);
    }

    public bool NavigateBack()
    {
        if (!CanNavigateBack)
        {
            return false;
        }

        CurrentRoute = _history.Pop();
        OnRouteChanged?.Invoke(CurrentRoute);
        return true;
    }

    public void ResetTo(ScreenRoute rootRoute)
    {
        _history.Clear();
        CurrentRoute = rootRoute;
        OnRouteChanged?.Invoke(CurrentRoute);
    }
}
