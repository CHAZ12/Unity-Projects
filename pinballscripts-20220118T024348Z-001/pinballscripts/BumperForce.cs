
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BumperForce : UdonSharpBehaviour
{
    public float ExplosionStrength = 100;
    public float ExplosionRadius = 5;
    //public Collision test;
    void Start()
    {
    }
    public void OnCollisionEnter(Collision col)
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (col.gameObject.name == "Ball")
        {
            col.rigidbody.AddExplosionForce(ExplosionStrength, transform.position, ExplosionRadius);
        }
    }
}
