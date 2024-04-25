using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Michsky.MUIP;
using Firebase;
using Firebase.Auth;
using System.Net.Mail;
using System;
using System.Threading.Tasks;
using Firebase.Extensions;
using System.Text.RegularExpressions;
using System.Net;

public class FireBase : MonoBehaviour
{
    [Header("Пользовательская информация")]
    public TMP_Text emailText; // Поле для отображения электронной почты

    [Header("Кнопки")]
    public GameObject ButtonsP;

    [Header("Уведомления Forgot")]
    public NotificationManager NotirificationForgotError;
    public NotificationManager NotirificationForgotComplete;

    [Header("Уведомления регистрация")]
    public NotificationManager NotirificationSingUpName;
    public NotificationManager NotirificationSingUpPassword;
    public NotificationManager NotirificationSingUpConfirm;
    public NotificationManager NotirificationSingUpMatch;
    public NotificationManager NotirificationSingUpEMail;
    public NotificationManager NotitificationSingUpMatch;
    public NotificationManager NotirificationPasswordLength;
    public NotificationManager NotirificationPasswordLength1;

    [Header("Уведомления вход")]
    public NotificationManager NotirificationSingInEMail;
    public NotificationManager NotirificationSingInPassword;

    [Header("Экраны")]
    public GameObject loginPanel;
    public GameObject singupPanel;
    public GameObject forgotPasswordPanel;
    public GameObject adminPanel;
    public GameObject homePanel;

    [Header("InputField вход")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;

    [Header("InputField регистрация")]
    public TMP_InputField singupEmail;
    public TMP_InputField singupPassword;
    public TMP_InputField singupCPassword;
    public TMP_InputField singupUserName;

    [Header("InputField forgot")]
    public TMP_InputField forgotPass;
    public static event Action UserAuthenticated;

    [Header("PanelController")]
    public PanelController panelController; // Добавьте ссылку на ваш PanelController
    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;

    bool isSingIn = false;

    private void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {

            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
                
                user = auth.CurrentUser;
                if (user != null)
                {
                    // Используйте PanelController для управления панелями
                    panelController.ActivatePanel(user.Email == "admin@admin.com" ? panelController.adminPanel: panelController.profilePanel);
                }
                else
                {
                    panelController.ActivatePanel(panelController.loginPanel);
                    UserAuthenticated?.Invoke();
                }
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }

    // Функция проверки валидности email
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Нормализация домена
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Проверка соответствия шаблону
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static string DomainMapper(Match match)
    {
        // Используется IdnMapping класс для преобразования доменных имен Unicode.
        var idn = new System.Globalization.IdnMapping();

        string domainName = match.Groups[2].Value;
        try
        {
            domainName = idn.GetAscii(domainName);
        }
        catch (ArgumentException)
        {
            throw;
        }
        return match.Groups[1].Value + domainName;
    }
    
    

    public void LoginUser()
    {
        if (!IsValidEmail(loginEmail.text))
        {
            NotirificationSingInEMail.Open();
        }
        else if (string.IsNullOrEmpty(loginPassword.text) || string.IsNullOrEmpty(loginEmail.text))
        {
            NotirificationSingInPassword.Open();
        }
        else
        {
            SingInUser(loginEmail.text, loginPassword.text);
        }
    }

    public void SingUpUser()
    {
        if (string.IsNullOrEmpty(singupUserName.text))
        {
            NotirificationSingUpName.Open();
        }

        if (string.IsNullOrEmpty(singupEmail.text))
        {
            NotirificationSingUpEMail.Open();
        }
        else if (!IsValidEmail(singupEmail.text))
        {
            NotirificationSingUpEMail.Open();
        }

        if (string.IsNullOrEmpty(singupPassword.text))
        {
            NotirificationSingUpPassword.Open();
        }

        if (string.IsNullOrEmpty(singupCPassword.text))
        {
            NotirificationSingUpConfirm.Open();
        }
        else if (singupPassword.text != singupCPassword.text)
        {
            NotitificationSingUpMatch.Open();
        }
        else if (singupPassword.text.Length < 6)
        {
            NotirificationPasswordLength.Open();
            NotirificationPasswordLength1.Open();
        }

        else
        {
            CreateUser(singupEmail.text, singupPassword.text, singupUserName.text);
        }
    }

    public void LogOut()
    {
        auth.SignOut();
        panelController.ActivatePanel(panelController.loginPanel);
    }

    public void ForgotPassword()
    {
        if (!IsValidEmail(forgotPass.text))
        {
            NotirificationForgotError.Open();
            return;

        }

        if (string.IsNullOrEmpty(forgotPass.text))
        {
            NotirificationForgotError.Open();
            return;
        }
        else
        {
            NotirificationForgotComplete.Open();
            forgotPasswordSubmit(forgotPass.text);
        }
        
    }
    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            isSingIn = auth.CurrentUser != null && auth.CurrentUser.IsValid();
            user = auth.CurrentUser;
            if (isSingIn)
            {
                Debug.Log("Signed in " + user.UserId);
                emailText.text = user.Email; // Обновление текстового поля с электронной почтой пользователя
                if (user.Email == "admin@admin.com")
                {
                    panelController.ActivatePanel(panelController.adminPanel);
                }
                else
                {
                    panelController.ActivatePanel(panelController.profilePanel);
                }
            }
            else
            {
                Debug.Log("Signed out");
                panelController.ActivatePanel(panelController.loginPanel);
            }
        }
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }
    void CreateUser(string email, string password, string Username)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
            UpdateUserProfile(Username);
            panelController.ActivatePanel(panelController.profilePanel);
        });
    }

    public void SingInUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Login failed: " + task.Exception);
            }
            else
            {
                Firebase.Auth.AuthResult result = task.Result;
                Debug.Log("User signed in successfully: " + result.User.Email);
                panelController.ActivatePanel(result.User.Email == "admin@admin.com" ? panelController.adminPanel : panelController.profilePanel);
            }
        });
    }

    void UpdateUserProfile(string UserName)
    {
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            {
                DisplayName = UserName,
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");
            });
        }
    }

    void forgotPasswordSubmit(string ForgotPasswordEmail)
    {
        auth.SendPasswordResetEmailAsync(ForgotPasswordEmail).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SendReset was canceled, лол");
            }

            if(task.IsFaulted)
            {
                foreach (Exception exception in task.Exception.InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        //ShowNotMes("Error", (GetErrorMessage(errorCode)));

                    }
                }
            }
        });
    }
}