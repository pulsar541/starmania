using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LevelController : NetworkBehaviour
{ 
    public static LevelController control; 

    public delegate void BuilderHandler(Vector3[] teamPositions);
    public event BuilderHandler Notify;

    public static class CubeType
    {
        public const int VOID_OPT = -1;
        public const int VOID = 0;
        public const int WALL = 1;
        public const int PLATFORM = 2;


        public const int VOID_SMOOTH = 3;
        public const int BEVEl_X = 4;
        public const int BEVEl_XI = 5;
        public const int BEVEl_Z = 6;
        public const int BEVEl_ZI = 7;

        public const int SPIKES = 8;
        public const int EXIT = 100;

        public const int OUT = 1000;
    };

    public static int CUBES_I = 100;
    public static int CUBES_J = 100;
    public static int CUBES_K = 100;

    public int[,,] cubes;

    private List<Vector3> lampPositions = new List<Vector3>();

    public static Vector3 mapCenter = new Vector3(CUBES_I / 2, CUBES_J / 2, CUBES_K / 2);

    [SerializeField] private GameObject cubeWallPrefab = null;
    [SerializeField] private GameObject cubeSinglePrefab = null;
    [SerializeField] private GameObject platformPrefab = null; 
    [SerializeField] private GameObject pointLightPrefab = null; 
    [SerializeField] private GameObject checkPointPrefab = null;
    [SerializeField] private GameObject spikesPrefab = null; 
    [SerializeField] private GameObject mapCubePrefab = null;
    [SerializeField] private GameObject exitPrefab = null;

    LightManager lightManager;

    public int actualDistance = 3;

    public GameObject[,,] _cubeGO = new GameObject[CUBES_I, CUBES_J, CUBES_K];
    private GameObject[,,] _lampGO = new GameObject[CUBES_I, CUBES_J, CUBES_K];

    public GameObject[,,] _mapCubesGO = new GameObject[CUBES_I, CUBES_J, CUBES_K];


    public bool[,,] enemyTrigger = new bool[CUBES_I, CUBES_J, CUBES_K];

    public bool[,,] hasCable = new bool[CUBES_I, CUBES_J, CUBES_K];

    public bool[,,] explore = new bool[CUBES_I, CUBES_J, CUBES_K];

    public List<Vector3> checkPoints = new List<Vector3>();
    public List<GameObject> checkPointsGO = new List<GameObject>();


    private List<GameObject> _actualCubeList = new List<GameObject>();
    private GameObject _localPlayer;

    Vector3 playerPos;

    public static bool generated = false;

    public bool buildProcessActive = false;

    private bool _builded = false;

      
    private bool _mapOfLevelBuilded = false;

    private Vector3[] teamPositions = new Vector3[3];

    public Vector3 exitPosition = new Vector3(10000,10000,10000);

    public bool Builded
    {
        get { return _builded; }
    }

    enum OrthoDir
    {
        GO_LEFT,
        GO_RIGHT,
        GO_FORWARD,
        GO_BACK,
        GO_UP,
        GO_DOWN
    }

    Vector3 ant;

    Vector3 cutDirection = new Vector3(0, -1, 0);

    float mr()
    {
        return Random.Range(0.0f, 1.0f);
    }


    bool isCorrectCluster(int i, int j, int k)
    {
        return (i >= 0
                 && j >= 0
                 && k >= 0
                 && i < CUBES_I
                 && j < CUBES_J
                 && k < CUBES_K);
    }
    public bool isCorrectCluster(Vector3 clusterPos)
    {
        return (Mathf.Round(clusterPos.x) >= 0
                 && Mathf.Round(clusterPos.y) >= 0
                 && Mathf.Round(clusterPos.z) >= 0
                 && Mathf.Round(clusterPos.x) < CUBES_I
                 && Mathf.Round(clusterPos.y) < CUBES_J
                 && Mathf.Round(clusterPos.z) < CUBES_K);
    }


    float squareDist(Vector3 a, Vector3 b)
    {
        return (Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2) + Mathf.Pow(a.z - b.z, 2));
    }

    public bool isType(int i, int j, int k, int cubeType)
    {
        return isCorrectCluster(i, j, k) && cubes[i, j, k] == cubeType;
    }

    public bool isType(Vector3 pos, int cubeType)
    {
        return isCorrectCluster(
            (int)Mathf.Round(pos.x),
            (int)Mathf.Round(pos.y),
            (int)Mathf.Round(pos.z))

        && cubes[(int)Mathf.Round(pos.x),
            (int)Mathf.Round(pos.y),
            (int)Mathf.Round(pos.z)] == cubeType;
    }


    bool isSolid(int i, int j, int k)
    {
        return isCorrectCluster(i, j, k) && (cubes[i, j, k] == CubeType.OUT || cubes[i, j, k] == CubeType.WALL);
    }

    public bool HasCable(Vector3 pos)
    {
        return isCorrectCluster(
            (int)Mathf.Round(pos.x),
            (int)Mathf.Round(pos.y),
            (int)Mathf.Round(pos.z))
            && hasCable[(int)Mathf.Round(pos.x),
                         (int)Mathf.Round(pos.y),
                         (int)Mathf.Round(pos.z)] == true;
    }

    public int NeihtboursCount(int i, int j, int k, int cubeType, int radius = 1)
    {
        int count = 0;
        for (int ii = i - radius; ii <= i + radius; ii++)
            for (int jj = j - radius; jj <= j + radius; jj++)
                for (int kk = k - radius; kk <= k + radius; kk++)
                {
                    if (ii == i && jj == j && kk == k)
                        continue;

                    if (isCorrectCluster(ii, jj, kk) && cubes[ii, jj, kk] == (int)cubeType)
                    {
                        count++;
                    }
                }
        return count;
    }

    public int WallsHorizontAroundCount(Vector3 pos)
    {
        int count = 0;
        int i = (int)Mathf.Round(pos.x);
        int j = (int)Mathf.Round(pos.y);
        int k = (int)Mathf.Round(pos.z);

        if (isType(i - 1, j, k, CubeType.WALL)
            || isType(i, j, k - 1, CubeType.WALL)
            || isType(i + 1, j, k, CubeType.WALL)
            || isType(i, j, k + 1, CubeType.WALL))
        {
            count++;
        }

        return count;
    }



    int LampsCount(int i, int j, int k, int radius = 1)
    {
        int count = 0;
        for (int ii = i - radius; ii <= i + radius; ii++)
            for (int jj = j - radius; jj <= j + radius; jj++)
                for (int kk = k - radius; kk <= k + radius; kk++)
                {
                    if (ii == jj && jj == kk)
                        continue;

                    if (isCorrectCluster(ii, jj, kk) && lampPositions.Contains(new Vector3(ii, jj, kk)))
                    {
                        count++;
                    }
                }
        return count;
    }

    public void Clear()
    {

        for (int i = 0; i < CUBES_I; i++)
        {
            for (int j = 0; j < CUBES_J; j++)
            {
                for (int k = 0; k < CUBES_K; k++)
                {
                    cubes[i, j, k] = CubeType.WALL;

                    if (_cubeGO[i, j, k] != null)
                        Destroy(_cubeGO[i, j, k]); 

                    if (_lampGO[i, j, k] != null)
                        Destroy(_lampGO[i, j, k]);

                    if(_mapCubesGO[i, j, k] != null)
                        Destroy(_mapCubesGO[i, j, k]); 

                    explore[i, j, k] = false;
                    hasCable[i, j, k] = false;                   
                    
                }
            }
        }


        foreach (GameObject checkpointGo in checkPointsGO)
            Destroy(checkpointGo);

        checkPointsGO.Clear();
 
        generated = false;
        _builded = false;
        _mapOfLevelBuilded = false;
        buildProcessActive = false;
    }


    void OptimizationLevel()
    {

        for (int i = 1; i < CUBES_I - 1; i++)
            for (int j = 1; j < CUBES_J - 1; j++)
                for (int k = 1; k < CUBES_K - 1; k++)
                {
                    if (//isSolid(i, j, k) &&
                        isSolid(i + 1, j, k) && isSolid(i - 1, j, k) &&
                        isSolid(i, j + 1, k) && isSolid(i, j - 1, k) &&
                        isSolid(i, j, k + 1) && isSolid(i, j, k - 1)
                    ) cubes[i, j, k] = (int)CubeType.OUT;

                }

        for (int i = 0; i < CUBES_I; i++)
            for (int j = 0; j < CUBES_J; j++)
                for (int k = 0; k < CUBES_K; k++)
                {
                    if (i == 0 || j == 0 || k == 0 || i == CUBES_I - 1 || j == CUBES_J - 1 || k == CUBES_K - 1)
                        cubes[i, j, k] = (int)CubeType.OUT;
                }

        // for (int i = 0; i < CUBES_I; i++)
        //     for (int j = 0; j < CUBES_J; j++)
        //         for (int k = 0; k < CUBES_K; k++)
        //         {
        //             if (cubes[i, j, k] == -1)
        //                 cubes[i, j, k] = 0;
        //         }

    }


    void GenCheckpoints(int count)
    {
        checkPoints.Clear();
        List<Vector3> voidOverWallList = new List<Vector3>(); 
        
        for (int i = 0; i < CUBES_I; i++)
            for (int j = 1; j < CUBES_J - 1; j++)
                for (int k = 0; k < CUBES_K; k++)
                {
                    if (cubes[i, j, k] == (int)CubeType.VOID)
                    {
                        if (cubes[i, j - 1, k] == (int)CubeType.WALL)
                        {
                            voidOverWallList.Add(new Vector3(i, j, k));
                        } 
                    }
                }

        Vector3 maxFarCheckpointPosition = LevelController.mapCenter; 
        float maxQuadist = 0;
        for (int c = 0; c < count  ; c++)
        {
            int voidsCount = voidOverWallList.Count;
            if (voidsCount > 0)
            {   int n = Random.Range(0, voidsCount);

                 
                checkPoints.Add(voidOverWallList[n] + new Vector3(0, -0.5f, 0)); 
   
                float tmpQDist = squareDist( LevelController.mapCenter, voidOverWallList[n]);

                if( tmpQDist > maxQuadist) {
                    maxQuadist = tmpQDist;
                    maxFarCheckpointPosition = voidOverWallList[n];
                }

                voidOverWallList.RemoveAt(n);
            } 
        }


        exitPosition = maxFarCheckpointPosition;
        cubes[(int)exitPosition.x, (int)exitPosition.y, (int)exitPosition.z] = CubeType.EXIT;

      
        // place exit
        //cubes[52,50,52] = CubeType.EXIT;
        //exitPosition = new Vector3(52,50,52);
          
 
    }


    void GenSpikes()
    { 
        for (int i = 1; i < CUBES_I - 1; i++)
            for (int j = 1; j < CUBES_J - 1; j++)
                for (int k = 1; k < CUBES_K - 1; k++)
                {
                    if (cubes[i, j, k] == (int)CubeType.WALL
                        && 
                        (cubes[i-1, j, k] == (int)CubeType.VOID 
                        || cubes[i+1, j, k] == (int)CubeType.VOID 
                        || cubes[i, j-1, k] == (int)CubeType.VOID
                        || cubes[i, j+1, k] == (int)CubeType.VOID 
                        || cubes[i, j, k-1] == (int)CubeType.VOID 
                        || cubes[i, j, k+1] == (int)CubeType.VOID 
                        )
                    )
                    {
                         
                        //voidOverWallList.Add(new Vector3(i, j, k));
                        if(Random.Range(0,100) < 7)
                            cubes[i, j, k] = (int)CubeType.SPIKES;
                        
                    }
                } 
    }
 
    void MakeLamps(int beetweenDist)
    {
        //lampPositions.Clear();
        //lampPositions.Add(LevelController.mapCenter);

        for (int i = 0; i < CUBES_I; i++)
            for (int j = 0; j < CUBES_J - 1; j++)
                for (int k = 0; k < CUBES_K; k++)
                {
                    if (cubes[i, j, k] == (int)CubeType.VOID  /*&& cubes[i, j+1, k] == (int)CubeType.WALL */
                    // && NeihtboursCount(i,j,k, CubeType.WALL) >= 3 
                    //  && LampsCount(i,j,k, 5) < 1
                    )
                    {
                        // lampPositions.Add(new Vector3(i,j,k));  
                        Vector3 lightPos = new Vector3(i, j, k);
                        lightManager.TryInsertLight(lightPos, LightManager.GetLampColorByPosition(lightPos), beetweenDist);
                    }
                }
    }

    // void MakeBridge(int i_cut, int j_cut, int k_cut, int length, bool direction)
    // {

    //     if (direction)
    //     {
    //         int ii = i_cut;
    //         for (int st = 0; st < length; st++)
    //             if (isCorrectCluster(ii + st, j_cut, k_cut))
    //             {
    //                 if (cubes[ii + st, j_cut, k_cut] == 1 && cubes[ii + st, j_cut, k_cut] == CubeType.OUT)
    //                     break;

    //                 cubes[ii + st, j_cut, k_cut] = CubeType.PLATFORM;
    //             }
    //         for (int st = 0; st < length; st++)
    //             if (isCorrectCluster(ii - st, j_cut, k_cut))
    //             {
    //                 if (cubes[ii - st, j_cut, k_cut] == 1 && cubes[ii - st, j_cut, k_cut] == CubeType.OUT)
    //                     break;

    //                 cubes[ii - st, j_cut, k_cut] = CubeType.PLATFORM;
    //             }
    //     }
    //     else
    //     {
    //         int kk = (int)k_cut;
    //         for (int st = 0; st < length; st++)
    //             if (isCorrectCluster(i_cut, j_cut, kk + st))
    //             {
    //                 if (cubes[i_cut, j_cut, kk + st] == 1 && cubes[i_cut, j_cut, kk + st] == 1)
    //                     break;
    //                 cubes[i_cut, j_cut, kk + st] = CubeType.PLATFORM;
    //             }
    //         for (int st = 0; st < length; st++)
    //             if (isCorrectCluster(i_cut, j_cut, kk - st))
    //             {
    //                 if (cubes[i_cut, j_cut, kk - st] == 1 && cubes[i_cut, j_cut, kk - st] == CubeType.OUT)
    //                     break;
    //                 cubes[i_cut, j_cut, kk - st] = CubeType.PLATFORM;
    //             }


    //     }

    // }




    Vector3 ProrezKoridor(Vector3 start, int length, Vector3 dir, int w, int h, bool withBridges, bool isSmoothVoid = false)
    {
        int width = w;
        int height = h;

        float i_cut = start.x;
        float j_cut = start.y;
        float k_cut = start.z;

        if (start == LevelController.mapCenter && dir.y != 0)
            return start;


        bool insertSpikes = true; //dir.y < 0 && Random.Range(0, 100) > 50 && (i_cut != mapCenter.x || k_cut != mapCenter.z);



        for (int st = 0; st < length; st++)
        {

            i_cut += dir.x;
            j_cut += dir.y;
            k_cut += dir.z;


            //if(randomWidth)
            //    width = Random.Range(1,w);
          
            for (int jj = (int)j_cut; jj < (int)j_cut + height; jj++)
            {
                 for (int ii = (int)i_cut; ii < (int)i_cut + width; ii++)
                {
                    for (int kk = (int)k_cut; kk < (int)k_cut + width; kk++)
                    {
                        if (isCorrectCluster(ii, jj, kk))
                        {
                            cubes[ii, jj, kk] = isSmoothVoid ? (int)CubeType.VOID_SMOOTH : (int)CubeType.VOID;

                            //if (st == length - 1 && insertSpikes)
                           // {
                                // if (isCorrectCluster(ii, jj - 1, kk))
                                // {
                                //     cubes[ii, jj - 1, kk] = CubeType.SPIKES;
                                // }
                           // }

                            if (withBridges)
                            {
                                  
                                Vector3 lightPos = new Vector3(ii, jj, kk);
                                if (isCorrectCluster(lightPos))
                                {
                                    lightManager.TryInsertLight(lightPos, LightManager.GetLampColorByPosition(lightPos), 5);
                                }

                            }

                            if (Random.Range(0, 1000) < 10)
                            {
                                enemyTrigger[ii, jj, kk] = true;
                            }

                        }
                    }
                } 


                if (withBridges)
                {
                  //  if (Random.Range(0, 100) < 50)
                   // {
                        int iii = (int)i_cut + width/2; 
                        int kkk = (int)k_cut + width/2;
                        int cnt = 0;
                        while( !isSolid(iii, jj, kkk) && cnt < 8)
                        {   if(isCorrectCluster(iii, jj, kkk)) {

                                if (Random.Range(0, 100) < 50)
                                {
                                    cubes[iii, jj, kkk] = (int)CubeType.PLATFORM;
                                }

                                iii += Random.Range(-1, 1);
                                kkk += Random.Range(-1, 1);
                                
                            }
                            cnt ++;
                        }

                    // cubes[ii, jj, kk] = (int)CubeType.PLATFORM;

                        
                 //   }
                }


            }
 

            // if (withBridges)
            // {   int ii_cut = Mathf.RoundToInt(i_cut);
            //     int jj_cut = Mathf.RoundToInt(j_cut);
            //     int kk_cut = Mathf.RoundToInt(k_cut);

            //     for (int jj = jj_cut - height / 2; jj <= jj_cut + height/2; jj++)
            //     {
            //         int iii = Random.Range(ii_cut - width / 2, ii_cut + width / 2);
            //         int kkk = Random.Range(kk_cut - width / 2, kk_cut + width / 2); 
            //         MakeBridge(iii, jj, kkk,  1 /*Random.Range(2, width*2/3)*/,   (jj%2) == 0);
            //     }
            // }




            // bool placeLampCondition = st == length / 2  && isType((int)i_cut, (int)j_cut, (int)k_cut, CubeType.VOID)  &&  Random.Range(0.0f, 1.0f) > 0.65f;

            // if(placeLampCondition && LampsCount((int)i_cut, (int)j_cut, (int)k_cut, length/2 ) == 0) 
            // {   
            //         //int tj = j_cut;
            //         // if(dir != Dir.GO_UP && dir != Dir.GO_DOWN) {
            //         //     while(correctIndex(i_cut, tj, k_cut)  && cubes[i_cut, tj, k_cut] == (int)CubeType.VOID){
            //         //         tj++;
            //         //     }
            //         //     tj--;
            //         // } 
            //         if(!lampPositions.Contains(new Vector3(i_cut,j_cut,k_cut)) )
            //              lampPositions.Add(new Vector3(i_cut,j_cut,k_cut)); 


            // } 

            // if (st == length / 3 || st == 2 * length / 3)
            // {
            //     Vector3 lightPos = new Vector3(i_cut, j_cut, k_cut);
            //     if (isCorrectCluster(lightPos))
            //     {
            //         lightManager.TryInsertLight(lightPos, LightManager.GetLampColorByPosition(lightPos), 3);
            //     }
            // }

        }

        return new Vector3(i_cut, j_cut, k_cut);

    }
 


    private Vector3 GetDirMovementByOrthoDir(OrthoDir dir)
    {
        switch (dir)
        {
            case OrthoDir.GO_UP:
                return new Vector3(0, 1, 0);
            case OrthoDir.GO_DOWN:
                return new Vector3(0, -1, 0);
            case OrthoDir.GO_FORWARD:
                return new Vector3(0, 0, 1);
            case OrthoDir.GO_BACK:
                return new Vector3(0, 0, -1);
            case OrthoDir.GO_RIGHT:
                return new Vector3(1, 0, 0);
            case OrthoDir.GO_LEFT:
                return new Vector3(-1, 0, 0);
        }

        return new Vector3(0, 0, 0);
    }
 
 

    private void GenOrthoTunnels(Vector3 startPos, int maxWidth, int maxHeight, int tunnelsCount, int tunnelMinLength, int tunnelMaxLength, out Vector3 someMiddlePos)
    {

        int max_cut = tunnelsCount;
        int temp_max_cut = max_cut;
        someMiddlePos = startPos;
        Vector3 ant = new Vector3();
        ant = startPos;

        do
        {
            OrthoDir orthoDir = (OrthoDir)Random.Range(-1, (int)OrthoDir.GO_DOWN + 1);
            Vector3 dirM = GetDirMovementByOrthoDir(orthoDir);

            int steps = Random.Range(tunnelMinLength, tunnelMaxLength);
            int width = Random.Range(1, maxWidth);
            int height = Random.Range(1, maxHeight);
            bool withPlatforms = false; //Random.Range(0,100)<50

            if (max_cut < temp_max_cut && (max_cut % 50) == 0)
            {
                width *= 4;
                height *= 4;
                steps = 1;
                withPlatforms = true;
            }


            ant = ProrezKoridor(ant, steps, dirM, width, height, withPlatforms, false);

            max_cut--;

            if (max_cut == tunnelsCount / 2)
                someMiddlePos = ant;



            if (!isCorrectCluster(ant))
                ant = someMiddlePos;


        } while (max_cut > 0);

    }



    int ClustersCountByType(int cubeType)
    {
        int count = 0;
        for (int i = 0; i < CUBES_I; i++)
            for (int j = 0; j < CUBES_J; j++)
                for (int k = 0; k < CUBES_K; k++)
                    if (cubes[i, j, k] == cubeType)
                        count++;
        return count;
    }

    void SetCubeType(Vector3 pos, int cubeType)
    {
        int i = Mathf.RoundToInt(pos.x);
        int j = Mathf.RoundToInt(pos.y);
        int k = Mathf.RoundToInt(pos.z);

        if(isCorrectCluster(i,j,k)) {
            cubes[i,j,k] = cubeType;
        }
    }

    public void GenerateLevel(int seed)
    {
        Clear();
        if (seed > 0)
            Random.InitState(seed);
 

        for (int i = 0; i < CUBES_I; i++)
            for (int j = 0; j < CUBES_J; j++)
                for (int k = 0; k < CUBES_K; k++)
                    cubes[i, j, k] = (int)CubeType.WALL;

        for (int i = (int)mapCenter.x - 2; i <= (int)mapCenter.x + 2; i++)
            for (int j = (int)mapCenter.y; j <= (int)mapCenter.y; j++)
                for (int k = (int)mapCenter.z - 2; k <= (int)mapCenter.z + 2; k++)
                    cubes[i, j, k] = CubeType.VOID;

        ant = LevelController.mapCenter; 

        Vector3 somePos = new Vector3(); 

       // lightManager.TryInsertLight(LevelController.mapCenter, Color.white, 5);

        GenOrthoTunnels(mapCenter, 3, 3, 200, 3, 7, out somePos);
        teamPositions[0] = mapCenter;
        GenOrthoTunnels(somePos, 4, 4, 200, 4, 7, out somePos);
        teamPositions[1] = mapCenter;
 
       
        GenSpikes();
        GenCheckpoints(50); 
 
 
        SetCubeType(teamPositions[0] + new Vector3(0,-1,0) , (int) CubeType.WALL);
        SetCubeType(teamPositions[0] + new Vector3(1,0,0) , (int) CubeType.VOID);
        SetCubeType(teamPositions[0] + new Vector3(0,1,0) , (int) CubeType.VOID);
        SetCubeType(teamPositions[0] + new Vector3(0,0,1) , (int) CubeType.VOID);


        SetCubeType(teamPositions[1] + new Vector3(0,-1,0) , (int) CubeType.WALL);
        SetCubeType(teamPositions[1] + new Vector3(1,0,0) , (int) CubeType.VOID);
        SetCubeType(teamPositions[1] + new Vector3(0,1,0) , (int) CubeType.VOID);
        SetCubeType(teamPositions[1] + new Vector3(0,0,1) , (int) CubeType.VOID);
               

        OptimizationLevel();
        MakeLamps(10);

        generated = true; 
    }



    IEnumerator BuildCube(int i, int j, int k, Vector3 pos)
    {
        _cubeGO[i, j, k] = (GameObject)Instantiate(cubeWallPrefab);
        _cubeGO[i, j, k].transform.position = new Vector3(i, j, k);
        _cubeGO[i, j, k].SetActive(true);
        yield return null;
    }



    public void Cute(Vector3 pos)
    {
        int i = (int)pos.x;
        int j = (int)pos.y;
        int k = (int)pos.z;
        cubes[i, j, k] = 0;
    }

    public void Insert(Vector3 pos)
    {
        int i = (int)pos.x;
        int j = (int)pos.y;
        int k = (int)pos.z;
        cubes[i, j, k] = 1;

        Debug.Log("Insert" + cubes[i, j, k]);
    }

    IEnumerator BuildLevel()
    {
        Debug.Log("BuildLevel");

        buildProcessActive = true;
        _builded = false;

        int iterator = 0;

       
    
        for (int i = 0; i < CUBES_I; i++)
        {
            for (int j = 0; j < CUBES_J; j++)
            {
                for (int k = 0; k < CUBES_K; k++)
                { 


                    switch (cubes[i, j, k] )
                    {
                        case (int)CubeType.WALL: 
                            if (i == mapCenter.x && j == mapCenter.y - 1 && k == mapCenter.z) 
                                _cubeGO[i, j, k] = (GameObject)Instantiate(cubeSinglePrefab); 
                            else 
                                _cubeGO[i, j, k] = (GameObject)Instantiate(cubeWallPrefab); 
                            break;
                        case (int)CubeType.PLATFORM:     
                            _cubeGO[i, j, k] = (GameObject)Instantiate(platformPrefab);                        
                            break;
                        case (int)CubeType.SPIKES:  
                            _cubeGO[i, j, k] = (GameObject)Instantiate(spikesPrefab);  
                            break;
                        case (int)CubeType.EXIT:  
                            _cubeGO[i, j, k] = (GameObject)Instantiate(exitPrefab);
                            break;
 
                    }
  
                    if (_cubeGO[i, j, k])
                    {
                        _cubeGO[i, j, k].transform.position = new Vector3(i, j, k); 
                    }

                    if (_cubeGO[i, j, k])
                        _cubeGO[i, j, k].SetActive(false);


                    iterator++;

                    if (iterator >= CUBES_I * CUBES_J * CUBES_K)
                    {
                        _builded = true;
                         Notify?.Invoke(teamPositions);
                    }
                }

            }
            yield return null;
            // yield break;
        }


        foreach (Vector3 lampPos in lampPositions)
        {
            _lampGO[(int)lampPos.x, (int)lampPos.y, (int)lampPos.z] = (GameObject)Instantiate(pointLightPrefab);
            _lampGO[(int)lampPos.x, (int)lampPos.y, (int)lampPos.z].transform.position = lampPos;
            Color lightColor = LightManager.GetLampColorByPosition(lampPos);

            if (lampPos == LevelController.mapCenter)
                lightColor = new Color(1, 1, 1);

            _lampGO[(int)lampPos.x, (int)lampPos.y, (int)lampPos.z].GetComponent<Light>().color = lightColor;


            if (_lampGO[(int)lampPos.x, (int)lampPos.y, (int)lampPos.z])
                _lampGO[(int)lampPos.x, (int)lampPos.y, (int)lampPos.z].SetActive(false);

        }

        foreach (Vector3 checkpointPos in checkPoints)
        {
            checkPointsGO.Add((GameObject)Instantiate(checkPointPrefab, checkpointPos, Quaternion.identity));
        }

    }
 
    IEnumerator BuildMapOfLevelCor()
    {
        Debug.Log("BuildMapOfLevel"); 
        _mapOfLevelBuilded = false; 
        int iterator = 0;  
        for (int i = 0; i < CUBES_I; i++)
        {
            for (int j = 0; j < CUBES_J; j++)
            {
                for (int k = 0; k < CUBES_K; k++)
                { 
                    if (cubes[i, j, k] == (int)CubeType.VOID)
                    {  
                      _mapCubesGO[i, j, k] = (GameObject)Instantiate(mapCubePrefab); 
                      _mapCubesGO[i, j, k].transform.position = 0.01f * (new Vector3(i, j, k)) + new Vector3(0,1000,0);
                      _mapCubesGO[i, j, k].SetActive(false);
                    }
   
                    iterator++; 
                    if (iterator >= CUBES_I * CUBES_J * CUBES_K)
                        _mapOfLevelBuilded = true;
                }

            }
            yield return null; 
        }  

    }


    void Awake()
    {
        control = this;
        lightManager = GameObject.Find("LightManager").GetComponent<LightManager>();
        DontDestroyOnLoad(transform.gameObject);
    }

    void Start()
    {
        cubes = new int[CUBES_I, CUBES_J, CUBES_K];
        // Set the fog color to be blue
        RenderSettings.fogColor = Color.black;

        // And enable fog
        RenderSettings.fog = true;

    }


    public void Build()
    {
        StartCoroutine(BuildLevel()); 
        StartCoroutine(BuildMapOfLevelCor());
    }
 
    public List<Vector3> GetCubesIJKs()
    {
        List<Vector3> list = new List<Vector3>();

        for (int i = 0; i < CUBES_I; i++)
        {
            for (int j = 0; j < CUBES_J; j++)
            {
                for (int k = 0; k < CUBES_K; k++)
                {
                    if (cubes[i, j, k] == 1)
                        list.Add(new Vector3(i, j, k));
                }
            }
        }
        return list;
    }

    public void ImportLevel(List<Vector3> list)
    {

        for (int i = 0; i < CUBES_I; i++)
            for (int j = 0; j < CUBES_J; j++)
                for (int k = 0; k < CUBES_K; k++)
                    cubes[i, j, k] = 0;

        foreach (Vector3 itemIJK in list)
        {
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
            // StartCoroutine(UpdateActualCubes()); 
            // if(Mathf.Abs(Input.GetAxis("Horizontal")) > 0 )
            {
                //StartCoroutine(RealtimeUpdateActualCubes());
                StartCoroutine(UpdateActualCubes());
                StartCoroutine(UpdateActualExplore());                
            }

        }

        if (Mathf.Abs(Input.mouseScrollDelta.x) > 0)
            StartCoroutine(UpdateActualCubes());


        FastUpdateNearestActualCubes();


        if (!SceneController.pause && Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.Pause();
        }

        if (SceneController.pause && Input.GetKeyDown(KeyCode.Return))
        {
            SceneController.Resume();
        }


    }


    IEnumerator UpdateActualCubes()
    {
        if (_localPlayer != null && generated)
        {
            Vector3 playerPos = _localPlayer.transform.position;

            int px = (int)playerPos.x;
            int py = (int)playerPos.y;
            int pz = (int)playerPos.z;


            for (int i = (int)px - actualDistance - 5; i < (int)px + actualDistance + 5; i++)
            {
                for (int j = (int)py - actualDistance - 5; j < (int)py + actualDistance + 5; j++)
                {
                    for (int k = (int)pz - actualDistance - 5; k < (int)pz + actualDistance + 5; k++)
                    {
                        if (!isCorrectCluster(i, j, k))
                            continue;

                        if (_cubeGO[i, j, k] && _localPlayer)
                        {

                            if (Mathf.Abs(i - px) < actualDistance
                                && Mathf.Abs(j - py) < actualDistance
                                && Mathf.Abs(k - pz) < actualDistance)

                            {
                                _cubeGO[i, j, k].SetActive(true);
                                // Vector3 cameraRelative = _player.transform.InverseTransformPoint(_cubeGO[i, j, k].transform.position);
                                // if (cameraRelative.z > -3)
                                // {
                                //     _cubeGO[i, j, k].SetActive(true);
                                // }
                                // else
                                // {
                                //     _cubeGO[i, j, k].SetActive(false);
                                // }

                                // if (Mathf.Abs(i - px) < actualDistance / 2
                                //      && Mathf.Abs(j - py) < actualDistance / 2
                                //      && Mathf.Abs(k - pz) < actualDistance / 2)
                                //     _cubeGO[i, j, k].SetActive(true);



                            }
                            else
                                _cubeGO[i, j, k].SetActive(false);


                        }


                        if (_lampGO[i, j, k])
                        {

                            if (Mathf.Abs(i - px) < actualDistance
                                && Mathf.Abs(j - py) < actualDistance
                                && Mathf.Abs(k - pz) < actualDistance)
                                _lampGO[i, j, k].SetActive(true);
                            else
                                _lampGO[i, j, k].SetActive(false);

                        }


                    }

                }
                yield return null;
            }


            lightManager.UpdateActual(playerPos, actualDistance);
            yield return null;


        }


    }





    void FastUpdateNearestActualCubes()
    {
        if (_localPlayer != null && generated)
        {
            Vector3 playerPos = _localPlayer.transform.position;

            int px = (int)playerPos.x;
            int py = (int)playerPos.y;
            int pz = (int)playerPos.z;

            for (int i = (int)px - 1; i <= (int)px + 1; i++)
            {
                for (int j = (int)py - 1; j <= (int)py + 1; j++)
                {
                    for (int k = (int)pz - 1; k <= (int)pz + 1; k++)
                    {
                        if (!isCorrectCluster(i, j, k))
                            continue;

                        if (_cubeGO[i, j, k])
                            _cubeGO[i, j, k].SetActive(true);

                        if (_lampGO[i, j, k])
                            _lampGO[i, j, k].SetActive(true);

                    }
                }
            }
        }
    }




    public bool GetNearEnemySpawn(Vector3 playerPos, int radius, out Vector3 enemyPos)
    {

        for (int i = (int)playerPos.x - radius; i < (int)playerPos.x + radius; i++)
        {
            for (int j = (int)playerPos.y - radius; j < (int)playerPos.y + radius; j++)
            {
                for (int k = (int)playerPos.z - radius; k < (int)playerPos.z + radius; k++)
                {
                    if (!isCorrectCluster(i, j, k))
                        continue;

                    if (enemyTrigger[i, j, k])
                    {
                        enemyPos = new Vector3(i, j, k);
                        return true;
                    }
                }
            }
        }
        enemyPos =  new Vector3(-1, -1, -1);
        return false;
    }

    public void BindPlayerGameObject(GameObject playerGO)
    {

        _localPlayer = playerGO;
    }
 

    ////////////////////////// MAP MODE /////////////////////////////////////////////

     IEnumerator SwitchViewMode(bool viewMode)
    {
        if (_localPlayer != null && generated)
        {
            Vector3 playerPos = _localPlayer.transform.position;

            int px = (int)playerPos.x;
            int py = (int)playerPos.y;
            int pz = (int)playerPos.z;


            for (int i = 0; i < CUBES_I; i++)
            {
                for (int j = 0; j < CUBES_J; j++)
                {
                    for (int k = 0; k < CUBES_K; k++)
                    {
                        if (cubes[i, j, k] == CubeType.VOID) {
                            _cubeGO[i,j,k].SetActive(!viewMode);
                            _mapCubesGO[i, j, k].SetActive(viewMode);
                        }
                    }
                }
            }

  
            yield return null; 
        } 
    }

 

    IEnumerator UpdateActualExplore()
    {
        if (_localPlayer != null && generated)
        {
            Vector3 playerPos = _localPlayer.transform.position;

            int px = (int)playerPos.x;
            int py = (int)playerPos.y;
            int pz = (int)playerPos.z; 

            for (int i = (int)px - actualDistance - 5; i < (int)px + actualDistance + 5; i++)
            {
                for (int j = (int)py - actualDistance - 5; j < (int)py + actualDistance + 5; j++)
                {
                    for (int k = (int)pz - actualDistance - 5; k < (int)pz + actualDistance + 5; k++)
                    {
                        if (!isCorrectCluster(i, j, k))
                            continue;

                        if (explore[i, j, k] && _mapCubesGO[i, j, k] && _localPlayer)
                        { 
                            if (Mathf.Abs(i - px) < actualDistance
                                && Mathf.Abs(j - py) < actualDistance
                                && Mathf.Abs(k - pz) < actualDistance) 
                                {
                                     
                                    _mapCubesGO[i, j, k].SetActive(true); 
                            
                                    // if(isCorrectCluster(i, j+1, k) && isType(i, j+1, k, CubeType.VOID))
                                    // {
                                    //     Color col = _mapCubesGO[i, j+1, k].GetComponent<Renderer>().material.color;
                                    //     _mapCubesGO[i, j+1, k].GetComponent<Renderer>().material.color = new Color(col.r, col.g, col.b, 0.5f);
                                    //     _mapCubesGO[i, j+1, k].SetActive(true); 
                                    // }

                                    // if(isCorrectCluster(i, j-1, k) && isType(i, j-1, k, CubeType.VOID))
                                    // {
                                    //     Color col = _mapCubesGO[i, j-1, k].GetComponent<Renderer>().material.color;
                                    //     _mapCubesGO[i, j-1, k].GetComponent<Renderer>().material.color = new Color(col.r, col.g, col.b, 0.5f);
                                    //     _mapCubesGO[i, j-1, k].SetActive(true); 
                                    // }
 
                                  
                                }  
                        } 

                    }

                }
                yield return null;
            }
 
            yield return null;  
        }


    }



    public void AreaExplored(Vector3 position)
    {
        int px = (int)Mathf.Round(position.x);
        int py = (int)Mathf.Round(position.y);
        int pz = (int)Mathf.Round(position.z); 
    
        for(int x = px, c = 0; isType(x, py, pz, CubeType.VOID) && c < actualDistance; x++, c++)  
        {
            explore[x, py, pz] = true;
            _LineExplored(x, py, pz, new Vector3Int(0,0,1));
            _LineExplored(x, py, pz, new Vector3Int(0,0,-1)); 
        } 

        for(int x = px, c = 0; isType(x, py, pz, CubeType.VOID) && c < actualDistance; x--, c++)  
        {
            explore[x, py, pz] = true;
            _LineExplored(x, py, pz, new Vector3Int(0,0,1));
            _LineExplored(x, py, pz, new Vector3Int(0,0,-1)); 
        } 

        for(int z = pz, c = 0; isType(px, py, z, CubeType.VOID) && c < actualDistance; z++, c++)  
        {
            explore[px, py, z] = true;
            _LineExplored(px, py, z, new Vector3Int(0,0,1));
            _LineExplored(px, py, z, new Vector3Int(0,0,-1)); 
        } 
        
        for(int z = pz, c = 0; isType(px, py, z, CubeType.VOID) && c < actualDistance; z--, c++)  
        {
            explore[px, py, z] = true;
            _LineExplored(px, py, z, new Vector3Int(0,0,1));
            _LineExplored(px, py, z, new Vector3Int(0,0,-1)); 
        }

        for(int y = py, c = 0; isType(px, y, pz, CubeType.VOID) && c < actualDistance; y++, c++)  
        {
            explore[px, y, pz] = true;
            _LineExplored(px, y, pz, new Vector3Int(0,0,1));
            _LineExplored(px, y, pz, new Vector3Int(0,0,-1)); 
        } 
        
        for(int y = py, c = 0; isType(px, y, pz, CubeType.VOID) && c < actualDistance; y--, c++)  
        {
            explore[px, y, pz] = true;
            _LineExplored(px, y, pz, new Vector3Int(0,0,1));
            _LineExplored(px, y, pz, new Vector3Int(0,0,-1)); 
        }     

            
    }


    private void _LineExplored(int i, int j, int k, Vector3Int dir)
    {
        int x = i;
        int y = j;
        int z = k; 
    
        for(int c = 0; isType(x, y, z, CubeType.VOID) && c < 3; c++)  
        {
            explore[x, y, z] = true;
            x += dir.x;
            y += dir.y;
            z += dir.z;
        } 
          
    }


}
