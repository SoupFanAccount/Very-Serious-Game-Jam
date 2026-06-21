using System;
using UnityEngine;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    public enum Phase { Day, Night }
    public Phase currentPhase = Phase.Day;
    [SerializeField] float TimeSpeed;
    [SerializeField] TextMeshProUGUI clockUI;
    private float timeInaDay=86400f;
    private float currentTime;
    private float previousTime;

    public void Start()
    {
        currentTime = 6*3600f;
        StartDay();
    }

    public void Update()
    {
        previousTime=currentTime;
        currentTime += Time.deltaTime * TimeSpeed;       
        currentTime %= timeInaDay;
        UpdateClock();

        if(currentPhase == Phase.Day && currentTime >= 18*3600f && previousTime < 18*3600f)
        {
            StartNight();
        }
        else if(currentPhase == Phase.Night && currentTime >= 6*3600f && previousTime< 6*3600f)
        {
           EndNight();
        }
    }

    public void UpdateClock()
    {
        int hours = Mathf.FloorToInt(currentTime / 3600f);
        int minutes = Mathf.FloorToInt((currentTime - hours * 3600f) / 60f);
        string ampm= hours < 12? "AM" : "PM";
        hours = hours % 12;
        if(hours == 0)
        {
            hours=12;
        }
        string clockString = string.Format("{0:00}:{1:00} {2}",hours,minutes,ampm);
        clockUI.text=clockString;
    }
    
    public void StartDay()
    {
        currentPhase = Phase.Day;
        Debug.Log("Day " + GameManager.Instance.currentDay + " started.");
    }

    public void StartNight()
    {
        currentPhase = Phase.Night;
        Debug.Log("Night started. Dirty money to clean: " + GameManager.Instance.dirtyMoney);
    }

    public void EndNight()
    {
        GameManager.Instance.NextDay();
        if(GameManager.Instance.currentDay > 7)
        {
            Debug.Log("End of the week");
            TimeSpeed=0f;
        }
        else
        {
            StartDay();
        }       
    }
}