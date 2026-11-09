using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Text;

namespace ProGamer
{
    public delegate void UpdateHandler();

    public class Localization : MonoBehaviour
    {
        static Localization mInstance;

        static string locale = "English";

        public static string Locale
        {
            get { return locale; }
        }

        static public string[] knownLanguages;

        static public Dictionary<string, string[]> dictionary
        {
            get
            {
                if (!localizationHasBeenSet)
                    language = PlayerPrefs.GetString("Language", "English");
                return mDictionary;
            }
        }

        static public bool localizationHasBeenSet = false;

        static public bool isActive
        {
            get { return mInstance != null; }
        }

        static public Localization instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = GameObject.FindObjectOfType(typeof(Localization)) as Localization;

                    if (mInstance == null)
                    {
                        //try to get the prefab first if not we create a new object
                        GameObject go;
                        go = Instantiate(Resources.Load("Prefabs/Localization"), Vector3.zero, Quaternion.identity) as GameObject;
                        if (go == null)
                        {
                            go = new GameObject("_Localization");
                            mInstance = go.AddComponent<Localization>();
                        }
                        else
                        {
                            mInstance = go.GetComponent<Localization>();
                        }

                        DontDestroyOnLoad(go);
                    }
                }

                return mInstance;
            }
        }

        [HideInInspector] public string startingLanguage = "English";

        public TextAsset[] languages;

        static Dictionary<string, string> mOldDictionary = new Dictionary<string, string>();
        static Dictionary<string, string[]> mDictionary = new Dictionary<string, string[]>();
        static int mLanguageIndex = -1;
        static string mLanguage;

        public event UpdateHandler LocalizeUpdate = delegate() { };

        static int currentFont
        {
            get { return PlayerPrefs.GetInt("LOCALIZATION_FONT", 0); }
            set { PlayerPrefs.SetInt("LOCALIZATION_FONT", value); }
        }

        void Awake()
        {
            if (mInstance == null)
            {
                mInstance = this;
                DontDestroyOnLoad(gameObject);

                // Legacy functionality
                if (mOldDictionary.Count == 0 && mDictionary.Count == 0)
                    language = PlayerPrefs.GetString("Language", startingLanguage);

                // Legacy functionality
                if (string.IsNullOrEmpty(mLanguage) && (languages != null && languages.Length > 0))
                    language = languages[0].name;


                int langIsSet = PlayerPrefs.GetInt("languageIsSet", -1);
                if (langIsSet == -1)
                {
                    GetDeviceLocaleAndSwitchLanguage();
                    PlayerPrefs.SetInt("languageIsSet", 1);
                }
            }
            else
                Destroy(gameObject);
        }

#if UNITY_IOS && !UNITY_EDITOR
		[DllImport ("__Internal")] private static extern IntPtr _GetLocale();
