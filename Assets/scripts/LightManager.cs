using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{

    [SerializeField] private GameObject pointLightPrefab = null;

    private GameObject[,,] _lightTable = new GameObject[LevelController.CUBES_I, LevelController.CUBES_J, LevelController.CUBES_K];
    private bool[,,] _nonDestroyLightTable = new bool[LevelController.CUBES_I, LevelController.CUBES_J, LevelController.CUBES_K];  
    private bool[,,] _activatedLightTable = new bool[LevelController.CUBES_I, LevelController.CUBES_J, LevelController.CUBES_K];  
    
    LevelController levelController;
     
    void Awake()
    {
        levelController = (LevelController)GameObject.Find("LevelController").GetComponent<LevelController>();
    }

    void Start()
    {

            for(int i = 0; i<LevelController.CUBES_I; i++)
            for(int j = 0; j<LevelController.CUBES_J; j++)   
            for(int k = 0; k<LevelController.CUBES_K; k++)   
            {
                if(_lightTable[i,j,k]) 
                    Destroy(_lightTable[i,j,k]);
                _nonDestroyLightTable[i,j,k] = false;
                _activatedLightTable [i,j,k] = false;
            }
    }
    bool isCorrectCluster(int i, int j, int k)
    {
        return (i >= 0
                 && j >= 0
                 && k >= 0
                 && i < LevelController.CUBES_I
                 && j < LevelController.CUBES_J
                 && k < LevelController.CUBES_K);
    }

    public int NeihtboursCount(Vector3 position, int radius= 1) {
        int count = 0;
        int i = (int)position.x;
        int j = (int)position.y;
        int k = (int)position.z;

        for(int ii = i-radius; ii<=i+radius; ii++)
            for(int jj = j-radius; jj<=j+radius; jj++)   
                for(int kk = k-radius; kk<=k+radius; kk++)   {
                    if(ii == i && jj == j && kk == k)
                        continue;

                    if(isCorrectCluster(ii, jj, kk) && _lightTable[ii, jj, kk] != null) {
                        count ++;
                    }
                }             
        return count;     
    }

    public void SetNonDestroy(Vector3 pos) 
    {
        if(!isCorrectCluster((int)pos.x, (int)pos.y, (int)pos.z))
            return;
        _nonDestroyLightTable[(int)pos.x, (int)pos.y, (int)pos.z] = true;
    }

    public static Color GetLampColorByPosition(Vector3 position) 
    {

        // int px  = (int)(((int)(position.x / 10.0f))*10.0f);
        // int py  = (int)(((int)(position.y / 10.0f))*10.0f);
        // int pz  = (int)(((int)(position.z / 10.0f))*10.0f);
         int px  = (int)position.x;
         int py  = (int)position.y;
         int pz  = (int)position.z;

        Color col = new Color( 
            ((float)((px * py)%100))/75.0f, 
            ((float)((py * pz)%100))/75.0f,  
            ((float)((pz * px)%100))/75.0f 
        );
 
        // return new Color( 
        //     Mathf.Sin( 0.1f * (position.x * position.y) ), 
        //     Mathf.Sin( 0.1f * (position.y * position.z) ),  
        //     Mathf.Sin( 0.1f * (position.z * position.x) )
        // ); 

 
        return col;
    }
 
    public void TryInsertLight(Vector3 pos, Color color, int noneNeightboursRadius = 1)
    {   
        if(!isCorrectCluster((int)pos.x, (int)pos.y, (int)pos.z))
            return;

            

        if (_lightTable[(int)pos.x, (int)pos.y, (int)pos.z] == null
             && levelController.isType((int)pos.x, (int)pos.y, (int)pos.z, LevelController.CubeType.VOID)
             && NeihtboursCount(pos, noneNeightboursRadius) == 0)
        {
            _lightTable[(int)pos.x, (int)pos.y, (int)pos.z] = Instantiate(pointLightPrefab, new Vector3((int)pos.x, (int)pos.y, (int)pos.z), Quaternion.identity);
            _lightTable[(int)pos.x, (int)pos.y, (int)pos.z].GetComponent<Light>().color = color;
        }
    }

    public void ActivateLight(Vector3 pos, int radius)
    {
 
        for (int i = (int)pos.x - radius; i < (int)pos.x + radius; i++)
        {
            for (int j = (int)pos.y - radius; j < (int)pos.y + radius; j++)
            {
                for (int k = (int)pos.z - radius; k < (int)pos.z + radius; k++)
                {
                    if (!isCorrectCluster(i, j, k))
                        continue;

                    if (_lightTable[i,j,k] != null)
                    {
                        _activatedLightTable[i,j,k] = true;
                    }
                }
            }
        }
 
    }


    public void DestroyLight(Vector3 pos)
    {
        if (_lightTable[(int)pos.x, (int)pos.y, (int)pos.z] != null)
        {
            if(!_nonDestroyLightTable[(int)pos.x, (int)pos.y, (int)pos.z] || NeihtboursCount(pos, 4) > 0) 
                Destroy(_lightTable[(int)pos.x, (int)pos.y, (int)pos.z].gameObject); 
        }
    }

    public void UpdateActual(Vector3 pos, int actualDistance)
    { 
        int px = (int)pos.x;
        int py = (int)pos.y;
        int pz = (int)pos.z; 

        for (int i = (int)px - actualDistance -5; i < (int)px + actualDistance +5; i++)
        {
            for (int j = (int)py - actualDistance  -5; j < (int)py + actualDistance+5; j++)
            {
                for (int k = (int)pz - actualDistance -5; k < (int)pz + actualDistance +5; k++)
                {
                    if (!isCorrectCluster(i, j, k))
                        continue;

                    if (_lightTable[i, j, k] && _activatedLightTable[i, j, k])
                    { 
                        if (Mathf.Abs(i - px) < actualDistance
                            && Mathf.Abs(j - py) < actualDistance
                            && Mathf.Abs(k - pz) < actualDistance)
                            { 
                                _lightTable[i, j, k].SetActive(true);
                                
                            }
                        else
                            _lightTable[i, j, k].SetActive(false); 

                    } 

                }

            }
                
        }

    }

}
