using UnityEngine;
using System.Collections;

public class Drag : MonoBehaviour {
	public GameObject circle1;        
    public GameObject circle2;
    public GameObject circle3;
    public GameObject circle4;    

    public Renderer r1;   
    public Renderer r2; 
    public Renderer r3; 
    public Renderer r4; 
 
    private LineRenderer line;   

    private void Start() {
    	circle1 = GameObject.Find("Circle1");
    	circle2 = GameObject.Find("Circle2");
    	circle3 = GameObject.Find("Circle3");
    	circle4 = GameObject.Find("Circle4");
 		
 		//hide circles
        r1 = circle1.GetComponent<Renderer>();
        r1.enabled = false;
        r2 = circle2.GetComponent<Renderer>();
        r2.enabled = false;
        r3 = circle3.GetComponent<Renderer>();
        r3.enabled = false;
        r4 = circle4.GetComponent<Renderer>();
        r4.enabled = false;

    	// Add a Line Renderer to the GameObject
        line = gameObject.AddComponent<LineRenderer>();
        // set the sorting layer
        line.sortingLayerName = "New Layer1";
        line.sortingOrder = 2;
        // set line color so we can see it
        line.material = new Material(Shader.Find("Particles/Additive"));
        line.SetColors(Color.white, Color.white);
        // Set the width of the Line Renderer
        line.SetWidth(0.01F, 0.01F);
        //hide line
        line.enabled = false;
        // Set the number of vertex fo the Line Renderer
        line.SetVertexCount(5);
    }                   

    private void Update() {
        // draw a 四邊形 with the circles as vertices
        line.SetPosition(0, circle1.transform.position);
        line.SetPosition(1, circle2.transform.position);
        line.SetPosition(2, circle4.transform.position);
        line.SetPosition(3, circle3.transform.position);
        line.SetPosition(4, circle1.transform.position);
    }

    public void OnClick1()
    {
    	//show circles and lines
    	line.enabled = true;
    	r1.enabled = true;
    	r2.enabled = true;
    	r3.enabled = true;
    	r4.enabled = true;
    }

    public void OnClick2()
    {
    	//hide circles and lines
    	line.enabled = false;
    	r1.enabled = false;
    	r2.enabled = false;
    	r3.enabled = false;
    	r4.enabled = false;
    }

    private void OnMouseDrag(){
    	Vector3 mos;
    	mos = Input.mousePosition;
    	// Vector3 arg3數值用來調整gameobj大小
    	gameObject.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(mos.x, mos.y, 10));
    }
}