using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

  public delegate void InformPlayer(GameObject g);
  public static event InformPlayer informPlayer;

  public float player_life_inspector;
  public float max_player_life_inspector;

  public uint player_ammo_inspector;
  public uint max_player_ammo_inspector;

  static float player_life;
  static float max_player_life;

  static uint player_ammo;
  static uint max_player_ammo;

  private static AudioSource audio;
  private static AudioClip health_clip;
  private static AudioClip damage_clip;
  private static AudioClip reload_clip;

  public AudioClip inspector_health_clip;
  public AudioClip inspector_damage_clip;
  public AudioClip inspector_reload_clip;

  private static GameObject player;
  // Singleton patter, avoid multiple controllers
  public static GameController instance;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake() {
      if (instance == null) {
        instance = this;
      } else if (instance != this) {// If it's not this, destroy it
        Destroy(gameObject);
      }

      player_ammo = player_ammo_inspector;
      player_life = player_life_inspector;

      max_player_ammo = max_player_ammo_inspector;
      max_player_life = max_player_life_inspector;


      health_clip = inspector_health_clip;
      damage_clip = inspector_damage_clip;
      reload_clip = inspector_reload_clip;

      audio = GetComponent<AudioSource> ();
    }

    public static void CustomStart () {
      player = GameObject.FindGameObjectWithTag("Player");
      informPlayer (player);
    }

    public static GameObject GetPlayer () {
      return player;
    }

    public static void onHandleLose () {
      Debug.Log ("YOU LOSE");
    }

    public static void onDecreaseLife (float ammount) {

      audio.clip = damage_clip;
      audio.Play();

      if (player_life < ammount)
        onHandleLose();
      else
        player_life -= ammount;

      Debug.Log ("Decreased life to " + player_life);
    }

    public static bool onIncreaseLife (float ammount) {
      if (player_life < max_player_life) {
        player_life += ammount;
        if (player_life > max_player_life)
          player_life = max_player_life;

        audio.clip = health_clip;
        audio.Play();

        return true;
      }
      return false;
    }

    public static void onDecreaseAmmo (uint ammount) {
      if (player_ammo < ammount)
        player_ammo = 0;
      else
        player_ammo -= ammount;
    }

    public static bool onIncreaseAmmo (uint ammount) {
      Debug.Log (player_ammo + " " + max_player_ammo);
      if (player_ammo < max_player_ammo) {
        player_ammo += ammount;
        if (player_ammo > max_player_ammo)
          player_ammo = max_player_ammo;

        audio.clip = reload_clip;
        audio.Play();

        return true;
      }
      return false;
    }

    public static bool TryShoot (uint ammount) {
      if (ammount > player_ammo)
        return false;
      onDecreaseAmmo (ammount);
      return true;
    }

    public static float getAmmo()
    {
        return player_ammo;
    }

    public static float getHealth()
    {
        return player_life;
    }
}
