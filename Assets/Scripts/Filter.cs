using UnityEngine;
using System.Collections;

public class Filter : MonoBehaviour 
{
    public float speed = 1;

    AudioLowPassFilter filter;
    float freq;
    float targetFreq;

    void Start()
    {
        filter = GetComponent<AudioLowPassFilter>();
        freq = filter.cutoffFrequency;
        targetFreq = freq;
        filter.cutoffFrequency = 0;
    }

    void Update()
    {
        //Debug.Log(freq + " " + targetFreq);

        if (filter.cutoffFrequency != targetFreq)
        {
            int sign = filter.cutoffFrequency > targetFreq ? -1 : 1;
            filter.cutoffFrequency += speed * sign;

            if (Mathf.Abs(filter.cutoffFrequency - targetFreq) < speed)
                filter.cutoffFrequency = targetFreq;
        }
    }

    public void StartFilter()
    {
        targetFreq = freq;
    }

    public void StopFilter()
    {
        targetFreq = 5000;
    }
}
