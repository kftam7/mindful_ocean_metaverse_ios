using UnityEngine;

public class AquariumInputControl : ScratchInput
{
    private FishGroupEntity controllingObjectEntity;

    private bool isHitObject;

    public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider != null)
            {
                FishGroupEntity entity = hit.collider.gameObject.GetComponent<FishGroupEntity>();
                if (entity != null)
                {
                    controllingObjectEntity = entity;

                    isHitObject = true;
                }
            }
        }

        if (!isHitObject)
        {
            base.OnPointerDown(eventData);
        }
    }

    public override void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (isHitObject)
        {
            //Swim fast
            controllingObjectEntity.SpeedBurst(5f);
            controllingObjectEntity = null;
            isHitObject = false;
        }
        else
        {
            // introduce delay to avoid tutorial bug
            if ((System.DateTime.UtcNow - AquariumUIController.Instance.startTime).Seconds >= 0.5)
            {
                AquariumUIController.Instance.startTime = System.DateTime.UtcNow;
                AquariumUIController.Instance.ShowHiddenButton();
            }

            base.OnPointerUp(eventData);
        }
    }
}