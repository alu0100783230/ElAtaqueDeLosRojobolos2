﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2 : MonoBehaviour {

    public GameObject muzzle_flash;
    private ParticleSystem particles;

    public float damage_per_attack = 1;
    public float life = 20;

    private UnityEngine.AI.NavMeshAgent agent;
    private GameObject player;

    private bool can_attack = true;
    private float seconds_to_wait = 0.5f;
    private Vector3 last_pos;
    public Vector3 destination;

    private AudioSource audio_shoot;

    void Awake () {
      GameController.informPlayer += onInformPlayer;
      agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
      audio_shoot = GetComponents<AudioSource>()[1];
      particles = muzzle_flash.GetComponent<ParticleSystem>();
      particles.Stop();
    }

    // Start is called before the first frame update
    void Start() {
      agent.destination = transform.position;
    }

    // Update is called once per frame
    void Update() {}

    void onInformPlayer (GameObject p) {
      player = p;
      StartCoroutine(onHandleIa());
    }

    void onLookPlayer () {
      RaycastHit hit;
      Vector3 dir = player.transform.position - transform.position;
      if (Physics.Raycast(transform.position, dir, out hit, Mathf.Infinity)) {
        if (hit.collider.tag == "Player") {
          Debug.DrawRay(transform.position, dir, Color.green);
          Vector3 aux = hit.point;
          aux.y = transform.position.y;
          agent.destination = aux;
          if (Vector3.Distance (hit.point, transform.position) < 15 &&
            Vector3.Angle (transform.forward, dir) < 10)
            onShootPlayer();
          else
            onStopShooting();
        }
      }
      onStopShooting();
    }

    void onExplore () {
      Vector3 aux_vec = new Vector3(1, 0, 0);
      Vector3 best_target = agent.transform.position;
      float longer_distance = -100000f;

      for (uint i = 0; i < 24; i++) {
        var vector = Quaternion.Euler(0, (360 / 24) * i, 0) * aux_vec;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, vector, out hit, Mathf.Infinity)) {
          Debug.DrawRay(transform.position, vector, Color.blue);
          float aux_distance = Vector3.Distance (transform.position, hit.point);
          if (aux_distance > longer_distance) {
            longer_distance = aux_distance;
            best_target = hit.point;
            best_target.y = transform.position.y;
          }
        }
      }
      agent.destination = best_target;
    }

    void onStopShooting () {
      particles.transform.GetChild (1).SendMessage ("onStop");
      audio_shoot.Stop();
    }

    void onShootPlayer () {
      particles.Play();
      particles.transform.GetChild (1).SendMessage ("onStart");
      audio_shoot.Play();
      GameController.onDecreaseLife (damage_per_attack);
    }

    void onGetDamage (float damage) {
      if (damage < life)
        life -= damage;
      else
        Destroy(gameObject, 0);
    }

    IEnumerator onHandleIa () {
      for (;;) {
        onLookPlayer();
        if (Vector3.Distance (agent.transform.position, agent.destination) < 2)
          onExplore();
        yield return new WaitForSeconds (0.3f);
      }
    }

}
