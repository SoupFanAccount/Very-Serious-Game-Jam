using System;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance {get;private set;}
    public enum Phase { Day, Night }
    public Phase currentPhase = Phase.Day;
    [SerializeField] float TimeSpeed;
    private float timeInaDay=86400f;
    private float currentTime;
    public float CurrentTime => currentTime;
    private float previousTime;
    public int CurrentHour => Mathf.FloorToInt(currentTime / 3600f);
    public bool isOpen {get;private set;}
    

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance=this;
    }

    public void Start()
    {
        currentTime = 6*3600f;
        StartDay();
        Debug.Log("The shop is closed");
    }

    public void Update()
    {
        previousTime=currentTime;
        currentTime += Time.deltaTime * TimeSpeed;       
        currentTime %= timeInaDay;

        if(currentPhase == Phase.Day && currentTime >= 18*3600f && previousTime < 18*3600f)
        {
            StartNight();
        }
        else if(currentPhase == Phase.Night && currentTime >= 6*3600f && previousTime< 6*3600f)
        {
           EndNight();
        }
    }

    public string ClockString()
    {
        int hours = Mathf.FloorToInt(currentTime / 3600f);
        int minutes = Mathf.FloorToInt((currentTime - hours * 3600f) / 60f);
        string ampm= hours < 12? "AM" : "PM";
        hours = hours % 12;
        if(hours == 0)
        {
            hours=12;
        }
        return string.Format("{0:00}:{1:00} {2}",hours,minutes,ampm);
    }
    
    public void StartDay()
    {
        currentPhase = Phase.Day;
        OpenShop(); // Added by Donags. isOpen was never being set true, breaking shop/audio
        if (AudioManager.Instance != null) AudioManager.Instance.PlayDayJingle();
        Debug.Log("Day " + GameManager.Instance.currentDay + " started.");
    }

    public void StartNight()
    {
        currentPhase = Phase.Night;
        CloseShop(); // Added by Donags. flip shop closed at night
        if (AudioManager.Instance != null) AudioManager.Instance.PlayNightJingle();
        Debug.Log("Night started. Dirty money to clean: " + GameManager.Instance.dirtyMoney);
    }

    public void EndNight()
    {
        GameManager.Instance.NextDay();
        StartDay();
    }

    public void OpenShop()
    {
        if(isOpen) return;

        isOpen=true;
        Debug.Log("The shop is open");
    }

    public void CloseShop()
    {
        if(!isOpen) return;

        isOpen=false;
        Debug.Log("The shop is closed");
    }
}