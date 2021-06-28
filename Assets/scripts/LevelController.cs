using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Mirror;

public class LevelController : MonoBehaviour
{
 
    public static int CUBES_I = 100; 
    public static int CUBES_J = 100;
    public static int CUBES_K = 100;
    
    public int[,,] cubes;

    public static Vector3 mapCenter = new Vector3(CUBES_I/2, CUBES_J/2, CUBES_K/2);

    [SerializeField] private GameObject cubePrefab; 

    public int actualDistance = 5;

    private GameObject[,,] _cube = new GameObject[CUBES_I, CUBES_J, CUBES_K];

    private List<GameObject> _actualCubeList = new List<GameObject>(); 

    bool isLevelBuilding = true;

    private GameObject _player;

    Vector3 playerPos; 

    public bool generated = false;

    public bool buildProcessActive = false;

    private bool _builded = false;
    public bool Builded {
        get {return _builded;}
    }

    enum Dir
    {
        GO_LEFT,
        GO_RIGHT,
        GO_FORWARD,
        GO_BACK,
        GO_UP,
        GO_DOWN
    }

    int i_cut = 0;
    int j_cut = 0;
    int k_cut = 0;


    private bool _isLevelGenerating = false;

    float mr()
    {
        return Random.Range(0.0f, 1.0f);
    }

    bool correctIndex(int i, int j, int k)
    {
        return (i >= 0
                 && j >= 0
                 && k >= 0
                 && i < CUBES_I
                 && j < CUBES_J
                 && k < CUBES_K);
    }


    public void Clear() {

        for (int i = 0; i < CUBES_I; i++)
        {
            for (int j = 0; j < CUBES_J; j++) 
            {
                for (int k = 0; k < CUBES_K; k++)
                {
                    if(cubes[i, j, k] != 0) {
                        if(_cube[i,j,k] != null)
                            Destroy(_cube[i,j,k]);
                        cubes[i, j, k] = 0;
                    }
                }
            }
        }
 

        generated = false;
        _builded = false;
        buildProcessActive = false;
    }


    void OptimizationLevel()
    {

        for (int i = 1; i < CUBES_I - 1; i++)
            for (int j = 1; j < CUBES_J - 1; j++)
                for (int k = 1; k < CUBES_K - 1; k++)
                {
                    if (cubes[i + 1, j, k] != 0 && cubes[i - 1, j, k] != 0 &&
                        cubes[i, j + 1, k] != 0 && cubes[i, j - 1, k] != 0 &&
                        cubes[i, j, k + 1] != 0 && cubes[i, j, k - 1] != 0
                    ) cubes[i, j, k] = -1;

                }

        for (int i = 0; i < CUBES_I; i++)
            for (int j = 0; j < CUBES_J; j++)
                for (int k = 0; k < CUBES_K; k++)
                {
                    if (i == 0 || j == 0 || k == 0 || i == CUBES_I - 1 || j == CUBES_J - 1 || k == CUBES_K - 1)
                        cubes[i, j, k] = -1;
                }

        for (int i = 0; i < CUBES_I; i++)
            for (int j = 0; j < CUBES_J; j++)
                for (int k = 0; k < CUBES_K; k++)
                {
                    if (cubes[i, j, k] == -1)
                        cubes[i, j, k] = 0;
                }

    }

    void ProrezKoridor(int length, Dir dir, int maxWidth)
    {

        for (int st = 0; st < length; st++)
        {

            switch (dir)
            {
                case Dir.GO_UP: if (j_cut + 2 < CUBES_J) j_cut++; break;
                case Dir.GO_DOWN: if (j_cut > 2) j_cut--; break;

                case Dir.GO_FORWARD: /*case Dir.GO_BACK:*/ if (k_cut + 2 < CUBES_K) k_cut++; break;
                	case Dir.GO_BACK: 		/*case GO_BACK2:*/ 		if(k_cut>2		 		) k_cut--; break;
                case Dir.GO_RIGHT: if (i_cut + 2 < CUBES_I) i_cut++; break;
                case Dir.GO_LEFT: if (i_cut > 2) i_cut--; break;
            }

            int width = (int)Random.Range(0.0f, maxWidth);

            for (int ii = i_cut; ii <= i_cut + width; ii++)
                for (int jj = j_cut; jj <= j_cut + width; jj++)
                    for (int kk = k_cut; kk <= k_cut + width; kk++)
                        if (correctIndex(ii, jj, kk))
                        {
                            cubes[ii, jj, kk] = 0;
                        }
        }
    }


