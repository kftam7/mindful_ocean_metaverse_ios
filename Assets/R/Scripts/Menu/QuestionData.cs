using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionData
{
    public int Id { get; set; }
    public string QuestionText { get; set; }
    public string MainImage { get; set; }
    public string[] Answer { get; set; }
    public int RightAnswerIndex { get; set; }
}