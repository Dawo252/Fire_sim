using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ForestFire2 : MonoBehaviour
{
    private GridManager grid_manager_script;
    private GameObject[,] grid;
    [HideInInspector]
    public Material fire_mat;
    public float fire_chance = 0.4f;
    [HideInInspector]
    public int columns;
    [HideInInspector]
    public int rows;
    public Material scorched_ground;
    public GameObject grid_manager;
    public int max_burn_time;
    public bool use_wind_generator;
    public float wind_left_right_user;
    public float wind_up_down_user;
    [HideInInspector]
    public bool is_started;
    private float wind_up_down = 0;
    private float wind_left_right = 0;
    private string wind_up_down_str;
    private string wind_left_right_str;
    public GameObject wind_indicator_up_down;
    public GameObject wind_indicator_left_right;
    public float total_burn_chance;
    private Dictionary<string, string> antonym_dict = new Dictionary<string, string>
    {
        { "left", "right" },
        {"right", "left"},
        { "up", "down" },
        {"down", "up"}
    };
    private int iter_val = 4;
    private GameObject selected_tile;
    
    void Start()
    {
        // pobranie potrzebnych wartości z obiektu GridManager
        grid_manager = GameObject.FindGameObjectWithTag("grid_manager");
        grid_manager_script = grid_manager.GetComponent<GridManager>();
        grid = grid_manager_script.grid;
        columns = grid_manager.GetComponent<GridManager>().columns;
        rows = grid_manager.GetComponent<GridManager>().rows;
    }
    
    void Update(){ 
        if (Input.GetMouseButtonDown(0) && is_started == false){ // sprawdzanie czy wciśnięto lewy przycisk myszy, jeśli symulacja się jeszcze nie rozpoczęła
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // pobranie komórki, która została wciśnięta
                selected_tile = hit.collider.gameObject;
                TileParameters tile_params = selected_tile.GetComponent<TileParameters>();
                // sprawdzeni czy wciśnięta komórka to drzewo (tylko wciskając drzewo można zacząć pożar)
                if (tile_params.tile_type == grid_manager_script.trees_mat)
                {                                                                                   
                    // ustawienie, że funkcja się zaczęła
                    is_started = true;
                    // zmiana materiału komórki na płonący
                    selected_tile.GetComponent<Renderer>().material = fire_mat;
                    tile_params.burning = true;
                    // rozpoczęcie korutyn
                    StartCoroutine(SpreadFire2());
                    StartCoroutine(WindGenerator());
                }
            }
        }
    }

    IEnumerator SpreadFire2() // korutyna odpowiedzialna za symulację pożaru, pokazuje wyniki co około sekundę
    {
        while (true)
        {
            // znalezienie indeksu komórki rozpoczynającej pożar
            int selected_i = 0;
            int selected_j = 0;
            for (int x = 0; x < columns; x++)
            {
                for (int z = 0; z < rows; z++)
                {
                    if (grid[x, z] == selected_tile)
                    {
                        selected_i = x;
                        selected_j = z;
                    }
                }
            }
            
            yield return new WaitForSeconds(1f); // ustawienie co ile sekund widać wyniki
            for (int i = 0; i<columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    GameObject tile = grid[i, j];
                    Renderer tile_renderer = tile.GetComponent<Renderer>();
                    TileParameters tile_params = tile.GetComponent<TileParameters>();
                    if (tile_params.burning)
                    {
                        tile_params.burning_time += 1;
                        // jeżeli komórka płonie dłużej niż ustawiona wartość max_burn_time zostaje spalona
                        if (tile_params.burning_time == max_burn_time)
                        {
                            tile_params.burning = false;
                            tile_params.scorched = true;
                            tile_renderer.material = scorched_ground;
                        }
                        // jeżeli płonie krócej losowane jest czy zostanie spalona przed upływem czasu
                        else if (Random.Range(0.0f, 1.0f) <= total_burn_chance)
                        {
                            tile_params.burning = false;
                            tile_params.scorched = true;
                            tile_renderer.material = scorched_ground;
                        }
                    }
                    // jeżeli kmórka nie płonie i nie jest spalona sprawdzane jest czy się zapali
                    else if (tile_params.burning == false && tile_params.scorched == false)
                    {
                        // ustalenie wpływu wiatru na szanse spalenia komórki
                        foreach (var key in tile_params.neighbors_dict.Keys)
                        {
                            float wind_fire_chance = 0;
                            if (wind_left_right_str == key)
                            {
                                wind_fire_chance = System.Math.Abs(wind_left_right);
                            }

                            if (wind_up_down_str == key)
                            {
                                wind_fire_chance = System.Math.Abs(wind_up_down);
                            }

                            if (wind_left_right_str == antonym_dict[key])
                            {
                                wind_fire_chance = -System.Math.Abs(wind_left_right);
                            }

                            if (wind_up_down_str == antonym_dict[key])
                            {
                                wind_fire_chance = -System.Math.Abs(wind_up_down);
                            }

                            float fire_chance_local = fire_chance;
                            fire_chance_local += wind_fire_chance;
                            // jeżeli sąsiad komórki płonie i może zapalać inne komórki sprawdzany jest materiał komórki i jego wpływ na szanse zapalenia
                            if (tile_params.neighbors_dict[key].GetComponent<TileParameters>().burning && 
                                (tile_params.neighbors_dict[key].GetComponent<TileParameters>().can_burn < 1)
                                )
                            {
                                if (tile_params.tile_type == grid_manager_script.water_mat)
                                {
                                    fire_chance_local = 0;
                                }

                                if (tile_params.tile_type == grid_manager_script.dirt_mat)
                                {
                                    fire_chance_local = fire_chance / 80;
                                }

                                if (tile_params.tile_type == grid_manager_script.sand_mat)
                                {
                                    fire_chance_local = 0;
                                }

                                if (tile_params.tile_type == grid_manager_script.meadow_mat)
                                {
                                    fire_chance_local = fire_chance / 10;
                                }
                                // sprawdzenie czy komórka zostanie zapalona
                                if (Random.Range(0.0f, 1.0f) <= fire_chance_local)
                                {
                                    tile_params.burning = true;
                                    tile_renderer.material = fire_mat;
                                }
                                
                            }
                            // część kodu sprawiająca, że komórka w tej samej iteracji w której została zapalona nie może zapalić innych
                            else if (tile_params.neighbors_dict[key].GetComponent<TileParameters>().burning && i < selected_i && j<selected_j)
                            {
                                tile_params.neighbors_dict[key].GetComponent<TileParameters>().can_burn -= 2;
                            }
                            else if (tile_params.neighbors_dict[key].GetComponent<TileParameters>().burning && j == selected_j && i<selected_i)
                            {
                                tile_params.neighbors_dict[key].GetComponent<TileParameters>().can_burn -= 2;
                            }
                            else if (tile_params.neighbors_dict[key].GetComponent<TileParameters>().burning && i == selected_i && j<selected_j)
                            {
                                tile_params.neighbors_dict[key].GetComponent<TileParameters>().can_burn -= 2;
                            }
                            else if (tile_params.neighbors_dict[key].GetComponent<TileParameters>().burning && j > selected_j && i<selected_i)
                            {
                                tile_params.neighbors_dict[key].GetComponent<TileParameters>().can_burn -= 2;
                            }
                            else if (tile_params.neighbors_dict[key].GetComponent<TileParameters>().burning && i > selected_i && j<selected_j)
                            {
                                tile_params.neighbors_dict[key].GetComponent<TileParameters>().can_burn -= 1;
                            }
                            else if (tile_params.neighbors_dict[key].GetComponent<TileParameters>().burning && i > selected_i)
                            {
                                tile_params.neighbors_dict[key].GetComponent<TileParameters>().can_burn -= 1;
                            }
                            else if (tile_params.neighbors_dict[key].GetComponent<TileParameters>().burning && j > selected_j)
                            {
                                tile_params.neighbors_dict[key].GetComponent<TileParameters>().can_burn -= 1;
                            }
                            else if (tile_params.neighbors_dict[key].GetComponent<TileParameters>().burning)
                            {
                                tile_params.neighbors_dict[key].GetComponent<TileParameters>().can_burn -= 2;
                            }
                        }
                    }
                }
            }
            iter_val += 1;
        }
    }

    // korutyna odpowiedzialna za generowanie wiatru
    IEnumerator WindGenerator()
    {
        // pobranie pól tekstowych na których wyświetli się wartość wiatru
        TextMeshProUGUI up_down = wind_indicator_up_down.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI left_right = wind_indicator_left_right.GetComponent<TextMeshProUGUI>();
        while (true)
        {
            // sprawdzenie czy upłynęły już 4 iteracje korutyny płonięcia lasu
            if (iter_val != 4)
            {
                yield return new WaitForSeconds(0f);
            }
            else
            {
                iter_val = 0;
                // jeżeli tak sprawdzane jest czy user zaznaczył pole use_wind_generator
                if (use_wind_generator)
                {
                    // generowanie wartości wiatru góra-dól i lewo-prawo
                    float rand_up_down = Random.Range(-0.2f, 0.2f);
                    float rand_left_right = Random.Range(-0.2f, 0.2f);
                    // sprawdzenie czy po dodaniu wiatru wartość szansy na zapalenie zmaleje poniżej 0,05
                    if (System.Math.Abs(wind_up_down + rand_up_down) < fire_chance-0.05f)
                    {
                        wind_up_down += rand_up_down;
                    }
                    if (System.Math.Abs(wind_left_right + rand_left_right) < fire_chance-0.05f)
                    {
                        wind_left_right += rand_left_right;
                    }
                }
                else
                {
                    // jeżeli user nie zaznaczył use_wind_generator wartości wiatru ustawiane są jako wartości podane przez usera
                    wind_up_down = wind_up_down_user;
                    wind_left_right = wind_left_right_user;
                }
                // sprawdzanie skąd wieje wiatr ( w zależności od znaku przy odpowiedniej zmiennej )
                if (wind_up_down < 0)
                {
                    wind_up_down_str = "down";
                }
                else
                {
                    wind_up_down_str = "up";
                }
                if (wind_left_right < 0)
                {
                    wind_left_right_str = "right";
                }
                else
                {
                    wind_left_right_str = "left";
                }
                // wyświetlenie wartości wiatru na ekran
                up_down.text = wind_up_down_str + System.Math.Abs(wind_up_down).ToString("0.00");
                left_right.text = wind_left_right_str + System.Math.Abs(wind_left_right).ToString("0.00");
            }
        }
    }
}
