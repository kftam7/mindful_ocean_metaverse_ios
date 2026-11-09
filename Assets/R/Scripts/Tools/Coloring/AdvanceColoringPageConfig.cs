using UnityEngine;

namespace PaintCraft.Canvas.Configs
{
    public class AdvanceColoringPageConfig : ColoringPageConfig
    {
        protected bool requireRefreshRegionMap = false;

        public override void SetColoringTexture(Texture2D outline, Texture2D region)
        {
            if (outlineTexture != null) Destroy(outlineTexture);
            if (regionTexture != null) Destroy(regionTexture);

            base.SetColoringTexture(outline, region);
            requireRefreshRegionMap = true;
        }

        public void GenerateRegionMapByOutline()
        {
            if (OutlineTexture != null && requireRefreshRegionMap)
            {
                Destroy(regionTexture);

                //TODO: Save for next time
                Texture2D texture = ColoringTools.GetColoredTexture(OutlineTexture, GameSetting.TreatAsAlphaRange);

                regionTexture = texture;
                requireRefreshRegionMap = false;
            }
        }
    }
}