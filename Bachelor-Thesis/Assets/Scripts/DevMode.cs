using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DevMode : MonoBehaviour {

    [SerializeField]
    TMP_InputField devConsol;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (Input.GetKeyUp(KeyCode.V))
                {
                    devConsol.gameObject.SetActive(!devConsol.gameObject.activeSelf);
                }
            }
        }
    }
}
