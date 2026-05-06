using System;
using System.Windows.Controls;

namespace BombaProMaxWPF.Services;

/// <summary>
/// Lightweight wrapper around a <see cref="Frame"/> so view models can request navigation
/// without holding a reference to a UI element.
/// </summary>
public sealed class NavigationService
{
    private Frame? _frame;

    public void SetFrame(Frame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        _frame = frame;
    }

    public void NavigateTo(Type pageType)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        if (_frame is null)
        {
            throw new InvalidOperationException("NavigationService has no Frame attached. Call SetFrame first.");
        }

        var page = Activator.CreateInstance(pageType)
            ?? throw new InvalidOperationException($"Unable to create page of type {pageType.FullName}.");
        _frame.Navigate(page);
    }
}
