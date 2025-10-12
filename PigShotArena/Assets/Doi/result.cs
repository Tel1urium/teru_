using UnityEngine;

public class result : MonoBehaviour
{
    public GameObject p1;
    public GameObject p2;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        if(player1.alive1 == false )
        {
            ChangeColor(p2, Color.blue);
            ChangeColor(p1, Color.red);
        }
        else if(player1.alive1 == true)
        {
            ChangeColor(p2, Color.red);
            ChangeColor(p1, Color.blue);
        } 
    }
    void ChangeColor(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }
}
