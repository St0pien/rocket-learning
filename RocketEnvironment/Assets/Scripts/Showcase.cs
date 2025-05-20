using UnityEngine;

public class Showcase : MonoBehaviour
{
    public Rocket rocket;
    public float MinHeight = 50;
    public float MaxHeight = 400;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(rocket.transform);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rocket.transform.position = new Vector3(0, Random.Range(MinHeight, MaxHeight), 0);
        }
    }
}
