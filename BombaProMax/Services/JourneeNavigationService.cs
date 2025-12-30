using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BombaProMax.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BombaProMax.Services;

/// <summary>
/// Centralized service for managing the guided "Journée" workflow.
/// Handles navigation order, step tracking, and lifecycle management.
/// </summary>
public partial class JourneeNavigationService : ObservableObject
{
    // Ordered list of routes for the journée workflow
    private readonly List<string> _journeeRoutes =
    [
        "//ClientPage",                     // Step 1: Manage clients
        "//AchatPage",                      // Step 2: Record purchases
        "//JaugeagePage",                   // Step 3: Jaugeage measurements
        "//PeriodePage",                    // Step 4: Sales periods
        "//VenteLubrifiantsEtArticlesPage", // Step 5: Lubricants & articles sales
        "//VenteServicePage",               // Step 6: Service sales
        "//DepensePage"                     // Step 7: Expenses
    ];

    [ObservableProperty]
    private JourneeState _state = JourneeState.None;

    [ObservableProperty]
    private int _currentStepIndex = -1;

    [ObservableProperty]
    private string _currentStepName = string.Empty;

    /// <summary>
    /// Total number of steps in the journée workflow.
    /// </summary>
    public int TotalSteps => _journeeRoutes.Count;

    /// <summary>
    /// Current step number (1-based for display).
    /// </summary>
    public int CurrentStepNumber => CurrentStepIndex + 1;

    /// <summary>
    /// Whether the user can navigate to the previous step.
    /// </summary>
    public bool CanGoPrevious => State == JourneeState.Active && CurrentStepIndex > 0;

    /// <summary>
    /// Whether the user can navigate to the next step.
    /// </summary>
    public bool CanGoNext => State == JourneeState.Active && CurrentStepIndex < _journeeRoutes.Count - 1;

    /// <summary>
    /// Whether this is the first step.
    /// </summary>
    public bool IsFirstStep => CurrentStepIndex == 0;

    /// <summary>
    /// Whether this is the last step.
    /// </summary>
    public bool IsLastStep => CurrentStepIndex == _journeeRoutes.Count - 1;

    /// <summary>
    /// Whether the journée is currently active.
    /// </summary>
    public bool IsJourneeActive => State == JourneeState.Active;

    /// <summary>
    /// Starts the journée workflow and navigates to the first step.
    /// </summary>
    public async Task StartJourneeAsync()
    {
        State = JourneeState.Active;
        CurrentStepIndex = 0;
        UpdateCurrentStepName();
        
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(IsJourneeActive));
        OnPropertyChanged(nameof(CurrentStepNumber));

        await NavigateToCurrentStepAsync();
    }

    /// <summary>
    /// Ends the journée workflow and returns to HomePage.
    /// </summary>
    public async Task EndJourneeAsync()
    {
        State = JourneeState.Finished;
        CurrentStepIndex = -1;
        CurrentStepName = string.Empty;

        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(IsJourneeActive));
        OnPropertyChanged(nameof(CurrentStepNumber));

        // Navigate back to HomePage
        await Shell.Current.GoToAsync("//HomePage");
        
        // Reset state after navigation
        State = JourneeState.None;
        OnPropertyChanged(nameof(IsJourneeActive));
    }

    /// <summary>
    /// Navigates to the next step in the workflow.
    /// </summary>
    /// <param name="skipped">Whether the current step was skipped.</param>
    public async Task GoNextAsync(bool skipped = false)
    {
        if (!CanGoNext)
        {
            // Last step - end the journée
            await EndJourneeAsync();
            return;
        }

        CurrentStepIndex++;
        UpdateCurrentStepName();
        
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(CurrentStepNumber));

        await NavigateToCurrentStepAsync();
    }

    /// <summary>
    /// Navigates to the previous step in the workflow.
    /// </summary>
    public async Task GoPreviousAsync()
    {
        if (!CanGoPrevious) return;

        CurrentStepIndex--;
        UpdateCurrentStepName();
        
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(CurrentStepNumber));

        await NavigateToCurrentStepAsync();
    }

    /// <summary>
    /// Checks if a given route is allowed during the active journée.
    /// </summary>
    public bool IsRouteAllowedDuringJournee(string route)
    {
        if (State != JourneeState.Active) return true;

        // Allow navigation to the current step's route
        if (CurrentStepIndex >= 0 && CurrentStepIndex < _journeeRoutes.Count)
        {
            var currentRoute = _journeeRoutes[CurrentStepIndex];
            
            // Check if navigating to the current step's route
            if (route.Contains(currentRoute.TrimStart('/'), StringComparison.OrdinalIgnoreCase))
                return true;
            
            // Allow sub-routes based on current step
            // ClientPage allows navigation to ClientCreditManagement
            if (currentRoute == "//ClientPage" && 
                route.Contains("ClientCreditManagement", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the route name for a given step index.
    /// </summary>
    public string GetRouteDisplayName(int index)
    {
        if (index < 0 || index >= _journeeRoutes.Count) return string.Empty;
        
        var route = _journeeRoutes[index];
        return route.Replace("//", "").Replace("Page", "");
    }

    private async Task NavigateToCurrentStepAsync()
    {
        if (CurrentStepIndex < 0 || CurrentStepIndex >= _journeeRoutes.Count) return;

        var route = _journeeRoutes[CurrentStepIndex];
        await Shell.Current.GoToAsync(route);
    }

    private void UpdateCurrentStepName()
    {
        CurrentStepName = GetRouteDisplayName(CurrentStepIndex);
    }
}
