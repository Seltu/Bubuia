using System;
using UnityEngine;

public class PlayerBoatMirroring : MonoBehaviour
{
    [SerializeField] private Transform boatTransform;
    [SerializeField] private FishingRodController rodController;
    private void Update()
    {
        if (rodController.IsHookInWater()||!rodController.GetCanCast())
        {
            transform.localScale =
            rodController.GetCastTarget().x > transform.position.x ?
            new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z) :
            new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale =
            boatTransform.localRotation.eulerAngles.y <= 180 ?
            new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z) :
            new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
}
