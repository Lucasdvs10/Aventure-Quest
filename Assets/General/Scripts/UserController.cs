using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UserController
{
    private const string BaseUrl = "https://dungeon-quest-api.fly.dev/api";
    private string LoginEndpoint = $"{BaseUrl}/users/login";
    private string SignupUserEndpoint = $"{BaseUrl}/users";
    private static readonly HttpClient HttpClient = new();


    public async Task<bool> LoginAsync(string userName, string password)
    {
        try
        {
            LoginRequest request = new()
            {
                user_name = userName,
                password = password
            };

            string json =
                JsonUtility.ToJson(request);

            StringContent content =
                new(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

            HttpResponseMessage response =
                await HttpClient.PostAsync(
                    LoginEndpoint,
                    content
                );

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogWarning(
                    $"Erro HTTP: {response.StatusCode}"
                );

                return false;
            }

            string responseJson =
                await response.Content.ReadAsStringAsync();

            LoginResponse loginResponse =
                JsonUtility.FromJson<LoginResponse>(
                    responseJson
                );

            bool loginValid =
                loginResponse?.response?.valid ?? false;

            if (
                loginValid &&
                !string.IsNullOrEmpty(
                    loginResponse.response.user.id
                )
            )
            {
                PlayerPrefs.SetString(
                    "CurrentUserID",
                    loginResponse.response.user.id
                );

                PlayerPrefs.Save();

                Debug.Log(
                    $"Usuário salvo: {loginResponse.response.user.id}"
                );
            }

            return loginValid;
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"Erro ao realizar login: {e}"
            );

            return false;
        }
    }

    [Serializable]
    private class LoginRequest
    {
        public string user_name;
        public string password;
    }

    [Serializable]
    private class LoginResponse
    {
        public LoginResult response;
    }

    [Serializable]
    private class LoginResult
    {
        public bool valid;

        public UserData user;
    }


    [Serializable]
    private class UserData
    {
        public string id;

        public string user_name;

        public int high_score;

        public bool active;
    }


    public async Task<bool> RegisterAsync(
    string userName,
    string password)
    {
        try
        {
            RegisterRequest request = new()
            {
                user_name = userName,
                password = password,
                high_score = 0,
                active = true
            };

            string json =
                JsonUtility.ToJson(request);

            StringContent content =
                new(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

            HttpResponseMessage response =
                await HttpClient.PostAsync(
                    SignupUserEndpoint,
                    content
                );

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogWarning(
                    $"Erro ao criar usuário: {response.StatusCode}"
                );

                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"Erro ao criar usuário: {e}"
            );

            return false;
        }
    }

    [Serializable]
    private class RegisterRequest
    {
        public string user_name;
        public string password;

        public int high_score;

        public bool active;
    }
}