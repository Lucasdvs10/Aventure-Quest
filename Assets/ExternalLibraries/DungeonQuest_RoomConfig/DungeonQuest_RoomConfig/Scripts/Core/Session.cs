/// <summary>
/// Guarda o usuário logado na sessão atual (em memória).
///
/// O /api/users/login agora retorna o "user", então dá para preencher isto logo
/// após o login. Exemplo (no seu fluxo de login, quando souber o id):
///     Session.CurrentUserId = userId;
///
/// A tela de Configuração de Sala usa Session.CurrentUserId como "owner".
/// </summary>
public static class Session
{
    public static string CurrentUserId;
    public static string CurrentUserName;
}
