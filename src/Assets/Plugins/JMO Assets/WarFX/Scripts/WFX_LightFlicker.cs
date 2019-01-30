using UnityEngine;
using System.Collections;

/**
 *	Rapidly sets a light on/off.
 *
 *	(c) 2015, Jean Moreno
**/

[RequireComponent(typeof(Light))]
public class WFX_LightFlicker : MonoBehaviour
{
	public float time = 0.05f;

  private bool emitting;
	private float timer;

	void Start ()
	{
    emitting = true;
		timer = time;
	}

	public void onStart () {
    emitting = true;
    StartCoroutine("Flicker");
  }

  public void onStop () {
    emitting = false;
  }

	IEnumerator Flicker()
	{
		while(emitting)
		{
      GetComponent<Light>().enabled = !GetComponent<Light>().enabled;

      do
      {
        timer -= Time.deltaTime;
        yield return null;
      }
      while(timer > 0);
      timer = time;
		}
	}
}
