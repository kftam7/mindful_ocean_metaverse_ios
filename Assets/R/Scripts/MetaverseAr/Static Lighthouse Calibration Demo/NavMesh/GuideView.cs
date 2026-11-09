using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Auki.Navigation
{
    public class GuideView : MonoBehaviour
    {
        public SpriteRenderer InnerSprite;
        public SpriteRenderer OuterSprite;
        public SortingGroup TextSort;
        public TMP_Text TextField;
        
        public Transform Outer;
        public Transform Inner;
    }
}