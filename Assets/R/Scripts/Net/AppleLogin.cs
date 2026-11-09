using System;
using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Network;
using UnityEngine;

public class AppleLogin : Network.Singleton<AppleLogin>
{
    private IAppleAuthManager _appleAuthManager;

    public void Initialize()
    {
        var deserializer = new PayloadDeserializer();
        _appleAuthManager = new AppleAuthManager(deserializer);
    }

    public void Update()
    {
        if (_appleAuthManager != null)
        {
            _appleAuthManager.Update();
        }
    }

    // Action: userId, idToken, authorizationCode, email, firstName, lastName
    public void LoginToApple(Action<string, string, string, string, string, string> onSuccess, Action onFail)
    {
        if (Application.isEditor)
        {
            onSuccess?.Invoke(NetworkSettings.Instance.TestAppleUserId,
                NetworkSettings.Instance.TestAppleIdToken,
                NetworkSettings.Instance.TestAppleAuthCode,
                "sampleEmail", "sampleFirstName", "sampleLastName");
            return;
        }

        // Initialize the Apple Auth Manager
        if (_appleAuthManager == null)
        {
            Initialize();
        }

        // Set the login arguments
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        // Perform the login
        _appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                if (credential is IAppleIDCredential appleCredential)
                {
                    string user = appleCredential.User;
                    string identityToken = Encoding.UTF8.GetString(appleCredential.IdentityToken, 0, appleCredential.IdentityToken.Length);
                    string authorizationCode = Encoding.UTF8.GetString(appleCredential.AuthorizationCode, 0, appleCredential.AuthorizationCode.Length);
                    string email = appleCredential.Email;
                    IPersonName personName = appleCredential.FullName;
                    string firstName = personName?.GivenName;
                    string lastName = personName?.FamilyName;
                    string state = appleCredential.State;
                    string[] scopes = appleCredential.AuthorizedScopes;
                    RealUserStatus realUserStatus = appleCredential.RealUserStatus;
                    string output = "";
                    output += "[AppleLogin] Sign-in with Apple successfully done.\n";
                    output += $"    UserId: {user}\n";
                    output += $"    IdentityToken: {identityToken}\n";
                    output += $"    AuthorizationCode: {authorizationCode}\n";
                    output += $"    Email: {email}\n";
                    output += $"    FirstName: {firstName}\n";
                    output += $"    LastName: {lastName}\n";
                    output += $"    State: {state}\n";
                    if (scopes != null)
                    {
                        output += "    Scopes:\n";
                        foreach (string scope in scopes)
                        {
                            output +=             scope + "\n";
                        }
                    }
                    else
                    {
                        output += "    Scopes: null\n";
                    }

                    if (personName != null)
                    {
                        output += "    PersonName:\n";
                        output += $"        Nickname: {personName.Nickname ?? ""}\n";
                        output += $"        FamilyName: {personName.FamilyName ?? ""}\n";
                        output += $"        GivenName: {personName.GivenName ?? ""}\n";
                        output += $"        MiddleName: {personName.MiddleName ?? ""}\n";
                        output += $"        NamePrefix: {personName.NamePrefix ?? ""}\n";
                        output += $"        NameSuffix: {personName.NameSuffix ?? ""}\n";
                    }
                    else
                    {
                        output += "    PersonName: null\n";
                    }

                    output += $"    RealUserStatus: {realUserStatus}\n";
                    Debug.Log(output);
                    onSuccess?.Invoke(user, identityToken, authorizationCode, email, firstName, lastName);
                }
                else
                {
                    Debug.Log("[AppleLogin] Sign-in with Apple error. Message: appleIDCredential is null");
                    onFail?.Invoke();
                }
            },
            error =>
            {
                Debug.Log("Sign-in with Apple error. Message: " + error);
                onFail?.Invoke();
            }
        );
    }
}