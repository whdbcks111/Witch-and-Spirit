﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

public class Hovl_Laser : MonoBehaviour
{
    public bool IsDamageLaser = false;
    public bool IsCollideedToPlayerLaser = false;

    public GameObject HitEffect;
    public float HitOffset = 0;
    public bool useLaserRotation = false;

    public float MaxLength;
    private LineRenderer Laser;

    public float MainTextureLength = 1f;
    public float NoiseTextureLength = 1f;
    private Vector4 Length = new Vector4(1, 1, 1, 1);
    //private Vector4 LaserSpeed = new Vector4(0, 0, 0, 0); {DISABLED AFTER UPDATE}
    //private Vector4 LaserStartSpeed; {DISABLED AFTER UPDATE}
    //One activation per shoot
    private bool LaserSaver = false;
    private bool UpdateSaver = false;

    private ParticleSystem[] Effects;
    private ParticleSystem[] Hit;

    private int layerMask;

    //--------- 직접 추가한 부분----
    bool AttackCoolTime;

    void Start()
    {
        AttackCoolTime = false;

        layerMask = ~LayerMask.GetMask("Pattern1Bullet");
        //Get LineRender and ParticleSystem components from current prefab;  
        Laser = GetComponent<LineRenderer>();
        Effects = GetComponentsInChildren<ParticleSystem>();
        Hit = HitEffect.GetComponentsInChildren<ParticleSystem>();
        //if (Laser.material.HasProperty("_SpeedMainTexUVNoiseZW")) LaserStartSpeed = Laser.material.GetVector("_SpeedMainTexUVNoiseZW");
        //Save [1] and [3] textures speed
        //{ DISABLED AFTER UPDATE}
        //LaserSpeed = LaserStartSpeed;
    }

    void Update()
    {
        Laser.material.SetTextureScale("_MainTex", new Vector2(Length[0], Length[1]));
        Laser.material.SetTextureScale("_Noise", new Vector2(Length[2], Length[3]));
        //To set LineRender position
        if (Laser != null && UpdateSaver == false)
        {
            // 레이저 시작지점 설정
            Laser.SetPosition(0, transform.position);
  
            // 앞으로 레이저 발사
            if (Physics.Raycast(transform.position, transform.forward, out var hit, MaxLength, LayerMask.GetMask("Player")))
            {
                // 공격 함수
                IEnumerator AttackCool()
                {
                    var colliders = Physics.OverlapSphere(hit.point, 1f);
                    foreach (var col in colliders)
                    {
                        if (col.TryGetComponent(out Player p) && !AttackCoolTime)
                        {
                            p.Damage(1);
                            AttackCoolTime = true;
                            break;
                        }
                    }
                    yield return new WaitForSeconds(2);
                    AttackCoolTime = false;
                }

                // 레이저 끝 지점 설정
                Laser.SetPosition(1, hit.point);

                HitEffect.transform.position = hit.point + hit.normal * HitOffset;

                if (useLaserRotation)
                    HitEffect.transform.rotation = transform.rotation;
                else
                    HitEffect.transform.LookAt(hit.point + hit.normal);

                foreach (var AllPs in Effects)
                {
                    if (!AllPs.isPlaying) AllPs.Play();
                }
                //Texture tiling
                Length[0] = MainTextureLength * (Vector3.Distance(transform.position, hit.point));
                Length[2] = NoiseTextureLength * (Vector3.Distance(transform.position, hit.point));
                //Texture speed balancer {DISABLED AFTER UPDATE}
                //LaserSpeed[0] = (LaserStartSpeed[0] * 4) / (Vector3.Distance(transform.position, hit.point));
                //LaserSpeed[2] = (LaserStartSpeed[2] * 4) / (Vector3.Distance(transform.position, hit.point));

                if (!AttackCoolTime && IsDamageLaser)
                {

                    StartCoroutine(AttackCool());
                    /*
                    var cols = Physics.OverlapSphere(hit.point, 1f);
                    foreach (var col in cols)
                    {
                        if (col.TryGetComponent(out Player p) && !AttackCoolTime)
                        {
                            p.Damage(1);
                            //print("Hp down");
                            AttackCoolTime = true;
                            StartCoroutine(AtkCoolTime());
                            break;
                        }
                    }
                    */
                }

            }
            else
            {
                //End laser position if doesn't collide with object
                var EndPos = transform.position + transform.forward * MaxLength;
                Laser.SetPosition(1, EndPos);
                HitEffect.transform.position = EndPos;
                foreach (var AllPs in Hit)
                {
                    if (AllPs.isPlaying) AllPs.Stop();
                }
                //Texture tiling
                Length[0] = MainTextureLength * (Vector3.Distance(transform.position, EndPos));
                Length[2] = NoiseTextureLength * (Vector3.Distance(transform.position, EndPos));
                //LaserSpeed[0] = (LaserStartSpeed[0] * 4) / (Vector3.Distance(transform.position, EndPos)); {DISABLED AFTER UPDATE}
                //LaserSpeed[2] = (LaserStartSpeed[2] * 4) / (Vector3.Distance(transform.position, EndPos)); {DISABLED AFTER UPDATE}
            }
            //Insurance against the appearance of a laser in the center of coordinates!
            if (Laser.enabled == false && LaserSaver == false)
            {
                LaserSaver = true;
                Laser.enabled = true;
            }
        }
    }

    public void DisablePrepare()
    {
        if (Laser != null)
        {
            Laser.enabled = false;
        }
        UpdateSaver = true;
        //Effects can = null in multiply shooting
        if (Effects != null)
        {
            foreach (var AllPs in Effects)
            {
                if (AllPs.isPlaying) AllPs.Stop();
            }
        }
    }

    IEnumerator AtkCoolTime()
    {
        //AttackCoolTime = true;
        yield return new WaitForSeconds(3);
        AttackCoolTime = false;
    }
}
