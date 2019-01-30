using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum MatrixTypes : uint {
  HORIZONTAL_WALL,
  VERTICAL_WALL,
  EMPTY
};

public struct Maze {
  public uint width;
  public uint height;
  public uint[,] matrix;

  public Maze (uint w, uint h) {
    width = w;
    height = h;

    matrix = new uint[h, w];

    for (uint i = 1; i < h - 1; i++) {
      for (uint j = 1; j < w - 1; j++) {
        matrix[i, j] = (uint) MatrixTypes.EMPTY;
      }
    }

    for (uint i = 0; i < h; i++) {
      matrix[i,    0] = (uint) MatrixTypes.VERTICAL_WALL;
      matrix[i,w - 1] = (uint) MatrixTypes.VERTICAL_WALL;
    }

    for (uint j = 0; j < h; j++) {
      matrix[0,     j] = (uint) MatrixTypes.HORIZONTAL_WALL;
      matrix[h - 1, j] = (uint) MatrixTypes.HORIZONTAL_WALL;
    }
  }

  public Maze transpose () {
    Maze maze = new Maze (height, width);
    for (uint i = 1; i < height - 1; i++) {
      for (uint j = 1; j < width - 1; j++) {
        maze.matrix[j, i] = matrix[i, j];
      }
    }
    return maze;
  }

  public void print () {

    char[] translator = { '|', '_', ' ' };

    for (uint i = 0; i < height; i++) {
      for (uint j = 0; j < width; j++) {
        Debug.Log (translator[matrix[i, j]]);
      }
    }
  }

};

public struct Limits {
  public uint x_0;
  public uint x_1;
  public uint y_0;
  public uint y_1;
  public int empty_h;
  public int empty_v;

  public Limits (uint a,
                 uint b,
                 uint c,
                 uint d,
                 int e,
                 int f)
  {
    x_0 = a;
    x_1 = b;

    y_0 = c;
    y_1 = d;

    empty_h = e;
    empty_v = f;
  }
};


public class LaberynthGenerator : MonoBehaviour
{

    public delegate bool Lambda(ref uint x, ref uint y);
    private Lambda lambda;

    public uint width;
    public uint height;

    public GameObject[] Walls;
    public GameObject[] Floor_tiles;
    public GameObject collision_cube;

    public GameObject[] spawneable;
    public float[] probabilities;

    public GameObject floor;
    public GameObject player;


    public uint min_chamber_size_x = 1;
    public uint min_chamber_size_y = 1;

    private int create_passage_y (Maze maze, uint y_0, uint y_1, uint x) {
      if ((y_1 - y_0) > 2) {
        int y_pos = (int) Random.Range(y_0 + 1, y_1 - 1);
        maze.matrix[(uint)y_pos, x] = (uint) MatrixTypes.EMPTY;
        return y_pos;
      }
      return -1;
    }

    private int create_passage_x (Maze maze, uint x_0, uint x_1, uint y) {
      if ((x_1 - x_0) > 2) {
        int x_pos =  (int) Random.Range(x_0 + 1, x_1 - 1);
        maze.matrix[y, (uint)x_pos] = (uint) MatrixTypes.EMPTY;
        return x_pos;
      }
      return -1;

    }

