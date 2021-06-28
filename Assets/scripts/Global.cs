using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global  
{
    
    public static int CUBES_I = 100; 
    public static int CUBES_J = 100;
    public static int CUBES_K = 100;
    
    public static int[,,] cubes;

    public static Vector3 mapCenter = new Vector3(CUBES_I/2, CUBES_J/2, CUBES_K/2);

    public static float GlobalGravity = -3.657f;
}
