using System.Threading;
using System.Threading.Tasks;

namespace BombaProMaxWPF.Services;

/// <summary>
/// Contract for views or view-models that load data asynchronously and want
/// to participate in the shell-cache lifecycle:
/// <list type="bullet">
///   <item>The hosting <see cref="System.Windows.Controls.UserControl"/> calls
///   <see cref="EnsureLoadedAsync"/> from its <c>Loaded</c> event — idempotent,
///   so subsequent shows are no-ops.</item>
///   <item><see cref="RefreshAsync"/> resets <see cref="IsLoaded"/> and re-fetches,
///   used by an explicit user "Refresh" affordance.</item>
///   <item>The hosting view is responsible for cancelling the in-flight token
///   on <c>Unloaded</c> so a slow network call doesn't update a stale UI.</item>
/// </list>
/// </summary>
public interface IAsyncLoadable
{
    /// <summary>True once the first successful load has completed.</summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Loads data the first time it is invoked; no-op on subsequent calls.
    /// Implementations should set <see cref="IsLoaded"/> to true on success
    /// and surface failures via their own error/state properties.
    /// </summary>
    Task EnsureLoadedAsync(CancellationToken ct = default);

    /// <summary>
    /// Forces a reload regardless of <see cref="IsLoaded"/>. Implementations
    /// typically reset <see cref="IsLoaded"/> first so concurrent ensure-calls
    /// fall through to the fresh fetch.
    /// </summary>
    Task RefreshAsync(CancellationToken ct = default);
}