    private void subdivide (Maze maze, Limits limits) {
      bool subdivide_x = ((limits.x_1 - limits.x_0) > min_chamber_size_x + 2);
      bool subdivide_y = ((limits.y_1 - limits.y_0) > min_chamber_size_y + 2);

      // Stop conditions

      if (!subdivide_x || !subdivide_y)
        return;

      if (limits.empty_h == -1 || limits.empty_v == -1)
        return;

      // Cut in 4 pieces
      uint x_coordinate = (uint) limits.empty_h;
      uint y_coordinate = (uint) limits.empty_v;

      // Check which side of each passage to choose
      if ((limits.empty_h - limits.x_0) < (limits.x_1 - limits.empty_h))
        x_coordinate++;
      else
        x_coordinate--;

      if ((limits.empty_v - limits.y_0) < (limits.y_1 - limits.empty_v))
        y_coordinate++;
      else
        y_coordinate--;

      int x_passage_pos_a;
      int x_passage_pos_b;
      int y_passage_pos_a;
      int y_passage_pos_b;

      // Create Wall X
      for (uint i = limits.x_0 + 1; i <= limits.x_1 - 1; i++)
        maze.matrix[y_coordinate, i] = (uint) MatrixTypes.HORIZONTAL_WALL;

      x_passage_pos_a = create_passage_x(maze, limits.x_0, x_coordinate, y_coordinate);
      x_passage_pos_b = create_passage_x(maze, x_coordinate, limits.x_1, y_coordinate);

      // Create Wall Y
      for (uint i = limits.y_0 + 1; i <= limits.y_1 - 1; i++)
        maze.matrix[i, x_coordinate] = (uint) MatrixTypes.VERTICAL_WALL;

      y_passage_pos_a = create_passage_y(maze, limits.y_0, y_coordinate, x_coordinate);
      y_passage_pos_b = create_passage_y(maze, y_coordinate, limits.y_1, x_coordinate);


      Limits aux_1 = new Limits (limits.x_0,
                                 x_coordinate,
                                 limits.y_0,
                                 y_coordinate,
                                 x_passage_pos_a,
                                 y_passage_pos_a);

      Limits aux_2 = new Limits (limits.x_0,
                                 x_coordinate,
                                 y_coordinate,
                                 limits.y_1,
                                 x_passage_pos_a,
                                 y_passage_pos_b);

      Limits aux_3 = new Limits (x_coordinate,
                                 limits.x_1,
                                 limits.y_0,
                                 y_coordinate,
                                 x_passage_pos_b,
                                 y_passage_pos_a);

      Limits aux_4 = new Limits (x_coordinate,
                                 limits.x_1,
                                 y_coordinate,
                                 limits.y_1,
                                 x_passage_pos_b,
                                 y_passage_pos_b);

      // Recursive Subdivide
      subdivide(maze, aux_1);
      subdivide(maze, aux_2);
      subdivide(maze, aux_3);
      subdivide(maze, aux_4);
    }

    struct Point {
      public Point (uint X, uint Y) {
        x = X;
        y = Y;
      }

      public uint x;
      public uint y;
    }

    struct Area {
      public Area (Point A, Point B) {
        a = A;
        b = B;
      }

      public Point a;
      public Point b;
    };

    bool has_adjacent_node_left (Maze maze, uint x, uint y) {
      if (x > 0)
        return (maze.matrix[y, x - 1] != (uint) MatrixTypes.EMPTY);
      return false;
    }

    bool has_adjacent_node_right (Maze maze, uint x, uint y) {
      if (x < maze.width - 1)
        return (maze.matrix[y, x + 1] != (uint) MatrixTypes.EMPTY);
      return false;
    }

    bool has_adjacent_node_up (Maze maze, uint x, uint y) {
      if (y > 0)
        return (maze.matrix[y - 1, x] != (uint) MatrixTypes.EMPTY);
      return false;
    }

    bool has_adjacent_node_down (Maze maze, uint x, uint y) {
      if (y < maze.height - 1)
        return (maze.matrix[y + 1, x] != (uint) MatrixTypes.EMPTY);
      return false;
    }

    void explore_up (Maze maze, List<Area> areas, uint x, uint y) {
      uint aux = y;

      if (maze.matrix[y, x] == (uint) MatrixTypes.EMPTY)
        return;
      maze.matrix[y, x] = (uint) MatrixTypes.EMPTY;


      for (int i = (int)y - 1; i >= 0; i--) {
        if (maze.matrix[i, x] != (uint) MatrixTypes.EMPTY) {
          aux--;
          maze.matrix[i, x] = (uint) MatrixTypes.EMPTY;
          if (has_adjacent_node_left(maze, x, (uint)i))
            explore_left (maze, areas, x - 1, (uint)i);
          if (has_adjacent_node_right(maze, x, (uint)i))
            explore_right (maze, areas, x + 1, (uint)i);
        } else {
          break;
        }
      }

      Area aux_area;
      aux_area.a = new Point (x, aux);
      aux_area.b = new Point (x, y);

      areas.Add (aux_area);
    }

