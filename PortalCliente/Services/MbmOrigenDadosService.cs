using System.Net.Http.Json;
using PortalCliente.Models;

namespace PortalCliente.Services;

public class MbmOrigenDadosService : IOrigenDadosService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public MbmOrigenDadosService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _baseUrl = configuration["MBM:BaseUrl"]?.TrimEnd('/') ?? "";
    }

    public async Task<List<Container>> GetIsotanquesAsync(string? cliente, string? status, string? busca, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(cliente)) query.Add($"cliente={Uri.EscapeDataString(cliente)}");
        if (!string.IsNullOrWhiteSpace(status))  query.Add($"status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrWhiteSpace(busca))   query.Add($"busca={Uri.EscapeDataString(busca)}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";

        try
        {
            var list = await _http.GetFromJsonAsync<List<Container>>($"{_baseUrl}/api/isotanques{qs}", cancellationToken);
            return list ?? new List<Container>();
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                "Não foi possível conectar ao MBM. Verifique se o Simulador MBM está rodando em " + _baseUrl + ". " +
                "Se já rodou o Simulador antes, apague o arquivo mbm.db na pasta SimuladorMBM e rode de novo.", ex);
        }
    }

    public async Task<Container?> GetIsotanquePorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        var encoded = Uri.EscapeDataString(codigo.Trim());
        try
        {
            var response = await _http.GetAsync($"{_baseUrl}/api/isotanques/{encoded}", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadFromJsonAsync<Container>(cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<string>> GetClientesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _http.GetFromJsonAsync<List<string>>($"{_baseUrl}/api/clientes", cancellationToken);
            return list ?? new List<string>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Não foi possível conectar ao MBM. Verifique se o Simulador MBM está rodando em " + _baseUrl + ".");
        }
    }

    public async Task<IReadOnlyList<string>> GetStatusListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _http.GetFromJsonAsync<List<string>>($"{_baseUrl}/api/status", cancellationToken);
            return (IReadOnlyList<string>?)(list ?? new List<string>()) ?? Array.Empty<string>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Não foi possível conectar ao MBM. Verifique se o Simulador MBM está rodando em " + _baseUrl + ".");
        }
    }
}