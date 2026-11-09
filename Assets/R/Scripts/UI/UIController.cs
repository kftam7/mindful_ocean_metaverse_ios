using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public enum State
    {
        CharMenu,
        UpgradeMenu,
        Others
    }

    public static UIController Instance { get; private set; }
    [SerializeField] private UICharacterMenu _charMenu;
    [SerializeField] private UIUpgradeMenu _upgradeMenu;
    [SerializeField] private Localize _playButtonText;
    [SerializeField] private Button _ARButton;
    [SerializeField] private Button _SettingButton;

    private SimpleStateMachine<State> _fsm = new SimpleStateMachine<State>();

    private void Awake()
    {
        Instance = this;
        _fsm.OnStateEnter += OnStateEnter;
        _upgradeMenu.AddBackButtonHandler(UpgradeMenuBackButtonHandler);
        _charMenu.AddUpgradeButtonHandler(MainMenuUpgradeButtonHandler);
        _charMenu.ScrollView.ScrollPositionController.OnItemSelected.AddListener(MainMenuCellSelectedHandler);
    }

    private void Start()
    {
        Debug.Log("Prev scene: " + SceneTransitHandler.PreviousScene);
        var state = State.CharMenu;
        if (SceneTransitHandler.PreviousScene == "PreviewScene" && !string.IsNullOrEmpty(GameSave.GetModelSkin(GameSave.SelectedModel)))
        {
#if EVENT_DEMO
			state = State.Others;
#else
            state = State.UpgradeMenu;
#endif
        }

        SwitchState(state);
    }

    void OnEnable()
    {
        EventManager.StartListening(EventList.OnModelPurchased, OnModelPurchased);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventList.OnModelPurchased, OnModelPurchased);
    }

    protected void OnModelPurchased(object result)
    {
        string model = (string)result;

        Debug.Log("Purchased " + model);
        UpdateSelectedCharacter(currentIndex);
    }

    private void OnStateEnter(State s)
    {
        switch (s)
        {
            case State.CharMenu:
                this._charMenu.gameObject.SetActive(true);
                this._upgradeMenu.gameObject.SetActive(false);
                this._charMenu.UpdateContent();
                this._ARButton.gameObject.SetActive(false);
                this._SettingButton.gameObject.SetActive(true);
                break;
            case State.UpgradeMenu:
                this._charMenu.gameObject.SetActive(false);
                this._upgradeMenu.gameObject.SetActive(true);
                this._upgradeMenu.UpdateContent(DataCenter.Instance.CharacterDataDic[GameSave.SelectedModel]);
                this._ARButton.gameObject.SetActive(true);
                this._SettingButton.gameObject.SetActive(false);
                break;
            case State.Others:
                this._upgradeMenu.gameObject.SetActive(false);
                this._charMenu.gameObject.SetActive(false);
                this._ARButton.gameObject.SetActive(false);
                this._SettingButton.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }


    public void SwitchState(State s)
    {
        Debug.Log("state: " + s);
        _fsm.SwitchState(s);
        if (s == State.CharMenu)
        {
            _charMenu.ScrollView.ScrollPositionController.ScrollTo(GameSave.MenuSelectedIndex, 0);
        }
    }

    public void ShowPlay(bool locked)
    {
        _playButtonText.Set(locked ? "txt_iap_btn_buy" : "txt_play");
    }

    public void ChangePriceText(int price)
    {
        _charMenu.PriceDisplay.ShowPrice(price);
    }

    public void ChangeNameText(string name)
    {
        _charMenu.NameDisplay.ChangeNameText(name);
    }

    private void MainMenuCellSelectedHandler(int index)
    {
        UpdateSelectedCharacter(index);
    }

    protected int currentIndex = 0;

    public void UpdateSelectedCharacter(int index)
    {
        currentIndex = index;

        var selectedCharacterData = DataCenter.Instance.CharacterDataDic.ElementAt(index).Value;
        var locked = !DataCenter.Instance.GetAvailabilityOfModel(selectedCharacterData.ModelName) && selectedCharacterData.Price > 0;

#if EVENT_DEMO
		//no lock fo event
		locked = false;
#endif

#if EVENT_DEMO
		_charMenu.UpgradeButton.gameObject.SetActive(false);
#else
        _charMenu.UpgradeButton.gameObject.SetActive(!_charMenu.IsScrollViewUpdating && !locked);
#endif
        _charMenu.PriceDisplay.gameObject.SetActive(!_charMenu.IsScrollViewUpdating && locked);
        ShowPlay(locked);
        ChangePriceText(selectedCharacterData.Price);
        var name = GameSave.GetEditedName(selectedCharacterData.ModelName);
        Debug.LogFormat("Got name from {0}: {1}", selectedCharacterData.ModelName, name);
        if (string.IsNullOrEmpty(name)) name = Localization.Get(selectedCharacterData.DisplayName);
        ChangeNameText(name);
        GameSave.SelectedModel = selectedCharacterData.ModelName;
        GameSave.MenuSelectedIndex = index;

        _charMenu.ScrollPositionUpdated();
    }

    private void MainMenuUpgradeButtonHandler()
    {
        SwitchState(State.UpgradeMenu);
    }

    private void UpgradeMenuBackButtonHandler()
    {
        SwitchState(State.CharMenu);
    }
}