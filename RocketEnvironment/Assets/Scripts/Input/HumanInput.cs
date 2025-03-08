using UnityEngine;

public class HumanInput : MonoBehaviour
{
    public Rocket rocket;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            rocket.engines[0].SetThrust(1f);
        }
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            rocket.engines[0].SetThrust(0f);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            rocket.engines[1].SetThrust(1f);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            rocket.engines[1].SetThrust(0f);
        }


        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            rocket.engines[2].SetThrust(1f);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            rocket.engines[2].SetThrust(0f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            rocket.engines[3].SetThrust(1f);
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            rocket.engines[3].SetThrust(0f);
        }
    }
}
