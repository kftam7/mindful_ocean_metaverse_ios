using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEngine.UI.Extensions
{
    public class InfiniteScrollView : FancyScrollView<AquariumData, InfiniteScrollViewContext>
    {
        [SerializeField] protected ScrollPositionController scrollPositionController;

        new void Awake()
        {
            scrollPositionController.OnUpdatePosition.AddListener(UpdatePosition);

            // Add OnItemSelected event listener
            scrollPositionController.OnItemSelected.AddListener(CellSelected);

            SetContext(new InfiniteScrollViewContext { OnPressedCell = OnPressedCell });
            base.Awake();
        }

        private void OnEnable()
        {
            UpdateContents();
        }

        public void UpdateData(List<AquariumData> data)
        {
            cellData = data;
            loop = false; //cellData.Count>=4;
            scrollPositionController.movementType
                = loop ? ScrollPositionController.MovementType.Unrestricted : ScrollPositionController.MovementType.Elastic;
            scrollPositionController.SetDataCount(cellData.Count);
            UpdateContents();
        }

        void OnPressedCell(InfiniteScrollViewCell cell)
        {
            scrollPositionController.ScrollTo(cell.DataIndex, 1f);
            context.SelectedIndex = cell.DataIndex;
            UpdateContents();
        }

        // An event triggered when a cell is selected.
        void CellSelected(int cellIndex)
        {
            // Update context.SelectedIndex and call UpdateContents for updating cell's content.
            context.SelectedIndex = cellIndex;
            GameSave.LastSelectedAquariumCellIndex = cellIndex;
            UpdateContents();
        }

        public void ScrollTo(int cellIndex)
        {
            scrollPositionController.ScrollTo(cellIndex, 1f);
            context.SelectedIndex = cellIndex;
            UpdateContents();
        }
    }
}