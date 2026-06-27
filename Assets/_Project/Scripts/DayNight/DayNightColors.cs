using UnityEngine;

public class DayNightColors : MonoBehaviour
{
    public DayNightCycle dayNightCycle;
    public Color dayColor = new Color(116, 158, 197);
    public Color nightColor = new Color(116, 13, 46);

    public Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (dayNightCycle.currentPhase == DayNightCycle.Phase.Day)
        {
            cam.backgroundColor = dayColor;
        }
        else if (dayNightCycle.currentPhase == DayNightCycle.Phase.Night)
        {
            cam.backgroundColor = nightColor;
        }
    }
}
