using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private GameObject playerPrefab;

    public int actualDistance = 5;

    private GameObject[,,] _cube = new GameObject[Global.CUBES_I, Global.CUBES_J, Global.CUBES_K];

    private List<GameObject> _actualCubeList = new List<GameObject>();
    //private bool[,,] _instantiateFlags = new bool[Global.CUBES_I, Global.CUBES_J, Global.CUBES_K];

    bool isLevelBuilding = true;

    private GameObject _player;

    Vector3 playerPos; 

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
                 && i < Global.CUBES_I
                 && j < Global.CUBES_J
                 && k < Global.CUBES_K);
    }


    void OptimizationLevel()
    {

        for (int i = 1; i < Global.CUBES_I - 1; i++)
            for (int j = 1; j < Global.CUBES_J - 1; j++)
                for (int k = 1; k < Global.CUBES_K - 1; k++)
                {
                    if (Global.cubes[i + 1, j, k] != 0 && Global.cubes[i - 1, j, k] != 0 &&
                        Global.cubes[i, j + 1, k] != 0 && Global.cubes[i, j - 1, k] != 0 &&
                        Global.cubes[i, j, k + 1] != 0 && Global.cubes[i, j, k - 1] != 0
                    ) Global.cubes[i, j, k] = -1;

                }

        for (int i = 0; i < Global.CUBES_I; i++)
            for (int j = 0; j < Global.CUBES_J; j++)
                for (int k = 0; k < Global.CUBES_K; k++)
                {
                    if (i == 0 || j == 0 || k == 0 || i == Global.CUBES_I - 1 || j == Global.CUBES_J - 1 || k == Global.CUBES_K - 1)
                        Global.cubes[i, j, k] = -1;
                }

        for (int i = 0; i < Global.CUBES_I; i++)
            for (int j = 0; j < Global.CUBES_J; j++)
                for (int k = 0; k < Global.CUBES_K; k++)
                {
                    if (Global.cubes[i, j, k] == -1)
                        Global.cubes[i, j, k] = 0;
                }

    }

    void ProrezKoridor(int length, Dir dir, int maxWidth)
    {

        for (int st = 0; st < length; st++)
        {

            switch (dir)
            {
                case Dir.GO_UP: if (j_cut + 2 < Global.CUBES_J) j_cut++; break;
                case Dir.GO_DOWN: if (j_cut > 1) j_cut--; break;

                case Dir.GO_FORWARD: case Dir.GO_BACK: if (k_cut + 2 < Global.CUBES_K) k_cut++; break;
                //	case GO_BACK: 		case GO_BACK2: 		if(j_cut>2		 		) j_cut--; break;
                case Dir.GO_RIGHT: if (i_cut + 2 < Global.CUBES_I) i_cut++; break;
                case Dir.GO_LEFT: if (i_cut > 2) i_cut--; break;
            }

            int width = (int)Random.Range(0.0f, maxWidth);

            for (int ii = i_cut; ii <= i_cut + width; ii++)
                for (int jj = j_cut; jj <= j_cut + width; jj++)
                    for (int kk = k_cut; kk <= k_cut + width; kk++)
                        if (correctIndex(ii, jj, kk))
                        {
                            Global.cubes[ii, jj, kk] = 0;
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
                    case Dir.GO_UP: if (j_cut + 2 < Global.CUBES_J) j_cut++; steps =  Random.Range(0, 3); break;
                    case Dir.GO_DOWN: if (j_cut > 1) j_cut--; steps =  Random.Range(0, 3); break;
                    case Dir.GO_FORWARD: if (k_cut + 2 < Global.CUBES_K) k_cut++; break;
                    case Dir.GO_BACK: if (k_cut > 2) k_cut--; break;
                    case Dir.GO_RIGHT: if (i_cut + 2 < Global.CUBES_I) i_cut++; break;
                    case Dir.GO_LEFT: if (i_cut > 2) i_cut--; break;
                }

                Global.cubes[i_cut, j_cut, k_cut] = 0;

                if (dir == Dir.GO_UP || dir == Dir.GO_DOWN)
                {
                  //  Global.cubes[i_cut, j_cut, k_cut] = 2; //water
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
                        if (rr < 50 || rr > 55) Global.cubes[ii, jj, kk] = 0;
                    }

        //  make_random_room(i_cut,j_cut,k_cut);
    }

    IEnumerator UpdateActualCubes()
    {
        Vector3 playerPos = playerPrefab.transform.position;

        int px = (int)playerPrefab.transform.position.x;
        int py = (int)playerPrefab.transform.position.y;
        int pz = (int)playerPrefab.transform.position.z;


        if (playerPrefab != null)
        {
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

    // IEnumerator UpdateActualCubes()
    // {
    //     Vector3 playerPos = playerPrefab.transform.position;

    //     foreach (GameObject actCube in _actualCubeList)
    //     {

    //         if ((int)actCube.transform.position.x < playerPos.x - actualDistance * 2
    //             || (int)actCube.transform.position.y < playerPos.y - actualDistance * 2
    //             || (int)actCube.transform.position.z < playerPos.z - actualDistance * 2
    //            || (int)actCube.transform.position.x > playerPos.x + actualDistance * 2
    //             || (int)actCube.transform.position.y > playerPos.y + actualDistance * 2
    //             || (int)actCube.transform.position.z > playerPos.z + actualDistance * 2
    //         )
    //         {
    //             _instantiateFlags[(int)actCube.transform.position.x,
    //                                 (int)actCube.transform.position.y,
    //                                 (int)actCube.transform.position.z] = false;
    //             Destroy(actCube);
    //         }
    //     }
    //     _actualCubeList.Clear();

    //     //   yield return null;

    //     if (playerPrefab != null)
    //     {
    //         for (int i = (int)playerPrefab.transform.position.x - actualDistance; i < (int)playerPrefab.transform.position.x + actualDistance; i++)
    //         {
    //             for (int j = (int)playerPrefab.transform.position.y - actualDistance; j < (int)playerPrefab.transform.position.y + actualDistance; j++)
    //             {
    //                 for (int k = (int)playerPrefab.transform.position.z - actualDistance; k < (int)playerPrefab.transform.position.z + actualDistance; k++)
    //                 {
    //                     if (correctIndex(i, j, k) && Global.cubes[i, j, k] == 1)
    //                     {
    //                         if (!_instantiateFlags[i, j, k])
    //                         {
    //                             GameObject tmpCube = (GameObject)Instantiate(cubePrefab);
    //                             tmpCube.transform.position = new Vector3(i, j, k);
    //                             _actualCubeList.Add(tmpCube);
    //                             _instantiateFlags[i, j, k] = true;
    //                         }
    //                     }

    //                 }

    //             }
    //             yield return null;
    //         }
    //     }

    // }

    Vector3 GenerateLevel()
    {
        _isLevelGenerating = true;

        Vector3 playerStartPosition; 

        for (int i = 0; i < Global.CUBES_I; i++)
            for (int j = 0; j < Global.CUBES_J; j++)
                for (int k = 0; k < Global.CUBES_K; k++)
                {
                    Global.cubes[i, j, k] = 1;
                }



        int di_cut = 0;
        int dj_cut = 0;
        i_cut = Global.CUBES_I / 2;
        j_cut = Global.CUBES_J / 2;
        k_cut = Global.CUBES_K / 2;
        Global.cubes[i_cut, j_cut, k_cut] = 0;
 
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

        return playerStartPosition;
    }



    IEnumerator BuildCube(int i, int j, int k, Vector3 pos)
    {
        _cube[i, j, k] = (GameObject)Instantiate(cubePrefab);
        _cube[i, j, k].transform.position = new Vector3(i, j, k);
        _cube[i, j, k].SetActive(true);
        yield return null;
    }


    IEnumerator BuildLevel()
    {
        isLevelBuilding = true;

        for (int i = 0; i < Global.CUBES_I; i++)
        {
            for (int j = 0; j < Global.CUBES_J; j++)
            {
                for (int k = 0; k < Global.CUBES_K; k++)
                {
                    //if (correctIndex(i, j, k))
                    // {
                    if (Global.cubes[i, j, k] == 1)
                    {
                        _cube[i, j, k] = (GameObject)Instantiate(cubePrefab);
                        _cube[i, j, k].transform.position = new Vector3(i, j, k);
                        _cube[i, j, k].SetActive(true);

                        //  GameObject tmpCube = (GameObject)Instantiate(cubePrefab, new Vector3(i, j, k), Quaternion.identity);
                        //    _actualCubeList.Add(tmpCube);
                        //_instantiateFlags[i, j, k] = true;

                        //StartCoroutine(BuildCube(i,j,k, new Vector3(i, j, k))); 


                    }
                    // }


                }

            }
            yield return null;
            isLevelBuilding = false;
           // yield break;
        }


        

        
    }


    // Start is called before the first frame update
    void Start()
    { 
        Global.cubes = new int[Global.CUBES_I, Global.CUBES_J, Global.CUBES_K];
 
        // _player = Instantiate(playerPrefab) as GameObject; 
        playerPrefab.transform.position =  GenerateLevel();  
       // playerPrefab.gameObject.GetComponent<FPSInput>().SetInputEnable(false);
        playerPrefab.SetActive(false);
        StartCoroutine(BuildLevel()); 
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
 
            if(!isLevelBuilding) {
               // playerPrefab.gameObject.GetComponent<FPSInput>().SetInputEnable(true);

                playerPrefab.SetActive(true);
                //isLevelBuilding = true;
            }
        }
 
    }
}
