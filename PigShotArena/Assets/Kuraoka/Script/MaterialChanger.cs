using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    private Renderer renderer;
    private Material instanceMaterial;

    void Start()
    {
        // Rendererコンポーネントを取得
        renderer = GetComponent<Renderer>();

        // 元のマテリアルのインスタンスを作成
        instanceMaterial = new Material(renderer.sharedMaterial);

        // インスタンス化したマテリアルをRendererに適用
        renderer.material = instanceMaterial;

        // 色を変更（例えば赤に変更）
        instanceMaterial.SetColor("_Color", Color.red);
    }
}
