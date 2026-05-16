using UnityEngine;

public class CutoutObject : MonoBehaviour
{
    public static int posId = Shader.PropertyToID("_playerPos");
    public static int sizeId = Shader.PropertyToID("_cutoutSize");

    [SerializeField] private Transform _targetObj;
    [SerializeField] private Material _cutoutMaterial;
    [SerializeField] private LayerMask _coutoutMasks;
    private Camera _mainCam;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

    private void Update()
    {
        var dir = _mainCam.transform.position - transform.position;
        var ray = new Ray(transform.position, dir.normalized);

        if(Physics.Raycast(ray, 3000, _coutoutMasks))
        {
            _cutoutMaterial.SetFloat(sizeId, 1);
        }
        else
        {
            _cutoutMaterial.SetFloat(sizeId, 0);
        }

        var view = _mainCam.WorldToViewportPoint(transform.position);
        _cutoutMaterial.SetVector(posId, view);
    }
}
