using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserControlObject : UserControlReceiver
{
    public static OnUserInputEvent OnDirectionChanged;

    //Set it private after decided
    public float SpeedConstant = 0.1f;
    public float GravityConstant = -5f;
    protected bool isAlive = false;

    protected float userDisableTime = 0;

    //init start direction
    protected Vector3 direction = new Vector3(1, 0);

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.StartListening(EventList.OnGameStatusChanged, OnGameStatusChanged);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.StopListening(EventList.OnGameStatusChanged, OnGameStatusChanged);
    }

    protected void OnGameStatusChanged(object result)
    {
        isAlive = (bool)result;
    }

    void Update()
    {
        if (userDisableTime > 0)
        {
            userDisableTime -= Time.deltaTime;
        }

        if (this.transform.position.y > WaterLevelBoundary.Instance.WaterSurfaceBoundaryLevel)
        {
            //Jump out water surface
            ProjectileMotion();
        }
        else
        {
            if (isAlive)
            {
                enteredProjectileMotion = false;
                //Swin underwater
                NormalMotion();
            }
        }
    }

    protected override void OnUserScratching(Vector2 force)
    {
        //Entered projectile motion will ignore input
        if (!enteredProjectileMotion && isAlive && userDisableTime <= 0)
        {
            direction = force / force.magnitude;

            if (OnDirectionChanged != null)
            {
                OnDirectionChanged(direction);
            }
        }
    }

    bool enteredProjectileMotion = false;

    float projectileTimeCount;
    Vector3 projectileInitSpeed;
    Vector3 projectileInitPosition;

    protected virtual void ProjectileMotion()
    {
        if (!enteredProjectileMotion)
        {
            //init
            enteredProjectileMotion = true;

            projectileTimeCount = 0;
            projectileInitSpeed = direction * SpeedConstant;
            projectileInitPosition = this.transform.position;

            EffectController.Instance.PlayEffect("Prefabs/Effects/WaterSplash", this.transform.position);
            ProGamer.SoundManager.Instance.Play("jump_into_water");
        }

        projectileTimeCount += Time.deltaTime;

        float dX = projectileInitSpeed.x * projectileTimeCount;
        float dY = projectileInitSpeed.y * projectileTimeCount + 0.5f * GravityConstant * Mathf.Pow(projectileTimeCount, 2);
        this.transform.position = projectileInitPosition + new Vector3(dX, dY);

        //update direction
        Vector3 speed = new Vector3(projectileInitSpeed.x, projectileInitSpeed.y + GravityConstant * projectileTimeCount);
        direction = speed / speed.magnitude;

        if (OnDirectionChanged != null)
        {
            OnDirectionChanged(direction);
        }
    }

    protected virtual void NormalMotion()
    {
        this.transform.position += direction * Time.deltaTime * SpeedConstant;
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Border":

                ProGamer.SoundManager.Instance.Play(Random.Range(0, 100) % 2 == 0 ? "hit_1" : "hit_2");

                InGameData.Instance.GotDamage(GameSetting.WallDamage);

                ShowHPEffect(new Color32(229, 49, 39, 255), "-" + GameSetting.WallDamage);

                foreach (var contact in collision.contacts)
                {
                    EffectController.Instance.PlayEffect(GameSetting.WallHitEffect, contact.point);

                    if (enteredProjectileMotion)
                    {
                        enteredProjectileMotion = false;
                    }

                    Vector3 normal = Vector3.Normalize(new Vector3(contact.normal.x, contact.normal.y));
                    direction = direction - 2 * Vector3.Dot(direction, normal) * normal;

                    userDisableTime = 0.5f;

                    if (OnDirectionChanged != null)
                    {
                        OnDirectionChanged(direction);
                    }
                }

                break;

            case "InGameObject":

                InGameObject o = collision.collider.gameObject.GetComponent<InGameObject>();
                o.IsActive = false;

                foreach (var contact in collision.contacts)
                {
                    EffectController.Instance.PlayEffect(o.Data.EffectPrefab, contact.point);
                }

                ExecuteGameObjectEffect(o.Data);
                Destroy(o.gameObject);
                break;
        }
    }

    public void ExecuteGameObjectEffect(InGameObjectData data)
    {
        switch (data.Effect)
        {
            case "Damage":

                ProGamer.SoundManager.Instance.Play(Random.Range(0, 100) % 2 == 0 ? "hit_1" : "hit_2");

                InGameData.Instance.GotDamage(data.EffectParameter);

                ShowHPEffect(new Color32(229, 49, 39, 255), "-" + data.EffectParameter);
                break;
            case "HP":
                ProGamer.SoundManager.Instance.Play("eat");

                InGameData.Instance.GainScore(GameBalanceCalculation.GetScoreByFood(data.ModelName, InGameData.Instance.WholeTime));
                InGameData.Instance.GotFood(data.EffectParameter);

                ShowHPEffect(new Color32(255, 231, 97, 255), "+" + data.EffectParameter);
                break;
            default:
                Debug.Log("No implementation of effect " + data.Effect);
                break;
        }
    }

    public void ShowHPEffect(Color32 color, string content)
    {
        EffectController.Instance.PlayHPEffect("Prefabs/Effects/HPEffect", color, content);
    }

#if UNITY_EDITOR
//	void OnGUI(){
//
//		GUI.Label(new Rect(0,0,200,50), "Control: " + (!enteredProjectileMotion).ToString()); 
//		GUI.Label(new Rect(0,50,200,50), "<color=red>Direction: " + direction.ToString() + "</color>"); 
//	}
#endif
}