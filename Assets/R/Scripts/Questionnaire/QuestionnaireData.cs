using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Role
{
    Parent = 0,
    Student = 1,
    Other = 2,
    Non = -1
};

public class QuestionnaireData
{
    public string Id { get; set; }
    public Role PlayerRole { get; set; }
    public string SelectedLanguage { get; set; }
    public string PlayerName { get; set; }
    public string Email { get; set; }
    public string Wishes { get; set; }
    public System.DateTime CreatedUtcTime { get; set; }

    public QuestionnaireData() { }

    public QuestionnaireData(Role role, string name, string email, string wishes, string lang)
    {
        // this.Id = ImageDataWriter.GetGUID();
        this.PlayerRole = role;
        this.PlayerName = name;
        this.Email = email;
        this.Wishes = wishes;
        this.SelectedLanguage = lang;
        this.CreatedUtcTime = System.DateTime.UtcNow;
    }
}