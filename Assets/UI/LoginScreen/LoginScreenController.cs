using TMPro;
using UnityEngine;

public class LoginScreenController : MonoBehaviour
{
    UserController userController;
    
    [Header("Login")]
    [SerializeField] private TMP_InputField loginInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    [Header("Signup")]
    [SerializeField] private TMP_InputField signupUsernameInputField;
    [SerializeField] private TMP_InputField signupPasswordInputField;

    void Awake()
    {
        userController = new UserController();
    }

    public async void Login()
    {
        var username = loginInputField.text;
        var password = passwordInputField.text;

        var response = await userController.LoginAsync(username, password);
    }

    public async void Signup()
    {
        var username = signupUsernameInputField.text;
        var password = signupPasswordInputField.text;

        var response = await userController.RegisterAsync(username, password);
    }
}
