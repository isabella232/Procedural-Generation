using UnityEngine;
using System.Collections;

public class RiseGracefully : MonoBehaviour {

    public float TargetHeight = 3f;
    public float k = 1.0f;
    public float c = 0.2f;


    private float velocity = 0;

	
	// Update is called once per frame
	void Update () {

        float distance = TargetHeight - transform.position.y;
        float delta = Time.deltaTime;

        velocity = k*distance - c*velocity;

        transform.position += new Vector3(0, delta*velocity, 0);
	}
}
