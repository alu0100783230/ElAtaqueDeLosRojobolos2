using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simply in game ammo item
public class Ammo : MonoBehaviour
{
  public uint ammount;


  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }

  void OnTriggerStay(Collider col) {
    if(col.gameObject.tag == "Player") {
      if (GameController.onIncreaseAmmo (ammount)) {
        Destroy(gameObject, 0);
      }
    }
  }
}
