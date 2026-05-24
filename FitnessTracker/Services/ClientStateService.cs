using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using FitnessTracker.Models;

namespace FitnessTracker.Services;

public class ClientStateService
{
    private readonly ProtectedSessionStorage _session;
    private Client? _cache;

    public ClientStateService(ProtectedSessionStorage session)
    {
        _session = session;
    }

    public Client? CurrentClient => _cache;
    public bool IsLoggedIn => _cache != null;

    public event Action? OnChange;

    public async Task SetClientAsync(Client client)
    {
        _cache = client;
        await _session.SetAsync("current_client", client);
        OnChange?.Invoke();
    }

    public async Task LoadFromSessionAsync()
    {
        try
        {
            var result = await _session.GetAsync<Client>("current_client");
            _cache = result.Success ? result.Value : null;
        }
        catch
        {
            _cache = null;
        }
        OnChange?.Invoke();
    }

    public async Task LogoutAsync()
    {
        _cache = null;
        await _session.DeleteAsync("current_client");
        OnChange?.Invoke();
    }
}