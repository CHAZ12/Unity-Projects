
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Flippers : UdonSharpBehaviour
{
    //public GameObject Target;
   // public float m_Thrust = 2f;
    public float restPositionR= 0F;
    public float pressedPositionR = 60F;
    public float flipperStrength = 10000F;
    public float flipperDamper = 25F;
    public HingeJoint Hinge;
    public Rigidbody Rigidbody;
    public bool pressed;
    public KeyCode button;

   public void Start()
    {
        Hinge.useSpring = true;
   
        Hinge.useLimits = true;
        Rigidbody.mass = 20f;
    }

    // Update is called once per frame
    public void Update()
    {

        var hspring = new JointSpring();
        hspring.spring = flipperStrength;
        hspring.damper = flipperDamper;
        if (Input.GetKey(button))
        {
            pressed = true;
           // Debug.LogWarning("pressed a key");
            hspring.targetPosition = pressedPositionR;
        }
        else
        {
            pressed = false;
            hspring.targetPosition = restPositionR;
        }
        Hinge.spring = hspring;
        // Hinge.limits.min = restPosition;
        // Hinge.limits.max = pressedPosition;
    } 
}
