using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Unity.VisualScripting;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;



public class GridManager : MonoBehaviour
{

    public GameObject cube_prefab;
    public GameObject[,] grid;
    public Material trees_mat;
    public Material water_mat;
    public Material dirt_mat;
    public Material meadow_mat;
    public Material sand_mat;
    public Material[] mat_grid;
    private int _vertical;
    private int _horizontal;
    public int columns;
    public int rows;
    public TextAsset textureToRead;
    public int width;
    public int height;

    void Start()
    {
        SetBitmapSize();
        // ustawienia kamery, żeby mapa wraz ze znacznikami wiatru znajdowały się w widoku kamery
        Camera.main.orthographicSize = height/2+width/15;
        Camera.main.transform.position = new Vector3((height/2+width/15), 30, (height/2+width/15) * 1.6f);
        // _vertical = ((int)Camera.main.orthographicSize-1)*2;
        // _horizontal = _vertical * (Screen.width / Screen.height);
        
        //wykorzystanie wartości odnalezionych w SetBitmapSize do ustawienia rozmiaru tablicy mapy
        columns = height;
        rows = width;
        grid = new GameObject[columns, rows];
        // utworzenie tablicy materiałów możliwych do wyboru
        mat_grid = new[] { dirt_mat, trees_mat, water_mat, sand_mat, meadow_mat };
        List<List<int>> bitmap = GetBitmap();
        // foreach (List<int> innerList in bitmap)
        // {
        //     Debug.Log(innerList.Count);
        // }
        // główna pętla tworząca mapę
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                // stworzenie obiektu cube (komórki) na scenie
                GameObject cube = SpawnCube(i, j);
                // pobranie obiektu TileParameters dla utworzonej komórki
                TileParameters tile_params_temp = cube.GetComponent<TileParameters>();
                // ustawienie odpowienich parametrów dla komórki
                tile_params_temp.z = i;
                tile_params_temp.t = j;
                // przypisanie komórki do rodzica obiektu GridManager dodanego do sceny Unity
                cube.transform.parent = gameObject.transform;
                //dodanie komórki na odpowienich współrzędnych w tablicy
                grid[i, j] = cube;
            }
        }
        
        // pętla odpowiadająca za nadanie komórkom materiałów odpowiednich do wartości z bitmapy
        for (int z = 0; z < columns; z++)
        {
            for (int t = 0; t < rows; t++)
            {
                // pobranie komórki z tablicy komórek
                GameObject tile = grid[z, t];
                // sprawdzenie jaka wartość znajduje się w bitmapie na tej samej pozycji co komórka
                int material_ind = bitmap[z][t];
                // wybranie z listy materiału o indeksie równym wartości znalezionej na pozycji komórki w bitmapie
                // i ustawienie tego materiału oraz parametru komórki
                tile.GetComponent<Renderer>().material = mat_grid[material_ind];
                tile.GetComponent<TileParameters>().tile_type = mat_grid[material_ind];
                TileParameters tile_params = tile.GetComponent<TileParameters>();
                // utworzenie słownika sąsiadów komórki w zależności od położenia komórki i ustawienie go jako parametru
                // neighbor_dict
                if (z == columns - 1 && t == rows - 1)
                {
                    // tile_params.neighbors = new[] { grid[z - 1, t], grid[z, t - 1] }; //z-1 -> w dol z+1 -> w gore t+1 -> w lewo t-1 -> w prawo
                    tile_params.neighbors_dict = new Dictionary<string, GameObject>()
                    {
                        {"down", grid[z - 1, t]},
                        {"right", grid[z, t - 1]}
                    };
                }
                else if (z == 0 && t == rows - 1)
                {
                    // tile_params.neighbors = new[] { grid[z + 1, t], grid[z, t - 1] };
                    tile_params.neighbors_dict = new Dictionary<string, GameObject>()
                    {
                        {"up", grid[z + 1, t]},
                        {"right", grid[z, t - 1]}
                    };
                }
                else if (z == 0 && t == 0)
                {
                    // tile_params.neighbors = new[] { grid[z + 1, t], grid[z, t + 1] };
                    tile_params.neighbors_dict = new Dictionary<string, GameObject>()
                    {
                        {"up", grid[z + 1, t]},
                        {"left", grid[z, t + 1]}
                    };
                }
                else if (z == columns - 1 && t == 0)
                {
                    // tile_params.neighbors = new[] { grid[z - 1, t], grid[z, t + 1] };
                    tile_params.neighbors_dict = new Dictionary<string, GameObject>()
                    {
                        {"down", grid[z - 1, t]},
                        {"left", grid[z, t + 1]}
                    };
                }
                else if (z == columns - 1)
                {
                    // tile_params.neighbors = new[] { grid[z - 1, t], grid[z, t + 1], grid[z, t - 1] };
                    tile_params.neighbors_dict = new Dictionary<string, GameObject>()
                    {
                        {"down", grid[z - 1, t]},
                        {"left", grid[z, t + 1]},
                        {"right", grid[z, t - 1]}
                    };
                }
                else if (t == rows - 1)
                {
                    // tile_params.neighbors = new[] { grid[z - 1, t], grid[z + 1, t], grid[z, t - 1] };
                    tile_params.neighbors_dict = new Dictionary<string, GameObject>()
                    {
                        {"down", grid[z - 1, t]},
                        {"up", grid[z + 1, t]},
                        {"right", grid[z, t - 1]}
                    };
                }
                else if (t == 0)
                {
                    // tile_params.neighbors = new[] { grid[z - 1, t], grid[z + 1, t], grid[z, t + 1] };
                    tile_params.neighbors_dict = new Dictionary<string, GameObject>()
                    {
                        {"down", grid[z - 1, t]},
                        {"up", grid[z + 1, t + 1]},
                        {"left", grid[z, t + 1]}
                    };
                }
                else if (z == 0)
                {
                    // tile_params.neighbors = new[] { grid[z + 1, t], grid[z, t + 1], grid[z, t - 1] };
                    tile_params.neighbors_dict = new Dictionary<string, GameObject>()
                    {
                        {"up", grid[z + 1, t]},
                        {"left", grid[z, t + 1]},
                        {"right", grid[z, t - 1]}
                    };
                }
                else
                {
                    // tile_params.neighbors = new[] { grid[z + 1, t], grid[z - 1, t], grid[z, t + 1], grid[z, t - 1] };
                    tile_params.neighbors_dict = new Dictionary<string, GameObject>()
                    {
                        {"down", grid[z - 1, t]},
                        {"up", grid[z + 1, t]},
                        {"left", grid[z, t + 1]},
                        {"right", grid[z, t - 1]}
                    };
                }
                Debug.Log(tile_params.neighbors_dict);
            }
        }
    }

    // funkcja pomocnicza do jednoczesnego tworzenia instancji obiektu komórki oraz tworzenia komórki na scenie Unity
    private GameObject SpawnCube(int x, int z)
    {
        Vector3 position = new Vector3(x - (_horizontal - 0.5f), 0, z - (_vertical - 0.5f));
        GameObject cube = Instantiate(cube_prefab, position, Quaternion.identity);
        return cube;
    }
    
    private List<List<int>> GetBitmap()
    {
        List<List<int>> bitmap = new List<List<int>>();
        // utworzenie zmiennej typu Texture2D o rozmiarze otrzymanym wcześniej z nazwy pliku .bytes
        Texture2D tex = new Texture2D(width, height);
        // załadowanie pliku .bytes
        tex.LoadImage(textureToRead.bytes);
        // stworzenie tablicy składowych koloru pikseli tekstury
        Color[] pixels = tex.GetPixels();
        int counter = 0;
        List<int> bitmap_row = new List<int>();
        // Iterowanie po wartościach RGBA pikseli i dodawanie wartości do listy bitmap w zależności od koloru piksela
        for (int i = 0; i < pixels.Length; i++)
        {
            counter++;
            Color pixelColor = pixels[i];
            if (pixelColor.g == 1 && pixelColor.r == 1 && pixelColor.b == 1 && pixelColor.a == 0)
            {
                bitmap_row.Add(0);
            }
            else if(pixelColor.g == 1 && pixelColor.r == 0 && pixelColor.b == 0 && pixelColor.a == 0)
            {
                bitmap_row.Add(1);
            }
            else if(pixelColor.b > pixelColor.g && pixelColor.b > pixelColor.r)
            {
                bitmap_row.Add(2);
            }
            else if (pixelColor.r == pixelColor.g && pixelColor.b == 0)
            {
                bitmap_row.Add(3);
            }
            else if (pixelColor.g == 1 && pixelColor.r == 0 && pixelColor.b == 0 && pixelColor.a == 1)
            {
                bitmap_row.Add(4);
            }
        }
        // zmiana wymiaru bitmap z tablicy jednowymiarowej na dwuwymiarową od odpowiedniej ilości kolumn i długości wiersza
        bitmap = SplitList(bitmap_row, width);
        // gameObject.GetComponent<Renderer>().material.mainTexture = tex;
        return bitmap;
    }
    
    // funkcja pomocnicza pomagająca w w zmianie listy jednowymiarowej na dwuwymiarową o odpowiednich rozmiarach
    static List<List<T>> SplitList<T>(List<T> originalList, int chunkSize)
    {
        return Enumerable.Range(0, (originalList.Count + chunkSize - 1) / chunkSize)
            .Select(i => originalList.Skip(i * chunkSize).Take(chunkSize).ToList())
            .ToList();
    }
    
    public void SetBitmapSize()
    {
        List<int> sizes = new List<int>();
        string input = textureToRead.name;

        // regex do znajdowania wartości numerycznych w tekscie
        string pattern = @"\d+";

        // znalezienie wszystkich liczb w stringu (nazwie pliku.bytes)
        MatchCollection matches = Regex.Matches(input, pattern);
        
        // wyciągnięcie zmatchowanych wartości do tabeli sizes
        foreach (Match match in matches)
        {
            string value = match.Value;
            
            int number = int.Parse(value);

            sizes.Add(number);
        }
        // nadanie wartości dla width i height
        width = sizes[0];
        height = sizes[1];
    }
}