    void explore_down (Maze maze, List<Area> areas, uint x, uint y) {
      uint aux = y;

      if (maze.matrix[y, x] == (uint) MatrixTypes.EMPTY)
        return;
      maze.matrix[y, x] = (uint) MatrixTypes.EMPTY;


      for (uint i = y + 1; i < maze.height; i++) {
        if (maze.matrix[i, x] != (uint) MatrixTypes.EMPTY) {
          aux++;
          maze.matrix[i, x] = (uint) MatrixTypes.EMPTY;
          if (has_adjacent_node_left(maze, x, (uint)i))
            explore_left (maze, areas, x - 1, (uint)i);
          if (has_adjacent_node_right(maze, x, (uint)i))
            explore_right (maze, areas, x + 1, (uint)i);
        } else {
          break;
        }
      }

      Area aux_area;
      aux_area.a = new Point (x, y);
      aux_area.b = new Point (x, aux);

      areas.Add (aux_area);
    }

    void explore_left (Maze maze, List<Area> areas, uint x, uint y) {
      uint aux = x;

      if (maze.matrix[y, x] == (uint) MatrixTypes.EMPTY)
        return;
      maze.matrix[y, x] = (uint) MatrixTypes.EMPTY;

      for (int i = (int)x - 1; i >= 0; i--) {
        if (maze.matrix[y, i] != (uint) MatrixTypes.EMPTY) {
          aux--;
          maze.matrix[y, i] = (uint) MatrixTypes.EMPTY;
          if (has_adjacent_node_up(maze, (uint)i, y))
            explore_up (maze, areas, (uint)i, y - 1);
          if (has_adjacent_node_down(maze, (uint)i, y))
            explore_down (maze, areas, (uint)i, y + 1);
        } else {
          break;
        }
      }

      Area aux_area;
      aux_area.a = new Point (aux, y);
      aux_area.b = new Point (x, y);

      areas.Add (aux_area);
    }

    void explore_right (Maze maze, List<Area> areas, uint x, uint y) {
      uint aux = x;

      if (maze.matrix[y, x] == (uint) MatrixTypes.EMPTY)
        return;
      maze.matrix[y, x] = (uint) MatrixTypes.EMPTY;


      for (uint i = x + 1; i < maze.width; i++) {
        if (maze.matrix[y, i] != (uint) MatrixTypes.EMPTY) {
          aux++;
          maze.matrix[y, i] = (uint) MatrixTypes.EMPTY;
          if (has_adjacent_node_up(maze, (uint)i, y))
            explore_up (maze, areas, (uint)i, y - 1);
          if (has_adjacent_node_down(maze, (uint)i, y))
            explore_down (maze, areas, (uint)i, y + 1);
        } else {
          break;
        }
      }

      Area aux_area;
      aux_area.a = new Point (x, y);
      aux_area.b = new Point (aux, y);

      areas.Add (aux_area);
    }

    void explore_start (Maze maze, List<Area> areas, uint x, uint y) {
      if (has_adjacent_node_down(maze, x, y))
        explore_down (maze, areas, x, y);
      else if (has_adjacent_node_up(maze, x, y))
        explore_up (maze, areas, x, y);
      else if (has_adjacent_node_left(maze, x, y))
        explore_left (maze, areas, x, y);
      else
        explore_right (maze, areas, x, y);
    }


    Maze GenerateLaberynth () {
      Maze maze = new Maze (width, height);

      int open_x = (int) Random.Range (3, width  - 3);
      int open_y = (int) Random.Range (3, height - 3);

      //maze.matrix[0, open_x] = (uint) MatrixTypes.EMPTY;
      //maze.matrix[open_y, 0] = (uint) MatrixTypes.EMPTY;

      Limits aux = new Limits (0,
                               width - 1,
                               0,
                               height - 1,
                               open_x,
                               open_y);

      subdivide(maze, aux);
      return maze;
    }


    GameObject instanceElement (float x, float y, float z, GameObject obj) {
      var i_element = Instantiate (obj);
      i_element.transform.position = new Vector3(x, y, z);
      return i_element;
    }

    GameObject tryToPopulate (float x, float y) {

      for (uint i = 0; i < probabilities.Length; i++) {
        float val = Random.Range (0.0f, 1.0f);
        if (val <= probabilities[i])
          return instanceElement (x, 0.5f, y, spawneable[i]);
      }
      return null;
    }

