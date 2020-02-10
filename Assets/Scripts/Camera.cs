using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    // Start is called before the first frame update

    public Mario mario;
    public float cameraSpeed;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {    
            cameraSpeed = mario.currentSpeed;
            cameraSpeed = Mathf.Max(cameraSpeed, 0);
           
        if (mario.transform.position.x >= transform.position.x)
        { 
            Vector2 movement = new Vector2(cameraSpeed, 0);
            transform.Translate(movement * Time.deltaTime);
        }
        
    }
}
