using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class UICharacterMenuScrollView : FancyScrollView<UICharacterMenuScrollViewData, UICharacterMenuScrollViewContext>
{
    [SerializeField] private ScrollPositionController _scrollPositionController;

    public ScrollPositionController ScrollPositionController
    {
        get { return _scrollPositionController; }
    }

    public int SelectedIndex
    {
        get { return context.SelectedIndex; }
    }

    private new void Awake()
    {
        _scrollPositionController.OnUpdatePosition.AddListener(UpdatePosition);

        _scrollPositionController.OnItemSelected.AddListener(CellSelected);

        SetContext(new UICharacterMenuScrollViewContext { OnPressedCell = OnPressedCell });

        base.Awake();
    }

    public void UpdateData(List<UICharacterMenuScrollViewData> data)
    {
        cellData = data;
        _scrollPositionController.SetDataCount(cellData.Count);
        UpdateContents();
    }

    public new void UpdateContents()
    {
        base.UpdateContents();
    }

    public void MoveToIndex(int index)
    {
        context.SelectedIndex = index;
        _scrollPositionController.ScrollTo(context.SelectedIndex, 0.4f);
        UpdateContents();
    }

    private void OnPressedCell(UICharacterMenuScrollViewCell cell)
    {
        _scrollPositionController.ScrollTo(cell.DataIndex, 0.4f);
        context.SelectedIndex = cell.DataIndex;
        UpdateContents();
    }

    private void CellSelected(int cellIndex)
    {
        Debug.Log("Focus on cell: " + cellIndex.ToString());
        context.SelectedIndex = cellIndex;
        UpdateContents();
    }
}