using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class UIUpgradeMenuScrollView : FancyScrollView<UIUpgradeMenuScrollViewData, UIUpgradeMenuScrollViewContext>
{
    [SerializeField] private ScrollPositionController _scrollPositionController;

    public ScrollPositionController ScrollPositionController
    {
        get { return _scrollPositionController; }
    }

    private System.Action<UIUpgradeMenuScrollViewCell> _onCellPressedHandler = delegate { };
    private System.Action<int> _onDeletePressedHandler = delegate { };

    public void AddCellPressedHandler(System.Action<UIUpgradeMenuScrollViewCell> handler)
    {
        _onCellPressedHandler += handler;
    }

    public void AddDeletePressedhandler(System.Action<int> handler)
    {
        _onDeletePressedHandler += handler;
    }

    public int SelectedIndex
    {
        get { return context.SelectedIndex; }
        set { context.SelectedIndex = value; }
    }

    private static bool isEditModeOn = false;

    public static bool IsEditModeOn
    {
        get { return isEditModeOn; }
    }

    private new void Awake()
    {
        _scrollPositionController.OnUpdatePosition.AddListener(UpdatePosition);
        _scrollPositionController.OnItemSelected.AddListener(CellSelected);
        SetContext(new UIUpgradeMenuScrollViewContext
        {
            OnPressedCell = OnPressedCell,
            OnDeletePress = OnPressedDelete
        });
        base.Awake();
    }

    public void UpdateData(List<UIUpgradeMenuScrollViewData> data)
    {
        cellData = data;
        _scrollPositionController.SetDataCount(cellData.Count);
        UpdateContents();

        SetEditMode(false); //reset to false
    }

    private void OnPressedCell(UIUpgradeMenuScrollViewCell cell)
    {
        _scrollPositionController.ScrollTo(cell.DataIndex, 0.4f);
        context.SelectedIndex = cell.DataIndex;
        UpdateContents();
        _onCellPressedHandler(cell);
    }

    //TODO: Handle null texture problem
    private void OnPressedDelete(UIUpgradeMenuScrollViewCell cell)
    {
        Debug.Log("Deleted: " + cell.DataIndex);
        _onDeletePressedHandler(cell.DataIndex);
        if (context.SelectedIndex == cell.DataIndex)
        {
//			context.SelectedIndex = cell.DataIndex - 1;
            context.SelectedIndex = 0;
            _scrollPositionController.ScrollTo(context.SelectedIndex, 0.4f);
        }

        //reset to default
        GameSave.SetModelSkin(GameSave.SelectedModel, string.Empty);
        EventManager.TriggerEvent(EventList.OnSkinDeleted, cell.DataIndex);

        UpdateContents();
    }

    private void CellSelected(int cellIndex)
    {
        context.SelectedIndex = cellIndex;
        UpdateContents();
    }

    public new void UpdateContents()
    {
        base.UpdateContents();
    }

    public void SetEditMode(bool isActive)
    {
        isEditModeOn = isActive;
        EventManager.TriggerEvent(EventList.OnSkinEditModeStatusChanged, isEditModeOn);
    }

    public void ToggleEditMode()
    {
        isEditModeOn = !isEditModeOn;
        SetEditMode(isEditModeOn);
    }
}