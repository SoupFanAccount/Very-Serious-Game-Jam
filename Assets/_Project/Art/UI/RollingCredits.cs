using UnityEngine;

public class RollingCredits : MonoBehaviour
{
    public float scrollSpeed = 150f;
    public float targetY = 5550f;

    private RectTransform rectTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();    
    }


    // Update is called once per frame
    void Update()
    {
        if(rectTransform.position.y < targetY)
        {        
            rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
        }
        else
        {
            scrollSpeed = 0f;
        }
    }
}
