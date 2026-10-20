using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace GuysBee.Services.Shop;

public record ConnectionResult(bool Ok, string Message);

/// <summary>Vérifie les identifiants PayPal et Colissimo, et fournit le jeton du widget de points relais.</summary>
public class IntegrationConnector
{
    public const string ColissimoBaseUrl = "https://ws.colissimo.fr";

    private readonly IHttpClientFactory _http;
    public IntegrationConnector(IHttpClientFactory http) => _http = http;

    public async Task<ConnectionResult> TestPayPalAsync(PayPalSettings s, CancellationToken ct = default)
    {
        if (!s.IsConfigured) return new(false, "Renseignez l'identifiant client et la clé secrète PayPal.");
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{s.ApiBaseUrl}/v1/oauth2/token")
            {
                Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{s.ClientId}:{s.Secret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
            using var response = await _http.CreateClient("integrations").SendAsync(request, ct);
            var mode = s.IsLive ? "réel" : "test (sandbox)";
            return response.IsSuccessStatusCode
                ? new(true, $"Connexion PayPal réussie en mode {mode}.")
                : new(false, $"PayPal a refusé les identifiants en mode {mode}. Vérifiez qu'ils correspondent bien à ce mode.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new(false, "Impossible de joindre PayPal pour le moment. Réessayez dans un instant.");
        }
    }

    public async Task<(string? Token, string Message)> GetColissimoTokenAsync(ColissimoSettings s, CancellationToken ct = default)
    {
        if (!s.HasCredentials) return (null, "Renseignez le numéro de contrat et le mot de passe Colissimo.");
        try
        {
            using var response = await _http.CreateClient("integrations").PostAsJsonAsync(
                $"{ColissimoBaseUrl}/widget-colissimo/rest/authenticate.rest",
                new { login = s.ContractNumber, password = s.Password }, ct);
            if (!response.IsSuccessStatusCode)
                return (null, "Colissimo a refusé les identifiants. Vérifiez le numéro de contrat et le mot de passe.");
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            var token = doc.RootElement.TryGetProperty("token", out var t) ? t.GetString() : null;
            return string.IsNullOrWhiteSpace(token)
                ? (null, "Colissimo n'a pas fourni d'accès au choix de point relais pour ce contrat.")
                : (token, "Connexion Colissimo réussie.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return (null, "Impossible de joindre Colissimo pour le moment. Réessayez dans un instant.");
        }
    }

    public async Task<ConnectionResult> TestColissimoAsync(ColissimoSettings s, CancellationToken ct = default)
    {
        var (token, message) = await GetColissimoTokenAsync(s, ct);
        return new(token is not null, message);
    }
}