    void make_random_corridor(int i_start,
                             int j_start,
                             int k_start,
                             Dir startDir,
                             int commonLength)
    {
        i_cut = i_start;
        j_cut = j_start;
        k_cut = k_start;

        int max_cut = commonLength;
        int steps = 0;

        //  int kk = 0;
        Dir dir = startDir;
        do
        {


            steps = Random.Range(1, 8);
            // 	if(dir==UP || dir==DOWN) steps=1;

            for (int st = 0; st < steps; st++)
            {

                switch (dir)
                {
                    case Dir.GO_UP: if (j_cut + 2 < CUBES_J) j_cut++; steps =  Random.Range(0, 3); break;
                    case Dir.GO_DOWN: if (j_cut > 1) j_cut--; steps =  Random.Range(0, 3); break;
                    case Dir.GO_FORWARD: if (k_cut + 2 < CUBES_K) k_cut++; break;
                    case Dir.GO_BACK: if (k_cut > 2) k_cut--; break;
                    case Dir.GO_RIGHT: if (i_cut + 2 < CUBES_I) i_cut++; break;
                    case Dir.GO_LEFT: if (i_cut > 2) i_cut--; break;
                }

                cubes[i_cut, j_cut, k_cut] = 0;

                if (dir == Dir.GO_UP || dir == Dir.GO_DOWN)
                {
                  //  cubes[i_cut, j_cut, k_cut] = 2; //water
                    //   cubes[i_cut][j_cut][k_cut].radius = 0.33f;
                    //   cubes[i_cut][j_cut][k_cut].x=i_cut*2-0.7f;
                    //   cubes[i_cut][j_cut][k_cut].y=j_cut*2-0.7f;
                    //   cubes[i_cut][j_cut][k_cut].z=k_cut*2+0.5f;
                }

            }


            max_cut -= steps;
            dir = (Dir)Random.Range(0.0f, (int)Dir.GO_DOWN);

        } while (max_cut > 0);



        int roomHalfSizeX = Random.Range(2, 10);
        int roomHalfSizeZ = Random.Range(2, 10);
        int roomHalfSizeY = Random.Range(0, 8);

        for (int ii = i_cut - roomHalfSizeX; ii <= i_cut + roomHalfSizeX; ii++)
            for (int jj = j_cut - roomHalfSizeY; jj <= j_cut + roomHalfSizeY; jj++)
                for (int kk = k_cut; kk <= k_cut + roomHalfSizeZ; kk++)
                    if (correctIndex(ii, jj, kk))
                    {
                        int rr = Random.Range(0, 100);
                        if (rr < 50 || rr > 55) cubes[ii, jj, kk] = 0;
                    }
 
    }

    IEnumerator UpdateActualCubes()
    {
        if (_player != null && generated)
        {        
            Vector3 playerPos = _player.transform.position;

            int px = (int)_player.transform.position.x;
            int py = (int)_player.transform.position.y;
            int pz = (int)_player.transform.position.z;

 
            for (int i = (int)px - actualDistance * 2; i < (int)px + actualDistance * 2; i++)
            {
                for (int j = (int)py - actualDistance * 2; j < (int)py + actualDistance * 2; j++)
                {
                    for (int k = (int)pz - actualDistance * 2; k < (int)pz + actualDistance * 2; k++)
                    {
                        if (correctIndex(i, j, k) && _cube[i, j, k] != null)
                        {

                            if (Mathf.Abs(i - px) < actualDistance
                                && Mathf.Abs(j - py) < actualDistance
                                && Mathf.Abs(k - pz) < actualDistance)
                                _cube[i, j, k].SetActive(true);
                            else
                                _cube[i, j, k].SetActive(false);

                        }

                    }

                }
               
            }
            yield return null;



        }
    }

   
    public Vector3 GenerateLevel(int seed)
    {

        if(seed > 0)
            Random.seed = seed;

        _isLevelGenerating = true;

        Vector3 playerStartPosition; 

        for (int i = 0; i < CUBES_I; i++)
        {
            for (int j = 0; j < CUBES_J; j++) 
            {
                for (int k = 0; k < CUBES_K; k++)
                {
                    cubes[i, j, k] = 1;
                }
            }
        }


        int di_cut = 0;
        int dj_cut = 0;
        i_cut = CUBES_I / 2;
        j_cut = CUBES_J / 2;
        k_cut = CUBES_K / 2;
        cubes[i_cut, j_cut, k_cut] = 0;
 
        playerStartPosition = new Vector3(i_cut, j_cut, k_cut);
 
        int max_cut = 100;
        int temp_max_cut = max_cut;
        int steps = 0;
         
        int fromRoomToRoom = 16 ;

        do
        {
            Dir dir = (Dir)Random.Range(0.0f, (int)Dir.GO_DOWN);
            steps = Random.Range(3, 12);
            if (dir == Dir.GO_UP || dir == Dir.GO_DOWN) steps = (int)Random.Range(0.0f, 4.0f);

            ProrezKoridor(steps, dir, 4);

            fromRoomToRoom -= steps;
            if (fromRoomToRoom < 0)
            {
                fromRoomToRoom = (int)Random.Range(0.0f, temp_max_cut / 4);
                make_random_corridor(i_cut, j_cut, k_cut, (Dir)Random.Range(0.0f, (int)Dir.GO_DOWN), 100);
            }


            // door_door -= steps;
            // if (door_door < 0)
            // {
            //     door_door = (int)Random.Range(0.0f, temp_max_cut / 3);
            //     ProrezKoridor(4, dir, 1);
            // }

            max_cut -= steps;


        } while (max_cut > 0);


        OptimizationLevel();

        _isLevelGenerating = false;


        generated = true;

        return playerStartPosition;
    }



