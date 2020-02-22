using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{

    public static int SIZE = 60;
    public static float[,] FOOD = new float[SIZE, SIZE];
    public static float[,] ORGANICS = new float[SIZE, SIZE];

    static Main()
    {
        // заполняем стартовую еду
        for(int i = 0; i < SIZE; i++)
        {
            for(int j = 0; j < SIZE; j++)
            {
                FOOD[i, j] = 100f;
                ORGANICS[i, j] = 0f;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // создаем начальную клетку по клику
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GameObject c = (GameObject)Object.Instantiate(Resources.Load("Cell", typeof(GameObject)), new Vector3(pos.x, pos.y, 0), Quaternion.identity);
            Cell cell = c.GetComponent<Cell>();
            cell.type = 0;
            cell.hp = 40f;
            cell.genome = new List<List<int>>();
            // начальный геном
            cell.genome.Add(new List<int>() {
                2, 1, 0,
                2, 5, 0,
                6, 6, 6, 6,
                1, 3, 0,
                7, 7, 7, 7,
                3, 0, 0
            });
            cell.genome.Add(new List<int>() 
                {7}
            );
            cell.genome.Add(new List<int>() 
                {7}
            );
            cell.genome.Add(new List<int>() 
                {6}
            );
            cell.genome.Add(new List<int>() 
                {0}
            );
            cell.genome.Add(new List<int>() 
                {0}
            );
            cell.genome.Add(new List<int>() 
                {0}
            );
        }
    }
}
