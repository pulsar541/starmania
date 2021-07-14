﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Mirror;

public class LevelController : MonoBehaviour
{ 
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

        public const int OUT = 1000;
    };
 
    public static int CUBES_I = 100;
    public static int CUBES_J = 100;
    public static int CUBES_K = 100;

    public int[,,] cubes;
    
    private List<Vector3> lampPositions = new List<Vector3>();

    public static Vector3 mapCenter = new Vector3(CUBES_I / 2, CUBES_J / 2, CUBES_K / 2);

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject cubeWallPrefab;
    [SerializeField] private GameObject cubeSinglePrefab;
    [SerializeField] private GameObject platformPrefab;

    [SerializeField] private GameObject pointLightPrefab;

    [SerializeField] private GameObject checkPointPrefab;

    public int actualDistance = 3;

    private GameObject[,,] _cubeGO = new GameObject[CUBES_I, CUBES_J, CUBES_K];

    
    private GameObject[,,] _lampGO = new GameObject[CUBES_I, CUBES_J, CUBES_K];


    private List<Vector3> checkPoints = new List<Vector3>();
 
  

    private List<GameObject> _actualCubeList = new List<GameObject>();

    bool isLevelBuilding = true;

    private GameObject _player;

    Vector3 playerPos;

    public bool generated = false;

    public bool buildProcessActive = false;

    private bool _builded = false;
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

    float i_cut = 0;
    float j_cut = 0;
    float k_cut = 0;


    Vector3 cutDirection = new Vector3(0,-1,0);


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



    bool isType(int i, int j, int k, int cubeType)
    {
        return  correctIndex(i, j, k) &&  cubes[i, j, k] == cubeType;
    }



    bool isSolid(int i, int j, int k)
    {
        return  correctIndex(i, j, k) && (cubes[i, j, k] == CubeType.OUT ||  cubes[i, j, k] == CubeType.WALL);
    }


    public int NeihtboursCount(int i, int j, int k, int cubeType, int radius = 1) {
        int count = 0;
        for(int ii = i-radius; ii<=i+radius; ii++)
            for(int jj = j-radius; jj<=j+radius; jj++)   
                for(int kk = k-radius; kk<=k+radius; kk++)   {
                    if(ii == jj && jj == kk)
                        continue;

                    if(correctIndex(ii, jj, kk) && cubes[ii, jj, kk] == (int)cubeType) {
                        count ++;
                    }
                }             
        return count;     
    }

    public int WallsHorizontAroundCount(Vector3 pos) {
        int count = 0; //Mathf.Round
        int i = (int)(pos.x);
        int j = (int)(pos.y);
        int k = (int)(pos.z);
  
        if(     isType(i-1, j, k, CubeType.WALL) 
            ||  isType(i, j, k-1, CubeType.WALL) 
            ||  isType(i+1, j, k, CubeType.WALL) 
            ||  isType(i, j, k+1, CubeType.WALL) ) {
            count ++;
        }
                
        return count;     
    }



    int LampsCount(int i, int j, int k, int radius = 1) {
        int count = 0;
        for(int ii = i-radius; ii<=i+radius; ii++)
            for(int jj = j-radius; jj<=j+radius; jj++)   
                for(int kk = k-radius; kk<=k+radius; kk++)   {
                    if(ii == jj && jj == kk)
                        continue;

                    if(correctIndex(ii, jj, kk) && lampPositions.Contains(new Vector3(ii,jj,kk))) {
                        count ++;
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
                    if (cubes[i, j, k] != 0)
                    {
                        if (_cubeGO[i, j, k] != null)
                            Destroy(_cubeGO[i, j, k]);
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


    void GenCheckpoints(int count) { 
        checkPoints.Clear();
        List<Vector3> voidUnderWallList = new List<Vector3>();

        for (int i = 0; i < CUBES_I; i++)
            for (int j = 1; j < CUBES_J; j++)
                for (int k = 0; k < CUBES_K; k++)  
                    if(cubes[i, j, k] == (int)CubeType.VOID && cubes[i, j-1, k] == (int)CubeType.WALL) {
                         voidUnderWallList.Add(new Vector3(i,j,k));   
                    }    

        for(int c = 0; c < count; c++){
            int voidsCount = voidUnderWallList.Count;
            int n = Random.Range(0, voidsCount);
            checkPoints.Add(voidUnderWallList[n] + new Vector3(0,-0.5f,0));
            voidUnderWallList.RemoveAt(n);
        } 
    }

    void MakeLamps() {
        lampPositions.Clear();  
        for (int i = 0; i < CUBES_I; i++)
            for (int j = 1; j < CUBES_J; j++)
                for (int k = 0; k < CUBES_K; k++) {
                    if(cubes[i, j, k] == (int)CubeType.VOID 
                      //  && cubes[i, j-1, k] != (int)CubeType.WALL 
                       // && NeihtboursCount(i,j,k, CubeType.WALL) >= 3

                        && LampsCount(i,j,k, 5) < 1
                    ) {
                         lampPositions.Add(new Vector3(i,j,k));  
                    }
                }        
    }

    void MakeBridge(int i_cut, int j_kut, int k_kut, int length, bool direction)
    {

        if (direction)
        {
            int ii = i_cut;
            for (int st = 0; st < length; st++)
                if (correctIndex(ii + st, j_kut, k_kut))
                {
                    if (cubes[ii + st, j_kut, k_kut] == 1 && cubes[ii + st, j_kut, k_kut] == CubeType.OUT)
                        break;

                    cubes[ii + st, j_kut, k_kut] = CubeType.PLATFORM;
                }
            for (int st = 0; st < length; st++)
                if (correctIndex(ii - st, j_kut, k_kut))
                {
                    if (cubes[ii - st, j_kut, k_kut] == 1 && cubes[ii - st, j_kut, k_kut] == CubeType.OUT)
                        break;

                    cubes[ii - st, j_kut, k_kut] = CubeType.PLATFORM;
                } 
        }
        else
        {
            int kk = (int)k_cut;
            for (int st = 0; st < length; st++) 
                if (correctIndex(i_cut, j_kut, kk + st))
                {
                    if (cubes[i_cut, j_kut, kk + st] == 1 && cubes[i_cut, j_kut, kk + st] == 1)
                        break;
                    cubes[i_cut, j_kut, kk + st] = CubeType.PLATFORM;
                }
            for (int st = 0; st < length; st++)
                if (correctIndex(i_cut, j_kut, kk - st))
                {
                    if (cubes[i_cut, j_kut, kk - st] == 1 && cubes[i_cut, j_kut, kk - st] == CubeType.OUT)
                        break;
                    cubes[i_cut, j_kut, kk - st] = CubeType.PLATFORM;
                }


        }

    }




    void ProrezKoridor(int length, Vector3 dir, int w, int h, bool withBridges, bool isSmoothVoid = false)
    {
        int width = w;
        int height = h;
         

        for (int st = 0; st < length; st++)
        { 

            i_cut += dir.x;
            j_cut += dir.y;
            k_cut += dir.z;
 

            //if(randomWidth)
            //    width = Random.Range(1,w);

            for (int ii = (int)i_cut; ii < (int)i_cut + width; ii++)
            for (int jj = (int)j_cut; jj < (int)j_cut + height; jj++)
            for (int kk = (int)k_cut; kk < (int)k_cut + width ; kk++)
                if (correctIndex(ii, jj, kk))
                {
                    cubes[ii, jj, kk] = isSmoothVoid ? (int)CubeType.VOID_SMOOTH : (int)CubeType.VOID; 
                }

            // if(withBridges)
            //     for (int jj = j_cut - height / 2; jj <= j_cut + height/2; jj++)
            //     {
            //         int iii = Random.Range(i_cut - width / 2, i_cut + width / 2);
            //         int kkk = Random.Range(k_cut - width / 2, k_cut + width / 2); 
            //         MakeBridge(iii, jj, kkk,  1 /*Random.Range(2, width*2/3)*/,   (jj%2) == 0);
            //     }

 
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
        }
    }


    // void MakeRoom(int roomHalfSizeX,
    //               int roomHalfSizeY,
    //               int roomHalfSizeZ)
    // {
    //     for (int ii = i_cut - roomHalfSizeX; ii <= i_cut + roomHalfSizeX; ii++)
    //         for (int jj = j_cut - roomHalfSizeY; jj <= j_cut + roomHalfSizeY; jj++)
    //             for (int kk = k_cut; kk <= k_cut + roomHalfSizeZ; kk++)
    //                 if (correctIndex(ii, jj, kk))
    //                 {
    //                     //int rr = Random.Range(0, 100);
    //                     //if (rr < 50 || rr > 55) 
    //                     cubes[ii, jj, kk] = 0;
    //                 }
    // }


    private Vector3 GetDirMovementByOrthoDir(OrthoDir dir) 
    { 
        switch (dir )
        {
            case OrthoDir.GO_UP:
                return new Vector3(0,1,0); 
            case OrthoDir.GO_DOWN:
                return new Vector3(0,-1,0); 
            case OrthoDir.GO_FORWARD: 
                return new Vector3(0,0,1); 
            case OrthoDir.GO_BACK:  
                return new Vector3(0,0,-1); 
            case OrthoDir.GO_RIGHT:
                return new Vector3(1,0,0); 
            case OrthoDir.GO_LEFT:
                return new Vector3(-1,0,0); 
        }

        return new Vector3(0,0,0); 
    }

    // private void GenBigCrossRooms(Vector3 startPos) {
    //   //  Dir oldDir = new Dir(); 
              
    //     int ox = (int)startPos.x;
    //     int oy = (int)startPos.y;
    //     int oz = (int)startPos.z; 
    
    //     i_cut = ox;
    //     j_cut = oy;
    //     k_cut = oz;
    
    //     int max_cut = 100;
    //     int temp_max_cut = max_cut;

    //     int globalStepNum = 0; 
    //     do
    //     {
    //         globalStepNum ++; 
    //        // Dir dir =; 
           

    //         Vector3 dirM = new Vector3(Random.Range(-1, 1),Random.Range(-1, 1), Random.Range(-1, 1));
 
    //         OrthoDir dir = (OrthoDir)Random.Range(0.0f, (int)OrthoDir.GO_DOWN );
    //         dirM = GetDirMovementByOrthoDir(dir); 


    //         int steps = Random.Range(3, 15); 

                      
    //         if(dir == OrthoDir.GO_UP || dir == OrthoDir.GO_DOWN) 
    //               steps=Random.Range(0,1);
        
    //         if ((globalStepNum%5)==0)
    //         {  
    //             int width = Random.Range(2, steps/2);
    //             int height = Random.Range(2, steps/2);
    //             ProrezKoridor(steps, dirM, width, height, false, false);
    //         }
    //         else 
    //         {

    //             steps = Random.Range(3, 10); 
    //             //if(dir == Dir.GO_UP || dir == Dir.GO_DOWN) 
    //             //    steps=Random.Range(0,1);

    //              //dirM = new Vector3(Random.Range(-1.0f, 1.0f),Random.Range(-1.0f, 1.0f),Random.Range(-1.0f, 1.0f));
    //               if(dir == OrthoDir.GO_UP || dir == OrthoDir.GO_DOWN) 
    //               steps=Random.Range(0,1);

    //             int width = Random.Range(1, 2);
    //             int height = Random.Range(1, 2); 
                
    //             // if(dir == Dir.GO_UP || dir == Dir.GO_DOWN) 
    //             //     steps/=2;

    //             ProrezKoridor(steps, dirM, width, height, true, false); 
    //         }

    //         max_cut --; 
    //         //oldDir = dir; 

    //         // ox = i_cut;
    //         // oy = j_cut;
    //         // oz = k_cut; 

    //     } while (max_cut > 0);
              
    // } 




    // private void GenBigDiagonalRooms(Vector3 startPos) {
    //     OrthoDir oldDir = new OrthoDir(); 
              
    //     int ox = (int)startPos.x;
    //     int oy = (int)startPos.y;
    //     int oz = (int)startPos.z; 
    
    //     i_cut = ox;
    //     j_cut = oy;
    //     k_cut = oz;
    
    //     int max_cut = 100;
    //     int temp_max_cut = max_cut;

    //     int globalStepNum = 0; 
    //     do
    //     {
    //         globalStepNum ++; 
    //        // Dir dir = (Dir)Random.Range(0.0f, (int)Dir.GO_DOWN + 1); 

    //        Vector3 dir = new Vector3(Random.Range(-1.0f, 1.0f),Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        
    //         int steps = Random.Range(3, 15); 
    //         if ((globalStepNum%4)==0)
    //         {  
    //             int width = Random.Range(2, steps/2);
    //             int height = Random.Range(2, steps/2);
    //             ProrezKoridor(steps, dir, width, height, false, false);
    //         }
    //         else 
    //         {
    //             steps = Random.Range(3, max_cut/10);
    //             int width = Random.Range(1, 4);
    //             int height = Random.Range(1, 4); 
                
    //             // if(dir == Dir.GO_UP || dir == Dir.GO_DOWN) 
    //             //     steps/=2;

    //             ProrezKoridor(steps, dir, width, height, true, false); 
    //         }

    //         max_cut --; 
    //         //oldDir = dir; 

    //         // ox = i_cut;
    //         // oy = j_cut;
    //         // oz = k_cut; 

    //     } while (max_cut > 0);
              
    // } 


    private void GenTunnels(Vector3 startPos, int width, int tunnelsCount, int tunnelMinLength, int tunnelMaxLength) 
    { 
              
        i_cut = startPos.x;
        j_cut = startPos.y;
        k_cut = startPos.z;  
        int max_cut = tunnelsCount;
        int temp_max_cut = max_cut;  
        do
        { 
            //Dir dir = (Dir)Random.Range(0.0f, (int)Dir.GO_DOWN + 1);  
             Vector3 dir = new Vector3(Random.Range(-1.0f, 1.0f),Random.Range(-1f, 1.0f), Random.Range(-1.0f, 1.0f));
        
            int steps = Random.Range(tunnelMinLength, tunnelMaxLength);   
            int height = Random.Range(1, 2);
            ProrezKoridor(steps, dir, width, height, false, false); 
            max_cut --;   

        } while (max_cut > 0);
              
    } 

    private void GenOrthoTunnels(Vector3 startPos, int maxWidth, int maxHeight, int tunnelsCount,  int tunnelMinLength, int tunnelMaxLength) 
    {  
        i_cut = (int)startPos.x;
        j_cut = (int)startPos.y;
        k_cut = (int)startPos.z;  
        int max_cut = tunnelsCount;
        int temp_max_cut = max_cut;  
 
        do
        { 
            OrthoDir orthoDir = (OrthoDir)Random.Range(-1, (int)OrthoDir.GO_DOWN + 1 ); 
            Vector3 dirM = GetDirMovementByOrthoDir(orthoDir); 
             
            int steps = Random.Range(tunnelMinLength, tunnelMaxLength);  
            int width = Random.Range(1, maxWidth);  
            int height = Random.Range(1, maxHeight);  

            
            //  if(orthoDir == OrthoDir.GO_DOWN  || orthoDir == OrthoDir.GO_UP) {
            //      dirM = new Vector3(Random.Range(-1.0f, 1.0f),Random.Range(-1f, 1.0f), Random.Range(-1.0f, 1.0f));
            //      steps = Random.Range(1, 4); 
            //      width++;
            //      height++;
            //  }

           if(orthoDir == OrthoDir.GO_DOWN  || orthoDir == OrthoDir.GO_UP) {
               steps = Random.Range(0, 3);  
               height = 1;
           }
            ProrezKoridor(steps, dirM, width, height, false, false); 
            max_cut --;   

        } while (max_cut > 0);
              
    } 



    int ClustersCountByType(int cubeType) 
    {   int count = 0;
        for (int i = 0; i < CUBES_I; i++)
            for (int j = 0; j < CUBES_J; j++)
                for (int k = 0; k < CUBES_K; k++)
                    if(cubes[i, j, k] == cubeType)
                        count++;
        return count;        
    }

    public Vector3 GenerateLevel(int seed)
    {

        if (seed > 0)
            Random.InitState(seed);

        _isLevelGenerating = true;
        Vector3 playerStartPosition;

        for (int i = 0; i < CUBES_I; i++)
            for (int j = 0; j < CUBES_J; j++)
                for (int k = 0; k < CUBES_K; k++)
                    cubes[i, j, k] = (int)CubeType.WALL;
                
        for (int i = (int)mapCenter.x - 2;  i <= (int)mapCenter.x + 2;  i++)
        for (int j = (int)mapCenter.y;      j <= (int)mapCenter.y + 1;  j++)
        for (int k = (int)mapCenter.z - 2;  k <= (int)mapCenter.z + 2;  k++)
            cubes[i, j, k] = CubeType.VOID;

        i_cut = CUBES_I / 2;
        j_cut = CUBES_J / 2;
        k_cut = CUBES_K / 2;

        playerStartPosition = new Vector3(i_cut, j_cut, k_cut); 
 
         GenOrthoTunnels(mapCenter, 3, 3, 100, 3, 7);
         GenOrthoTunnels(mapCenter, 3, 3, 100, 3, 7);
         GenOrthoTunnels(mapCenter, 3, 3, 100, 3, 7);

         GenCheckpoints(10);

       //GenBigCrossRooms(mapCenter);

        //GenBigDiagonalRooms(mapCenter);

        
        // for(int c = 0; c < 10; c++) {
        //     int voidsCount = ClustersCountByType(CubeType.VOID);
        //     int randomVoidNumber = Random.Range(0, voidsCount);
        //     for (int i = 0; i < CUBES_I; i++)
        //         for (int j = 0; j < CUBES_J; j++)
        //             for (int k = 0; k < CUBES_K; k++) 
        //             {
        //                 voidsCount--;
        //                 if(voidsCount == 0) {
        //                     GenTunnels(new Vector3(i,j,k), Random.Range(2,4), 100);
        //                     break;
        //                 }
        //             }     
        // }
                     
     
        // int nextStartCutX = (int)mapCenter.x;
        // int nextStartCutY = (int)mapCenter.y;
        // int nextStartCutZ = (int)mapCenter.z;

              
        // int ox = (int)mapCenter.x;
        // int oy = (int)mapCenter.y;
        // int oz = (int)mapCenter.z;

        

        // int c = 0;
        // //for(int c = 0; c<2; c++) 
        // {   
        //     i_cut = nextStartCutX;
        //     j_cut = nextStartCutY;
        //     k_cut = nextStartCutZ;
           
        //     int max_cut = 100;
        //     int temp_max_cut = max_cut;

        //     //int fromRoomToRoom = 16 ; 
        //     int globalStepNum = 0;

        //     do
        //     {
        //         globalStepNum ++; 
        //         Dir dir = (Dir)Random.Range(0.0f, (int)Dir.GO_DOWN + 1); 
            
        //         int steps = Random.Range(3, 15); 
        //         if ((globalStepNum%4)==0)
        //         {  
        //             int width = Random.Range(2, steps);
        //             int height = Random.Range(2, steps);
        //             ProrezKoridor(steps, dir, width, height, false, false);
        //         }
        //         else 
        //         {
        //             steps = Random.Range(3, max_cut/10);
        //             int width = c==1? 1 : Random.Range(1, 4);
        //             int height = c==1? 1 :Random.Range(1, 4);
                    
                    
        //               if(dir == Dir.GO_UP || dir == Dir.GO_DOWN) 
        //                  steps/=2;

        //             ProrezKoridor(steps, dir, width, height, true, false); 
        //       }
    
    
        //         max_cut --; 
        //         oldDir = dir;


        //         if(max_cut == temp_max_cut /2) {
        //             nextStartCutX = i_cut;
        //             nextStartCutY = j_cut;
        //             nextStartCutZ = k_cut;
        //         }

        //         ox = i_cut;
        //         oy = j_cut;
        //         oz = k_cut;


        //     } while (max_cut > 0);
        // } 

        OptimizationLevel(); 
        
        MakeLamps();

        _isLevelGenerating = false;


        generated = true;

        return playerStartPosition;
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
        isLevelBuilding = true;
        _builded = false;

        int iterator = 0;

        for (int i = 0; i < CUBES_I; i++)
        {
            for (int j = 0; j < CUBES_J; j++)
            {
                for (int k = 0; k < CUBES_K; k++)
                {
                    if (cubes[i, j, k] == (int)CubeType.WALL)
                    {
                        //Debug.Log("BuildL " + i + j + k);

                     //   if (correctIndex(i, j + 1, k) && cubes[i, j + 1, k] != (int)CubeType.WALL
                      //      && correctIndex(i, j - 1, k) && cubes[i, j - 1, k] != (int)CubeType.WALL)
                     //   {
                      //      _cubeGO[i, j, k] = (GameObject)Instantiate(cubeSinglePrefab);
                     //   }
                    //    else
                    //    {
                            _cubeGO[i, j, k] = (GameObject)Instantiate(cubeWallPrefab);
                     //   } 

                    } 

                    if (cubes[i, j, k] == (int)CubeType.PLATFORM)
                    {
                         _cubeGO[i, j, k] = (GameObject)Instantiate(platformPrefab); 
                    }
  
                     if( _cubeGO[i, j, k]) {
                        _cubeGO[i, j, k].transform.position = new Vector3(i, j, k)  ; 
                     }

                    if(_cubeGO[i, j, k])
                        _cubeGO[i, j, k].SetActive(false);
                    

                    iterator++;

                    if (iterator >= CUBES_I * CUBES_J * CUBES_K)
                        _builded = true;
                }

            }
            yield return null;
            isLevelBuilding = false;
            // yield break;
        }


 
        foreach(Vector3 lampPos in lampPositions) 
        {
            _lampGO[(int)lampPos.x, (int)lampPos.y,(int)lampPos.z] = (GameObject)Instantiate(pointLightPrefab);
            _lampGO[(int)lampPos.x, (int)lampPos.y,(int)lampPos.z].transform.position = lampPos;  
            Color lightColor = new Color( 
                  ((float)((int)(lampPos.x * lampPos.y)%100))/75.0f, 
                  ((float)((int)(lampPos.y * lampPos.z)%100))/75.0f,  
                  ((float)((int)(lampPos.z * lampPos.x)%100))/75.0f 
                );
            _lampGO[(int)lampPos.x, (int)lampPos.y,(int)lampPos.z].GetComponent<Light>().color = lightColor;

           if(_lampGO[(int)lampPos.x, (int)lampPos.y,(int)lampPos.z]) 
                _lampGO[(int)lampPos.x, (int)lampPos.y,(int)lampPos.z].SetActive(false);
                    
        }

        foreach(Vector3 checkpointPos in checkPoints) 
        {
            GameObject checkpoint = (GameObject)Instantiate(checkPointPrefab);
            checkpoint.transform.position = checkpointPos;    
        }

    }

    void Start()
    {
        cubes = new int[CUBES_I, CUBES_J, CUBES_K];
    }


    public void Build()
    {
        StartCoroutine(BuildLevel());
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
            StartCoroutine(UpdateActualCubes());
        }


        if (Input.GetMouseButtonDown(0))
        {

        }

    }



    IEnumerator UpdateActualCubes()
    {
        if (_player != null && generated)
        {
            Vector3 playerPos = _player.transform.position;

            int px = (int)playerPos.x;
            int py = (int)playerPos.y;
            int pz = (int)playerPos.z;


            for (int i = (int)px - actualDistance -10; i < (int)px + actualDistance +10; i++)
            {
                for (int j = (int)py - actualDistance  -10; j < (int)py + actualDistance+10; j++)
                {
                    for (int k = (int)pz - actualDistance -10; k < (int)pz + actualDistance +10; k++)
                    {
                        if (!correctIndex(i, j, k))
                            continue;

                        if (_cubeGO[i, j, k])
                        {

                            if (Mathf.Abs(i - px) < actualDistance
                                && Mathf.Abs(j - py) < actualDistance
                                && Mathf.Abs(k - pz) < actualDistance) 
                                _cubeGO[i, j, k].SetActive(true);
                            else
                                _cubeGO[i, j, k].SetActive(false); 

                        }
 

                        if ( _lampGO[i, j, k])
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
           
 

        }
    }


    public void BindPlayerGameObject(GameObject playerGO) {
        _player = playerGO;
    }

}
