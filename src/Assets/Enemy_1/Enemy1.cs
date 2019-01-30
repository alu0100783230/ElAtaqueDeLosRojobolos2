using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : MonoBehaviour {

    public float damage_per_attack = 20;
    public float life = 20;

    private UnityEngine.AI.NavMeshAgent agent;
    private GameObject player;

    private bool can_attack = true;
    private float seconds_to_wait = 0.5f;
    private Vector3 last_pos;
    public Vector3 destination;
    public GameObject explosion;

    void Awake () {
      GameController.informPlayer += onInformPlayer;
      agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    // Start is called before the first frame update
    void Start() {
      agent.destination = transform.position + new Vector3(0, 1, 0);
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
        }
      }
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


    void OnCollisionEnter (Collision col) {
      if(col.gameObject.tag == "Player" && can_attack) {
        GameController.onDecreaseLife (damage_per_attack);
        var aux = Instantiate (explosion);
        aux.transform.position = transform.position;
        Destroy(gameObject);
      }
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
        if (Vector3.Distance (agent.transform.position, agent.destination) < 4.5)
          onExplore();
        yield return new WaitForSeconds (0.5f);
      }
    }
}
