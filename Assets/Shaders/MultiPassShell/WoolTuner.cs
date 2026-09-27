using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// [WOOL PROTOTYPE]  Live parameter tuner.
//
// Drop this on any GameObject in the scene (an empty one is fine). It finds
// every material in the scene that uses the Multi-Pass wool shader and pushes
// the values below onto them, so you can drag sliders and watch the Scene view
// update in edit mode (no Play needed).
//
// It writes to the SHARED material, so the values are saved into the .mat asset.
// Use the context menu "Reset Wool To Straight Fur" to A/B against the original.
//
// Delete this file when the wool look is locked in.
// =============================================================================
[ExecuteAlways]
[AddComponentMenu("Fur/Wool Tuner (Prototype)")]
public class WoolTuner : MonoBehaviour
{
    [Header("Wool")]
    [Range(0f, 3f)]     public float woolCurlAmount = 0.9f;   // 0 = original straight fur
    [Range(0f, 3f)]     public float woolCurlTurns  = 1.2f;   // 0 = lean, >1 = screw
    [Range(0.1f, 20f)]  public float woolClumpScale = 5f;     // smaller clumps when higher
    [Range(0.1f, 3f)]   public float woolPuff       = 1f;     // overall volume

    [Header("Fur (existing parameters)")]
    [Range(0.05f, 0.5f)] public float alphaCutout       = 0.23f;
    [Range(0f, 10f)]     public float furScale          = 2.1f;
    [Range(0f, 0.5f)]    public float totalShellStep    = 0.288f;
    [Range(0.01f, 5f)]   public float lengthIntensity   = 0.66f;
    [Range(0f, 1f)]      public float groomingIntensity = 1f;

    [Header("Target (leave empty to auto-find in scene)")]
    public Material targetMaterial;

    static readonly string kWoolShaderToken = "Multi-Pass Shell/Lit";

    void OnEnable()   { Apply(); }
    void OnValidate() { Apply(); }

    [ContextMenu("Reset Wool To Straight Fur")]
    void ResetWool()
    {
        woolCurlAmount = 0f;
        Apply();
    }

    [ContextMenu("Log Target Materials")]
    void LogTargets()
    {
        foreach (var mat in ResolveTargets())
            Debug.Log($"[WoolTuner] target: {mat.name} ({mat.shader.name})", mat);
    }

    public void Apply()
    {
        foreach (var mat in ResolveTargets())
        {
            mat.SetFloat("_WoolCurlAmount", woolCurlAmount);
            mat.SetFloat("_WoolCurlTurns",  woolCurlTurns);
            mat.SetFloat("_WoolClumpScale", woolClumpScale);
            mat.SetFloat("_WoolPuff",       woolPuff);

            mat.SetFloat("_AlphaCutout",        alphaCutout);
            mat.SetFloat("_FurScale",           furScale);
            mat.SetFloat("_TotalShellStep",     totalShellStep);
            mat.SetFloat("_FurLengthIntensity", lengthIntensity);
            mat.SetFloat("_GroomingIntensity",  groomingIntensity);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(mat);
#endif
        }
    }

    IEnumerable<Material> ResolveTargets()
    {
        if (targetMaterial != null)
        {
            yield return targetMaterial;
            yield break;
        }

        var seen = new HashSet<Material>();
#if UNITY_2022_2_OR_NEWER
        var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
#else
        var renderers = FindObjectsOfType<Renderer>();
#endif
        foreach (var r in renderers)
        {
            foreach (var m in r.sharedMaterials)
            {
                if (m != null && m.shader != null &&
                    m.shader.name.Contains(kWoolShaderToken) && seen.Add(m))
                {
                    yield return m;
                }
            }
        }
    }
}
