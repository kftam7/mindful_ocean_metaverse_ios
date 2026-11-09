using UnityEngine;
using System.Collections;

namespace ProGamer.UIAnimation
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UITweenFadeCrossAnimation : UITweenPopupAnimation
    {
        private CanvasGroup targetCanvasGroup;

        public CanvasGroup TargetCanvasGroup
        {
            get
            {
                if (targetCanvasGroup == null) targetCanvasGroup = this.gameObject.GetComponent<CanvasGroup>();
                return targetCanvasGroup;
            }
        }

        public override void ResetPosition()
        {
            base.ResetPosition();
            TargetCanvasGroup.alpha = 0;
        }

        protected override void SetAnimation(AnimationCurve curve, float duration, Vector3 from, Vector3 to, bool isIn)
        {
            base.SetAnimation(curve, duration, from, to, isIn);
            if (isIn)
            {
                targetAlpha = 1;
            }
            else
            {
                targetAlpha = 0;
            }

            fromAlpha = TargetCanvasGroup.alpha;
            targetDif = targetAlpha - fromAlpha;
        }

        private float fromAlpha;
        private float targetAlpha;
        private float targetDif;

        public override void AnimationUpdate(float deltaTime)
        {
            base.AnimationUpdate(deltaTime);
            TargetCanvasGroup.alpha = fromAlpha + targetDif * progressInPercentage;
        }
    }
}