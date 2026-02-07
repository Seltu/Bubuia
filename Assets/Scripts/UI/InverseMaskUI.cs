using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class InverseMaskUI : Image
{
    public override Material materialForRendering
    {
        get
        {
            Material material =  base.materialForRendering;

            material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);

            return material;
        }
    }
}
