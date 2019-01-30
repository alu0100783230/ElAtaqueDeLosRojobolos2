using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Manages the flying drone.
 *
 * The system uses a navmesh to move the agent and several raycasts
 * to obtain information about it's environment.
 *
 * This enemy tries to collide with the player and then explodes.
 */
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

    // Agent's AI is started when this is called
    void onInformPlayer (GameObject p) {
      player = p;
      StartCoroutine(onHandleIa());
    }

    /* Check if the player is visible using a raycast oriented to him and
     * then checkng the tag asociated to the collider reached.
     *
     * If the player is visible the agent move torwads him.
     * */
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

    /* Shoot 24 raycast 360º araound the agent to obtain the farest
     * point in sight.
     * */
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

    // Handle the collision and explosion with the player
    void OnCollisionEnter (Collision col) {
      if(col.gameObject.tag == "Player" && can_attack) {
        GameController.onDecreaseLife (damage_per_attack);
        var aux = Instantiate (explosion);
        aux.transform.position = transform.position;
        Destroy(gameObject);
      }
    }

    // Handles the damage caused by the player
    void onGetDamage (float damage) {
      if (damage < life)
        life -= damage;
      else
        Destroy(gameObject, 0);
    }

    /* This coroutine handles AI every 0.5 seconds, avoiding the performance cost of
     * doing so in the Update function.
     * */
    IEnumerator onHandleIa () {
      for (;;) {
        onLookPlayer();
        if (Vector3.Distance (agent.transform.position, agent.destination) < 4.5)
          onExplore();
        yield return new WaitForSeconds (0.5f);
      }
    }
}
