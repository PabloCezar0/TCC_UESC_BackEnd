namespace Rota.Application.Settings;

public class EmailSettings
{
    public string SmtpHost    { get; set; } = default!;
    public int    SmtpPort    { get; set; }
    public bool   UseSsl      { get; set; }
    public string UserName    { get; set; } = default!;
    public string Password    { get; set; } = default!;
    public string FromAddress { get; set; } = default!;
    public string FromName    { get; set; } = default!;
}
    