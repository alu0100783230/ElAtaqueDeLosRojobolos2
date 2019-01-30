using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
  public float shoot_damage = 10;
  public uint ammo_per_shot = 1;
    // Start is called before the first frame update

  private FixedJoint joint;

  public GameObject decal;
  public AudioClip[] shoot_sounds;
  private AudioSource audio;

    void Start()
    {
      joint = GetComponent<FixedJoint> ();
      audio = GetComponent<AudioSource> ();

    }

    // Update is called once per frame
    void Update()
    {
      if (Input.GetKeyDown(KeyCode.Alpha0))
        onHandleShoot();
      else if (Input.GetKeyDown(KeyCode.Alpha1))
        onHandlePick();
      else if (Input.GetKeyUp(KeyCode.Alpha1))
        onHandleUnpick();
    }

    void onHandlePick() {
      RaycastHit hit;
      Vector3 dir = transform.forward;
      if (Physics.Raycast(transform.position, dir, out hit, 50)) {
        Debug.DrawRay(transform.position, dir, Color.yellow);
        if (hit.collider.tag == "Pickable") {
          joint.connectedBody = hit.collider.gameObject.GetComponent<Rigidbody>();
        }
      }
    }

    void onHandleUnpick() {
      joint.connectedBody = null;
    }

    void onHandleShoot () {
      if (GameController.TryShoot (ammo_per_shot)) {
        RaycastHit hit;
        Vector3 dir = transform.forward;

        audio.clip = shoot_sounds [Random.Range (0, shoot_sounds.Length - 1)];
        audio.Play ();

        if (Physics.Raycast(transform.position, dir, out hit, Mathf.Infinity)) {
          Debug.DrawRay(transform.position, dir, Color.magenta);
          if (hit.collider.tag == "Enemy") {
            hit.collider.gameObject.SendMessage("onGetDamage", shoot_damage);
          }
          var test_rigid = hit.collider.GetComponent<Rigidbody>();
          if (test_rigid != null) {
            test_rigid.AddForce (dir * 1000);
          }
          var decal_i = Instantiate (decal);
          decal_i.transform.position = hit.point;
        }
      } else {
        Debug.Log ("No ammo");
      }
    }
}
