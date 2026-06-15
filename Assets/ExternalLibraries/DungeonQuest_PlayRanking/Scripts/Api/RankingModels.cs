using System;

// Usuário no ranking (GET /api/users).
// ApiResponse<T> e TagModel são reaproveitados dos outros módulos.
// Se usar este módulo sozinho, defina ApiResponse<T>/TagModel (ver README).
[Serializable]
public class UserRankModel
{
    public string id;
    public string user_name;
    public int high_score;
    public bool active;
}
