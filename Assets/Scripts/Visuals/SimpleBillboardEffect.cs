using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SimpleBillboardEffect : MonoBehaviour
{
    [SerializeField] private bool ignoreY;
    private Camera targetCamera; // A câmera para a qual o objeto irá apontar

    private void Start()
    {
        // Se nenhuma câmera foi atribuída, usar a câmera principal
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        Vector3 transformPosition = transform.position;
        if (ignoreY)
        {
            transformPosition.y = 0;
        }
        transform.forward = (transformPosition - targetCamera.transform.position).normalized;
    }
}