    IEnumerator BuildCube(int i, int j, int k, Vector3 pos)
    {
        _cube[i, j, k] = (GameObject)Instantiate(cubePrefab);
        _cube[i, j, k].transform.position = new Vector3(i, j, k);
        _cube[i, j, k].SetActive(true);
        yield return null;
    }



    public void Cute(Vector3 pos)
    {   int i = (int)pos.x;
        int j = (int)pos.y;
        int k = (int)pos.z; 
        cubes[i, j, k] = 0; 
    }

    public void Insert(Vector3 pos)
    {   int i = (int)pos.x;
        int j = (int)pos.y;
        int k = (int)pos.z; 
        cubes[i, j, k] = 1; 

        Debug.Log("Insert" + cubes[i, j, k]);
    }

    IEnumerator BuildLevel()
    {   Debug.Log("BuildLevel");

        buildProcessActive = true; 
        isLevelBuilding = true;
        _builded = false;

        int iterator = 0;

        for (int i = 0; i < CUBES_I; i++)
        {
            for (int j = 0; j < CUBES_J; j++)
            {
                for (int k = 0; k < CUBES_K; k++)
                { 
                    if (cubes[i, j, k] == 1)
                    {
                        Debug.Log("BuildL " + i + j + k);

                        _cube[i, j, k] = (GameObject)Instantiate(cubePrefab);
                        _cube[i, j, k].transform.position = new Vector3(i, j, k);
                        _cube[i, j, k].SetActive(true); 

                    } 
                    iterator++;

                    if(iterator >= CUBES_I * CUBES_J * CUBES_K)
                        _builded = true;
                }

            }
            yield return null;
            isLevelBuilding = false;
           // yield break;
        }


        

        
    }

    void Start() {
         cubes = new int[CUBES_I, CUBES_J, CUBES_K]; 
    } 


    public void Build() {
        StartCoroutine(BuildLevel()); 
    }



    public List<Vector3> GetCubesIJKs() {
        List<Vector3> list = new List<Vector3>();
        
        for (int i = 0; i < CUBES_I; i++)
        {
            for (int j = 0; j < CUBES_J; j++)
            {
                for (int k = 0; k < CUBES_K; k++)
                {
                    if(cubes[i, j, k] == 1)
                        list.Add(new Vector3(i,j,k));
                }
            }
        }
        return list;
    }

    public void ImportLevel(List<Vector3> list) {

        for (int i = 0; i < CUBES_I; i++)
            for (int j = 0; j < CUBES_J; j++)
                for (int k = 0; k < CUBES_K; k++)
                    cubes[i, j, k] = 0; 

        foreach (Vector3 itemIJK in list) {
            cubes[(int)itemIJK.x, (int)itemIJK.y, (int)itemIJK.z] = 1;
        } 
    }


    float _msek = 0;
    // Update is called once per frame
    void Update()
    {
        _msek += Time.deltaTime;
        if (_msek > 0.5f)
        { 
            _msek = 0;
            StartCoroutine(UpdateActualCubes()); 
        }


        if(Input.GetMouseButtonDown(0)) {
           
        }
 
    }
}
