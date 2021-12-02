using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    [SerializeField] GameObject visuals;
    [SerializeField] ParticleSystem particles;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "player")
        {
            visuals.SetActive(false);
            GetComponent<Collider>().enabled = false;
            particles.Play();
            other.gameObject.GetComponent<PlayerController>().ChangeEnergy(10f);
        }
    }

}
