using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SpriteSheetSwapper : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private int currentAlt = 0;

    private Dictionary<string, Dictionary<int, Sprite>> spriteMapping = new();

    public void LateUpdate()
    {
        if (_spriteRenderer.sprite == null) return;

        string spriteName = _spriteRenderer.sprite.name;

        if (spriteMapping.ContainsKey(spriteName) && spriteMapping[spriteName].ContainsKey(currentAlt))
        {
            _spriteRenderer.sprite = spriteMapping[spriteName][currentAlt];
        }
    }

    public void BuildSpriteMapping(string originalRef, string newRef, int alt)
    {
        Addressables.LoadAssetAsync<Sprite[]>(newRef).Completed += handle =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                Sprite[] newSprites = handle.Result;

                Addressables.LoadAssetAsync<Sprite[]>(originalRef).Completed += handleOriginal =>
                {
                    if (handleOriginal.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        Sprite[] originalSprites = handleOriginal.Result;

                        for (int i = 0; i < originalSprites.Length; i++)
                        {
                            if (i < newSprites.Length)
                            {
                                string name = originalSprites[i].name;

                                if (!spriteMapping.ContainsKey(name))
                                    spriteMapping[name] = new Dictionary<int, Sprite>();

                                spriteMapping[name][alt] = newSprites[i];
                            }
                        }

                        currentAlt = alt;
                    }
                    else
                        Debug.LogError($"Failed to load originalRef: {originalRef}");
                };
            }
            else
            {
                Debug.LogError($"Failed to load newRef: {newRef}");
            }
        };
    }


    private void Start()
    {
        Debug.unityLogger.logEnabled = true;
    }

    public void SetCurrentAlt(int alt)
    {
        currentAlt = alt;
    }
}
