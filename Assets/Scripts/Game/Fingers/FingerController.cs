using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerController : MonoBehaviour
{
    public GameObject finger_0, finger_1;

    void Start()
    {
        int int_finger_0 = PlayerPrefs.GetInt("finger_0");
        if (int_finger_0 != 1)
        {
            finger_0.SetActive(true);
        }
    }

    void Update()
    {
        int int_finger_0 = PlayerPrefs.GetInt("finger_0");
        if (int_finger_0 == 1)
        {
            finger_0.SetActive(false);
        }

        int int_finger_1 = PlayerPrefs.GetInt("finger_1");
        if (int_finger_1 != 1)
        {
            int int_fix_finger_1 = PlayerPrefs.GetInt("fix_finger_1");
            if (int_fix_finger_1 == 1)
            {
                finger_1.SetActive(true);
            }
        }
        else
        {
            finger_1.SetActive(false);

        }
    }
}