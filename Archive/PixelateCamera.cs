using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PixelateCamera : MonoBehaviour
{
    public Shader pixelateShader;
    private Material pixelateMaterial;
    public float pixelSize = 0.05f;

    void Start()
    {
        pixelateMaterial = new Material(pixelateShader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        pixelateMaterial.SetFloat("_PixelSize", pixelSize);
        Graphics.Blit(src, dest, pixelateMaterial);
    }
}
