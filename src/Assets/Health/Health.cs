using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simply in game health item
public class Health : MonoBehaviour
{
    public float ammount;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerStay(Collider target) {
      if(target.tag == "Player"){
        if (GameController.onIncreaseLife (ammount))
          Destroy(gameObject, 0);
      }
    }
}
