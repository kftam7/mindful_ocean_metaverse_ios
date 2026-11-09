using System.Collections.Generic;
using System.Collections;
using ProGamer;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Extensions
{
    public class PackageScrollView : FancyScrollView<PackageCellData, PackageScrollViewContext>
    {
        [SerializeField]
        ScrollPositionController scrollPositionController;

        new void Awake()
        {
            scrollPositionController.OnUpdatePosition.AddListener(UpdatePosition);

            // Add OnItemSelected event listener
            scrollPositionController.OnItemSelected.AddListener(CellSelected);

            SetContext(new PackageScrollViewContext { OnPressedCell = OnPressedCell });
            base.Awake();
        }

        public void UpdateData(List<PackageCellData> data)
        {
            cellData = data;
            scrollPositionController.SetDataCount(cellData.Count);
            for (int i = 0; i < cells.Count; i++)
            {
                ((PackageScrollViewCell)cells[i]).IsImageBeingSet = false;
            }

            UpdateContents();
        }

        public void OnPressedCell(PackageScrollViewCell cell)
        {
            SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");
            if (context.SelectedIndex == cell.DataIndex)
            {
                if (cell.Data.cellType == PackageCellData.CellType.PackageCell)
                {
                    GameSave.LastSelectedPackageCellIndex = cell.DataIndex;
                    PrintingUIManager.Instance.SelectPackage(cell.Data.Id);
                }
                else if (cell.Data.cellType == PackageCellData.CellType.ColoringPageCell && cell.Data.purchased)
                {
                    GameSave.LastSelectedColoringPageCellIndex = cell.DataIndex;
                    PrintingUIManager.Instance.SetSelectedImage(cell.Data.materialData, true);
                }
                else if (cell.Data.cellType == PackageCellData.CellType.PackageFrontFaceCell)
                {
                    PrintingUIManager.Instance.QuitPackage(cell.Data.Id);
                }
            }
            else
            {
                scrollPositionController.ScrollTo(cell.DataIndex, 1f);
                if (cell.Data.cellType == PackageCellData.CellType.ColoringPageCell && cell.Data.purchased)
                {
                    GameSave.LastSelectedColoringPageCellIndex = cell.DataIndex;
                    PrintingUIManager.Instance.SetSelectedImage(cell.Data.materialData, context.SelectedIndex == cell.DataIndex);
                    PrintingUIManager.Instance.SetFunctionButtonActive(true);
                }
                else if (cell.Data.cellType == PackageCellData.CellType.PackageFrontFaceCell)
                {
                    PrintingUIManager.Instance.SetFunctionButtonActive(false);
                    PrintingUIManager.Instance.DeselectedImage();
                }

                context.SelectedIndex = cell.DataIndex;
                UpdateContents();
            }
        }

        // An event triggered when a cell is selected.
        void CellSelected(int cellIndex)
        {
            // Update context.SelectedIndex and call UpdateContents for updating cell's content.
            context.SelectedIndex = cellIndex;
            UpdateContents();
        }

        public void ScrollTo(int cellIndex, float scrollSpeed = 1f)
        {
            scrollPositionController.ScrollTo(cellIndex, scrollSpeed);
            context.SelectedIndex = cellIndex;
            UpdateContents();
        }

        public void NowScrollTo(int cellIndex)
        {
            this.ScrollTo(cellIndex, 0);
        }
    }
}