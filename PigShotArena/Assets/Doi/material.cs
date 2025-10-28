using UnityEngine;

public class material : MonoBehaviour
{
    public Renderer p1;
    public Renderer p2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeColor(p2, UnityEngine.Color.blue);
        ChangeColor(p1, UnityEngine.Color.red);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ChangeColor(Renderer renderer, UnityEngine.Color color)
    {
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }
}
