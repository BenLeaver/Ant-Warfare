using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DescriptionOnHover : MonoBehaviour
{
    public GameObject commandDescription;

    public void DisplayDescription()
    {
        commandDescription.SetActive(true);
    }

    public void HideDescription()
    {
        commandDescription.SetActive(false);
    }
}
