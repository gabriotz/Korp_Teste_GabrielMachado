namespace FaturamentoService.Domain.Entities;

public class IdempotencyRecord
{
    public string Key { get; private set; } = string.Empty;
    public string Response { get; private set; } = string.Empty;
    public int StatusCode { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private IdempotencyRecord() { }

    public static IdempotencyRecord Criar(string key, string response, int statusCode)
    {
        return new IdempotencyRecord
        {
            Key = key,
            Response = response,
            StatusCode = statusCode,
            CriadoEm = DateTime.UtcNow
        };
    }
}