    void InstantiateProps (Maze maze) {
      var grouper_p = Instantiate (new GameObject("props_group"));
      grouper_p.transform.parent = transform;

      float size = Walls[0].GetComponentsInChildren<MeshFilter>()[0].sharedMesh.bounds.size.x;
      float h_size = size / 2; // half size

      for (uint i = 0; i < height; i++) {
        for (uint j = 0; j < width; j++) {
          if (maze.matrix[i, j] == (uint) MatrixTypes.EMPTY) {
            var prop = tryToPopulate (i * size, j * size - h_size);
            if (prop != null)
              prop.transform.parent = grouper_p.transform;
          }
        }
      }
    }

    void InstantiateLaberynth (Maze maze) {
      // As the elements are square any compontent of the vector should work

      var grouper_w = Instantiate (new GameObject("walls_group"));
      grouper_w.transform.parent = transform;

      float size = Walls[0].GetComponentsInChildren<MeshFilter>()[0].sharedMesh.bounds.size.x;
      float v_size = Walls[0].GetComponentsInChildren<MeshFilter>()[0].sharedMesh.bounds.size.y;
      float h_size = size / 2; // half size

      for (uint i = 0; i < height; i++) {
        for (uint j = 0; j < width; j++) {
          if (maze.matrix[i, j] != (uint) MatrixTypes.EMPTY) {
            var wall = instanceElement (i * size, v_size / 2, j * size, Walls[Random.Range(0, Walls.Length)]);
            //if (maze.matrix[i, j] == (uint) MatrixTypes.VERTICAL_WALL)
//              wall.transform.eulerAngles = new Vector3(0, 90, 0);
            wall.transform.parent = grouper_w.transform;
          }
        }
      }

      //floor.transform.position = new Vector3 (0, +0.01f, 0);


      float floor_size = Floor_tiles[0].GetComponentsInChildren<MeshFilter>()[0].sharedMesh.bounds.size.x;
      // FLoor
      uint h_piceces = (uint) (maze.width  * size / floor_size);
      uint v_piceces = (uint) (maze.height * size / floor_size);

      for (uint i = 0; i < v_piceces; i++) {
        for (uint j = 0; j < h_piceces; j++) {
          var i_floor = Instantiate (Floor_tiles[ (uint) Random.Range(0, Floor_tiles.Length)]);
          i_floor.transform.position = new Vector3 (j * floor_size , 0, i * floor_size);
          i_floor.transform.parent = grouper_w.transform;
        }
      }
    }


    void InstantiateAreas (List<Area> areas) {

      var grouper = Instantiate (new GameObject("areas_group"));
      grouper.transform.parent = transform;

      float size = Walls[0].GetComponentsInChildren<MeshFilter>()[0].sharedMesh.bounds.size.x;
      float v_size = Walls[0].GetComponentsInChildren<MeshFilter>()[0].sharedMesh.bounds.size.y;
      float h_size = size / 2; // half size

      areas.ForEach (delegate (Area area) {
        var element = Instantiate (collision_cube);

        float distance_x = (int) Mathf.Abs((int)area.a.x - (int)area.b.x) + 1;
        float distance_y = (int) Mathf.Abs((int)area.a.y - (int)area.b.y) + 1;

        element.transform.localScale = new Vector3(distance_x * size,
                                                   v_size,
                                                   distance_y * size);

        Vector3 new_pos;
        new_pos = new Vector3 (((area.a.x + (distance_x / 2)) * size) - h_size,
                               v_size / 2,
                               ((area.a.y + (distance_y / 2)) * size) - size);

        element.transform.position = new_pos;
        element.transform.parent = grouper.transform;
      });
    }


    // Start is called before the first frame update
    void Start() {
      Maze maze = GenerateLaberynth();
      Maze transposed_maze = maze.transpose();

      List<Area> areas = new List<Area>();
      for (uint i = 0; i < transposed_maze.height; i++) {
        for (uint j = 0; j < transposed_maze.width; j++) {
          explore_start (transposed_maze, areas, i, j);
        }
      }
      InstantiateAreas (areas);
      InstantiateLaberynth (maze);
      InstantiateProps (maze);

      GameController.CustomStart();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
