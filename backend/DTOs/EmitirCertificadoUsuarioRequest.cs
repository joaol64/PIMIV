namespace Backend.DTOs;

/// <summary>Emite certificado com base nas inscrições reais da conta (usuário logado).</summary>
public class EmitirCertificadoUsuarioRequest
{
    public string UsuarioId { get; set; } = string.Empty;

    /// <summary>Se true, retorna HTML; se false, texto puro (útil para download .txt).</summary>
    public bool ComoHtml { get; set; }
}
