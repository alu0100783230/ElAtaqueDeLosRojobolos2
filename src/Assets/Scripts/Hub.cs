﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Used for GUI */
public class Hub : MonoBehaviour
{

    public Text vida;
    public Text municion;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
      municion.text = "Ammo: " + GameController.getAmmo();
      vida.text = "Health: " + GameController.getHealth();
    }
}
