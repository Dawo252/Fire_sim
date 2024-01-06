using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileParameters : MonoBehaviour
{
    /// <summary>
    /// skrypt przypięty do każdej komórki przypięty do każdej z nich (konkretnie do prefabrykatu objektu komórki)
    /// </summary>
    public IDictionary<string, GameObject> neighbors_dict;
    public Material tile_type;
    public bool burning = false;
    public bool scorched = false;
    public int z;
    public int t;
    public int burning_time = 0;
    public int can_burn = 2;

}
