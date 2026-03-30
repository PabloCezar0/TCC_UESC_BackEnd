using Microsoft.Extensions.Hosting;

namespace Rota.Application.Services;

public interface IEmailTemplateService
{
    Task<string> RenderAsync(string templateName,
                             IDictionary<string,string> data,
                             CancellationToken ct = default);
}

public sealed class EmailTemplateService : IEmailTemplateService
{
    private readonly IHostEnvironment _env;
    public EmailTemplateService(IHostEnvironment env) => _env = env;

    public async Task<string> RenderAsync(string templateName,
                                          IDictionary<string,string> data,
                                          CancellationToken ct = default)
    {
        var path = Path.Combine(_env.ContentRootPath, "EmailTemplates", templateName);
        var html = await File.ReadAllTextAsync(path, ct);

        foreach (var kv in data)
            html = html.Replace($"{{{{{kv.Key}}}}}", kv.Value);

        return html;
    }
}
