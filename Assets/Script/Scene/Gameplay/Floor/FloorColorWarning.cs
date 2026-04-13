using UnityEngine;

public class FloorColorWarning : MonoBehaviour
{
    [Header("Target Renderers")]
    public Renderer[] targetRenderers;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;

    private Material[][] cachedMaterials;

    private void Awake()
    {
        CacheMaterials();
        ResetColor();
    }

    private void CacheMaterials()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            Debug.LogWarning("FloorColorWarning: Target Renderers belum diisi pada " + gameObject.name);
            return;
        }

        cachedMaterials = new Material[targetRenderers.Length][];

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] != null)
            {
                cachedMaterials[i] = targetRenderers[i].materials;
            }
        }
    }

    public void SetWarning(bool isWarning)
    {
        SetColor(isWarning ? warningColor : normalColor);
    }

    public void ResetColor()
    {
        SetColor(normalColor);
    }

    private void SetColor(Color targetColor)
    {
        if (cachedMaterials == null)
        {
            CacheMaterials();
        }

        if (cachedMaterials == null) return;

        for (int i = 0; i < cachedMaterials.Length; i++)
        {
            if (cachedMaterials[i] == null) continue;

            for (int j = 0; j < cachedMaterials[i].Length; j++)
            {
                Material mat = cachedMaterials[i][j];
                if (mat == null) continue;

                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", targetColor);
                }
                else if (mat.HasProperty("_Color"))
                {
                    mat.SetColor("_Color", targetColor);
                }
            }
        }
    }
}