#endif

        void GetDeviceLocaleAndSwitchLanguage()
        {
#if UNITY_EDITOR
            return;
#endif

#pragma warning disable 0162
#if UNITY_ANDROID
			try {

				var localeClass = new AndroidJavaClass ("java.util.Locale");
				var localeInst = localeClass.CallStatic<AndroidJavaObject> ("getDefault");
				locale = localeInst.Call<String> ("toString");

			} catch (System.Exception e) {

				Debug.LogWarning ("failed to get locale for android!" + e.Message);
			}

#endif
#if UNITY_IOS && !UNITY_EDITOR
			locale = Marshal.PtrToStringAnsi(_GetLocale());
#endif

            Debug.Log("locale: " + locale.Substring(0, 2).ToLower());

            switch (locale.Substring(0, 2).ToLower())
            {
                case "en":
                    language = "English";
                    break;
                case "fr":
                    language = "French";
                    break;
                case "de":
                    language = "German";
                    break;
                case "it":
                    language = "Italian";
                    break;
                case "pt":
                    if (locale.ToLower().Contains("br"))
                    {
                        language = "BrazilianPortuguese";
                    }
                    else
                    {
                        language = "Portuguese";
                    }

                    break;
                case "ja":
                    language = "Japanese";
                    break;
                case "es":
                    if (locale.ToLower().Contains("419") || locale.ToLower().Contains("mx"))
                    {
                        language = "LatinSpanish";
                    }
                    else
                    {
                        language = "Spanish";
                    }

                    break;
                case "ko":
                    language = "Korean";
                    break;
                case "zh":
                    if (locale.ToLower().Contains("hans") || locale.Contains("zh_CN"))
                    {
                        language = "SimplifiedChinese";
                    }
                    else
                    {
                        language = "TraditionalChinese";
                    }

                    break;
                case "tr":
                    language = "Turkish";
                    break;
                case "ru":
                    language = "Russian";
                    break;
                case "vi":
                    language = "Vietnamese";
                    break;
                default:
                    language = "English";
                    break;
            }

            PlayerPrefs.SetString("Language", language);
            GameSave.GameLanguage = language;
#pragma warning restore 0162
        }

        void OnEnable()
        {
            if (mInstance == null)
                mInstance = this;
        }


        void OnDisable()
        {
            localizationHasBeenSet = false;
            mLanguageIndex = -1;
            mDictionary.Clear();
            mOldDictionary.Clear();
        }

        void OnDestroy()
        {
            if (mInstance == this)
                mInstance = null;
        }

        [System.Obsolete("Use Localization.language instead")]
        public string currentLanguage
        {
            get { return language; }
            set { language = value; }
        }

        static public string language
        {
            get { return mLanguage; }
            set
            {
                if (mLanguage != value)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Try to load the language from the CSV list
                        if (mDictionary.Count != 0 && SelectLanguage(value))
                            return;

                        // Legacy functionality where languages were specified on the Localization class
                        if (mInstance != null && mInstance.languages != null)
                        {
                            for (int i = 0, imax = mInstance.languages.Length; i < imax; ++i)
                            {
                                TextAsset asset = mInstance.languages[i];

                                if (asset != null && asset.name == value)
                                {
                                    currentFont = i;
                                    Load(asset);
                                    return;
                                }
                            }
                        }
                    }

                    // Either the language is null, or it wasn't found
                    mOldDictionary.Clear();
                    PlayerPrefs.DeleteKey("Language");
                }
            }
        }

        /// <summary>
        /// Load the specified asset and activate the localization.
        /// </summary>
        static public void Load(TextAsset asset)
        {
            ByteReader reader = new ByteReader(asset);
            Set(asset.name, reader.ReadDictionary());
        }

        /// <summary>
        /// Load the specified CSV file.
        /// </summary>
        static public bool LoadCSV(TextAsset asset)
        {
#if SHOW_REPORT
			mUsed.Clear();
#endif
            ByteReader reader = new ByteReader(asset);

            // The first line should contain "KEY", followed by languages.
            BetterList<string> temp = reader.ReadCSV();

            // There must be at least two columns in a valid CSV file
            if (temp.size < 2)
                return false;

            // The first entry must be 'KEY', capitalized
            temp[0] = "KEY";

#if !UNITY_3_5
            // Ensure that the first value is what we expect
            if (!string.Equals(temp[0], "KEY"))
            {
                Debug.LogError("Invalid localization CSV file. The first value is expected to be 'KEY', followed by language columns.\n" +
                               "Instead found '" + temp[0] + "'", asset);
                return false;
            }
            else
#endif
            {
                knownLanguages = new string[temp.size - 1];
                for (int i = 0; i < knownLanguages.Length; ++i)
                    knownLanguages[i] = temp[i + 1];
            }

            mDictionary.Clear();

            // Read the entire CSV file into memory
            while (temp != null)
            {
                AddCSV(temp);
                temp = reader.ReadCSV();
            }

            return true;
        }

        /// <summary>
        /// Select the specified language from the previously loaded CSV file.
        /// </summary>
        static bool SelectLanguage(string language)
        {
            mLanguageIndex = -1;

            if (mDictionary.Count == 0)
                return false;

            string[] keys;

            if (mDictionary.TryGetValue("KEY", out keys))
            {
                for (int i = 0; i < keys.Length; ++i)
                {
                    if (keys[i] == language)
                    {
                        mOldDictionary.Clear();
                        mLanguageIndex = i;
                        mLanguage = language;
                        PlayerPrefs.SetString("Language", mLanguage);
                        instance.LocalizeUpdate();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Helper function that adds a single line from a CSV file to the localization list.
        /// </summary>
        static void AddCSV(BetterList<string> values)
        {
            if (values.size < 2)
                return;
            string[] temp = new string[values.size - 1];
            for (int i = 1; i < values.size; ++i)
                temp[i - 1] = values[i];
            mDictionary.Add(values[0], temp);
        }

        /// <summary>
        /// Load the specified asset and activate the localization.
        /// </summary>
        static public void Set(string languageName, Dictionary<string, string> dictionary)
        {
#if SHOW_REPORT
			mUsed.Clear();
#endif
            mLanguage = languageName;
            PlayerPrefs.SetString("Language", mLanguage);
            mOldDictionary = dictionary;
            localizationHasBeenSet = false;
            mLanguageIndex = -1;
            knownLanguages = new string[] { languageName };
            instance.LocalizeUpdate();
        }

        /// <summary>
        /// Localize the specified value.
        /// </summary>
        public static string ConvertTextToUTF8(string text)
        {
            return Encoding.Unicode.GetString(Encoding.Convert(Encoding.UTF8, Encoding.Unicode, Encoding.UTF8.GetBytes(text)));
        }

        static public string Get(string key)
        {
            if (string.IsNullOrEmpty(key) || key.Equals("-"))
                return "";
            // Ensure we have a language to work with
            if (!localizationHasBeenSet)
                language = PlayerPrefs.GetString("Language", "English");

            string val;
            string[] vals;
#if UNITY_IOS || UNITY_ANDROID
            string mobKey = key + " Mobile";

            if (mLanguageIndex != -1 && mDictionary.TryGetValue(mobKey, out vals))
            {
                if (mLanguageIndex < vals.Length)
                    return ConvertTextToUTF8(vals[mLanguageIndex]);
            }
            else if (mOldDictionary.TryGetValue(mobKey, out val))
                return ConvertTextToUTF8(val);
#endif
            if (mLanguageIndex != -1 && mDictionary.TryGetValue(key, out vals))
            {
                if (mLanguageIndex < vals.Length)
                    return ConvertTextToUTF8(vals[mLanguageIndex]);
            }
            else if (mOldDictionary.TryGetValue(key, out val))
                return ConvertTextToUTF8(val);

#if UNITY_EDITOR
            if (Localization.instance)
            {
                Debug.LogWarning("Localization key not found: '" + key + "'");
            }
            else
            {
                Debug.LogWarning("Cannot find localization instance");
            }
#endif
            return ConvertTextToUTF8(key);
        }

#if MULTIPLE_FONT
		public Font[] normalFontList;
		public TMP_FontAsset[] tmpFontList;
		public Vector2[] pivotOffsetList;
		public int[] FontSizeOffsetList;
#endif

        public Vector2 currentFontPivotOffset
        {
            get
            {
#if MULTIPLE_FONT
				return pivotOffsetList[currentFont];
#else
                return Vector2.zero;
#endif
            }
        }

        public int CurrentFontSizeOffset
        {
            get
            {
#if MULTIPLE_FONT
				return FontSizeOffsetList[currentFont];
#else
                return 0;
#endif
            }
        }

        #region Text Setter

        static public void SetText(string key, Text text)
        {
            text.text = Get(key);
            SetFont(text);
        }

        static public void SetTextWithParameters(string key, ref Text text, object[] array)
        {
            string tem = Get(key);
            text.text = string.Format(tem, array);
        }

        static public void SetTextWithFormat(string key, ref Text text, string format)
        {
            string tem = Get(key);
            text.text = string.Format(format, tem);
        }

        static public void SetFont(Text text)
        {
            text.font = FontDicCenter.Instance.LocalizationFontDic.GetFontByLanguage(Localization.language);
            // #if MULTIPLE_FONT
            // if(currentFont >= 0 && currentFont < mInstance.normalFontList.Length){
            // 	Font targetFont = mInstance.normalFontList[currentFont];
            // 	if(targetFont != text.font){
            // 		text.font = targetFont;
            // 	}
            // }
            // #endif
        }

        #endregion

        public string Get(string key, int languageIndex)
        {
            TextAsset asset = languages[languageIndex];
            ByteReader reader = new ByteReader(asset);
            Dictionary<string, string> langDict = reader.ReadDictionary();
            string val;
            return ConvertTextToUTF8((langDict.TryGetValue(key, out val)) ? val : key);
        }

        /// <summary>
        /// Localize the specified value.
        /// </summary>
        [System.Obsolete("Use Localization.Get instead")]
        static public string Localize(string key)
        {
            return Get(key);
        }

        /// <summary>
        /// Returns whether the specified key is present in the localization dictionary.
        /// </summary>
        static public bool Exists(string key)
        {
            if (mLanguageIndex != -1)
                return mDictionary.ContainsKey(key);
            return mOldDictionary.ContainsKey(key);
        }

#if UNITY_EDITOR
        private bool isDebugMenu = false;
        private int selLangInt = 0;
        private string[] langList;

        public string[] LangList
        {
            get
            {
                if (langList == null)
                {
                    langList = new string[languages.Length];
                    for (int i = 0; i < languages.Length; i++) langList[i] = languages[i].name;
                }

                return langList;
            }
        }

        void OnGUI()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.D))
            {
                isDebugMenu = true;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.F))
            {
                isDebugMenu = false;
            }

            if (isDebugMenu)
            {
                GUILayout.BeginVertical("Box");
                selLangInt = GUILayout.SelectionGrid(selLangInt, LangList, 2);

                if (GUILayout.Button("Apply"))
                {
                    if (language != LangList[selLangInt])
                    {
                        language = LangList[selLangInt];
                    }
                }

                GUILayout.EndVertical();
            }
        }
#endif
    }
}