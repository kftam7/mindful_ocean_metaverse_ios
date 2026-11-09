using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

public class QuestionnaireMenu : MonoBehaviour
{
    protected Role selectedIdentity = Role.Non;

    [SerializeField] protected InputField nameField;
    [SerializeField] protected InputField emailField;
    [SerializeField] protected InputField wishesField;
    [SerializeField] protected Toggle[] identityToggles;
    [SerializeField] protected Toggle agreementToggle;

    [SerializeField] protected GameObject identityFieldWarnning;
    [SerializeField] protected GameObject nameFieldWarnning;
    [SerializeField] protected GameObject emailFieldWarnning;
    [SerializeField] protected GameObject wishesFieldWarnning;
    [SerializeField] protected GameObject agreementWarnning;

    void Start()
    {
        HideAllWarnnings();

#if EVENT_DEMO
		identityToggles[(int)Role.Student].isOn = true;
		agreementToggle.isOn = true;
#endif
    }

    protected void HideAllWarnnings()
    {
        identityFieldWarnning.SetActive(false);
        nameFieldWarnning.SetActive(false);
        emailFieldWarnning.SetActive(false);
        wishesFieldWarnning.SetActive(false);
        agreementWarnning.SetActive(false);
    }

    protected void SetActiveAllInputField(bool isActive)
    {
        if (!isActive)
        {
            nameField.text = "";
            emailField.text = "";
            wishesField.text = "";
            HideAllWarnnings();
        }

        nameField.interactable = isActive;
        emailField.interactable = isActive;
        wishesField.interactable = isActive;
    }

    public void SetParent(bool isTrue)
    {
        if (isTrue)
        {
            selectedIdentity = Role.Parent;
            SetActiveAllInputField(true);
        }
    }

    public void SetStudent(bool isTrue)
    {
        if (isTrue)
        {
            selectedIdentity = Role.Student;
            SetActiveAllInputField(false);
        }
    }

    public void SetOther(bool isTrue)
    {
        if (isTrue)
        {
            selectedIdentity = Role.Other;
        }
    }


    public void ShowPrivacyPolicy()
    {
        ProGamer.PopupManager.Instance.QuickShowPopupWebview("https://www.outblaze.com/volvoar-privacy-policy");
    }

    public void Submit()
    {
        bool formatPass = true;

        if (selectedIdentity == Role.Non)
        {
            formatPass = false;
            identityFieldWarnning.SetActive(true);
        }
        else
        {
            identityFieldWarnning.SetActive(false);
        }

        if (selectedIdentity != Role.Student)
        {
            if (string.IsNullOrEmpty(nameField.text))
            {
                formatPass = false;
                nameFieldWarnning.SetActive(true);
            }
            else
            {
                nameFieldWarnning.SetActive(false);
            }

            if (string.IsNullOrEmpty(emailField.text) || !IsEmail(emailField.text))
            {
                formatPass = false;
                emailFieldWarnning.SetActive(true);
            }
            else
            {
                emailFieldWarnning.SetActive(false);
            }

            if (string.IsNullOrEmpty(wishesField.text))
            {
                formatPass = false;
                wishesFieldWarnning.SetActive(true);
            }
            else
            {
                wishesFieldWarnning.SetActive(false);
            }
        }

        if (!agreementToggle.isOn)
        {
            formatPass = false;
            agreementWarnning.SetActive(true);
        }
        else
        {
            agreementWarnning.SetActive(false);
        }

        if (formatPass)
        {
            QuestionnaireData data;

            if (selectedIdentity != Role.Student)
            {
                data = new QuestionnaireData(selectedIdentity, nameField.text, emailField.text, wishesField.text, ProGamer.Localization.language);

#if !UNITY_EDITOR
				LocalStorageManager.Instance.AddData(data);
#endif
            }
            else
            {
                data = new QuestionnaireData(selectedIdentity, "not_available", "not_available", "not_available", "not_available");
            }

            GameSave.QuestionnaireResult = JsonConvert.SerializeObject(data);
            DataCenter.Instance.PlayerQuestionnaireData = data;

            GameSave.IsFinishedQuestionnaire = true;
            SceneTransitHandler.BackMainMenu();
        }
    }

    public const string MatchEmailPattern =
        @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
        + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
        + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
        + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";


    /// <summary>
    /// Checks whether the given Email-Parameter is a valid E-Mail address.
    /// </summary>
    /// <param name="email">Parameter-string that contains an E-Mail address.</param>
    /// <returns>True, wenn Parameter-string is not null and contains a valid E-Mail address;
    /// otherwise false.</returns>
    public static bool IsEmail(string email)
    {
        if (email != null) return Regex.IsMatch(email, MatchEmailPattern);
        else return false;
    }
}