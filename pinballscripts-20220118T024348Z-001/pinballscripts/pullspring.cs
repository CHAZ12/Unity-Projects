
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class pullspring : UdonSharpBehaviour
{
    public GameObject Ball;
    public float Power = 100f;
    public float maxPower = 750f;
    public KeyCode Button;

    public bool ready = false;

    public void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.name == "Ball")
        {
            ready = true;
        }
    }
    public void OnCollisionExit(Collision col)
    {
        if (col.gameObject.name == "Ball")
        {
            Power = 0f;
            ready = false;
        }
    }
    public void Update()
    {
        if (Input.GetKey(Button) && ready)
        {
            if(Power <= maxPower)
            {
                Power += 100 * Time.deltaTime;
            }
        }
        if (Input.GetKeyUp(Button))
        {
            {
                Ball.GetComponent<Rigidbody>().AddForce(Power * Vector3.back);
                Power = 0f;
            }
        }

    }

}
