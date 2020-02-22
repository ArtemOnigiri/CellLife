using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Dictionary<Cell, HingeJoint2D> links;

    public List<List<int>> genome;
    public int genomeIndex;
    public int type;
    public float hp;
    public float organics;
    public float glue;
    public float age;

    private float geneTimer = 0f;

    void Update()
    {
        // выполняем команды в геноме раз в 0.25 сек
        if(geneTimer > 0.25f)
        {
            NextGene();
            geneTimer = 0f;
        }
        if(type == 1)
        {
            // зеленая клетка берет еду из массива FOOD
            int x = (int)(transform.position.x + Main.SIZE / 2);
            if(x < 0) x = 0;
            else if(x > Main.SIZE - 1) x = Main.SIZE - 1;
            int y = (int)(transform.position.y + Main.SIZE / 2);
            if(y < 0) y = 0;
            else if(y > Main.SIZE - 1) y = Main.SIZE - 1;
            if(Main.FOOD[x, y] >= Time.deltaTime * 8f)
            {
                hp += Time.deltaTime * 8f;
                Main.FOOD[x, y] -= Time.deltaTime * 8f;
            }
        }
        else if(type == 2)
        {
            // синяя клетка летит вперед и тратит на это еду
            GetComponent<Rigidbody2D>().AddForce(transform.up * 0.1f);
            hp -= Time.deltaTime;
            organics += Time.deltaTime;
        }
        else if(type == 4)
        {
            // желтая клетка берет еду из массива ORGANICS
            int x = (int)(transform.position.x + Main.SIZE / 2);
            if(x < 0) x = 0;
            else if(x > Main.SIZE - 1) x = Main.SIZE - 1;
            int y = (int)(transform.position.y + Main.SIZE / 2);
            if(y < 0) y = 0;
            else if(y > Main.SIZE - 1) y = Main.SIZE - 1;
            if(Main.ORGANICS[x, y] >= Time.deltaTime * 8f)
            {
                hp += Time.deltaTime * 8f;
                Main.ORGANICS[x, y] -= Time.deltaTime * 8f;
            }
        }
        // делим hp между связанными клетками
        float hpPart = hp * Time.deltaTime * 0.1f;
        foreach(Cell c in links.Keys)
        {
            hp -= hpPart;
            c.hp += hpPart;
        }
        // отнимаем hp и увеличиваем всякие таймеры
        age += Time.deltaTime;
        geneTimer += Time.deltaTime;
        hp -= Time.deltaTime;
        organics += Time.deltaTime;
        if(hp < 0) Kill();
        else if(hp > 256)
        {
            if(type == 3) hp = 256;
            else Kill();
        }
    }

    void Awake()
    {
        links = new Dictionary<Cell, HingeJoint2D>();
        genomeIndex = 0;
        glue = 1f;
        age = 0f;
        organics = 0f;
    }

    void OnJointBreak2D(Joint2D joint)
    {
        Rigidbody2D connectedBody = joint.connectedBody;
        if(connectedBody)
        {
            Cell cell = connectedBody.GetComponent<Cell>();
            BreakLink(cell);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Cell cell = col.gameObject.GetComponent<Cell>();
        if(cell == null) return;
        if(glue + cell.glue > 1f && (age > 1f || cell.age < 1f)) Link(cell);
        else
        {
            if(!links.ContainsKey(cell) && age > 1f && type != 3)
            {
                // дамажим при столкновении, если клетка красная
                if(cell.type == 3)
                {
                    float dmg = Mathf.Min(Time.deltaTime * 100000f, hp);
                    cell.hp += dmg;
                    hp -= dmg;
                }
            }
        }
        if(hp < 0) Kill();
    }

    // убиваем клетку
    public void Kill()
    {
        int x = (int)(transform.position.x + Main.SIZE / 2);
        if(x < 0) x = 0;
        else if(x > Main.SIZE - 1) x = Main.SIZE - 1;
        int y = (int)(transform.position.y + Main.SIZE / 2);
        if(y < 0) y = 0;
        else if(y > Main.SIZE - 1) y = Main.SIZE - 1;
        Main.ORGANICS[x, y] += organics;
        List<Cell> connectedCells = links.Keys.ToList();
        for(int i = 0; i < connectedCells.Count; i++)
        {
            Destroy(links[connectedCells[i]]);
            BreakLink(connectedCells[i]);
        }
        Destroy(gameObject);
    }

    // мутируем геном клетки
    public void Mutate()
    {
        for(int i = 0; i < genome.Count; i++)
        {
            if(Random.value < 0.2)
            {
                int j = Random.Range(0, genome[i].Count);
                genome[i][j] = Random.Range(0, 8);
            }
            if(Random.value < 0.1)
            {
                genome[i].Add(Random.Range(0, 8));
            }
            else if(Random.value < 0.1)
            {
                if(genome[i].Count > 1)
                {
                    genome[i].RemoveAt(Random.Range(0, genome[i].Count));
                }
            }
        }
    }

    // получаем копию генома клетки
    public List<List<int>> CopyGenome()
    {
        List<List<int>> g = new List<List<int>>();
        for(int i = 0; i < genome.Count; i++)
        {
            g.Add(new List<int>());
            for(int j = 0; j < genome[i].Count; j++)
            {
                g[i].Add(genome[i][j]);
            }
        }
        return g;
    }

    // здесь выполняем следующий ген
    public void NextGene()
    {
        // переход по геному
        if(genome[type][genomeIndex] == 0)
        {
            genomeIndex = (genomeIndex + 1) % genome[type].Count;
            genomeIndex = genome[type][genomeIndex] % genome[type].Count;
        }
        // создание новой клетки
        if(genome[type][genomeIndex] > 0 && genome[type][genomeIndex] < 6)
        {
            float size = 0.6f;
            int newCellType = genome[type][genomeIndex] - 1;
            if(hp < 11) return;
            int newCellDirection = genome[type][(genomeIndex + 1) % genome[type].Count] % 6;
            int newCellGenomeIndex = genome[type][(genomeIndex + 2) % genome[type].Count] % genome[type].Count;
            float realAngle = transform.eulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 2f + newCellDirection / 3f * Mathf.PI;
            Vector2 pos = new Vector2(transform.position.x + size * Mathf.Cos(realAngle), transform.position.y + size * Mathf.Sin(realAngle));
            if(!Physics2D.OverlapCircle(pos, size / 4))
            {
                hp -= 10;
                genomeIndex += 3;
                genomeIndex %= genome[type].Count;
                GameObject c = (GameObject)Object.Instantiate(Resources.Load("Cell", typeof(GameObject)), new Vector3(pos.x, pos.y, 0), Quaternion.identity);
                c.transform.eulerAngles = new Vector3(0f, 0f, newCellDirection * 60f + transform.eulerAngles.z);
                c.GetComponent<Rigidbody2D>().velocity = gameObject.GetComponent<Rigidbody2D>().velocity;
                Cell cell = c.GetComponent<Cell>();
                cell.genome = CopyGenome();
                cell.Mutate();
                cell.type = newCellType;
                cell.hp = 10f;
                if(newCellType == 0)
                {
                    c.GetComponent<Rigidbody2D>().AddForce(-transform.up * 10f);
                }
                else if(newCellType == 1)
                {
                    c.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.8f, 0.3f);
                }
                else if(newCellType == 2)
                {
                    c.GetComponent<SpriteRenderer>().color = new Color(0.1f, 0.3f, 0.8f);
                }
                else if(newCellType == 3)
                {
                    c.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.3f, 0.1f);
                }
                else
                {
                    c.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.9f, 0.3f);
                }
            }
            else
            {
                genomeIndex += 3;
                genomeIndex %= genome[type].Count;
            }
        }
        // понижение клейкости
        else if(genome[type][genomeIndex] == 6)
        {
            glue -= 1f;
            genomeIndex++;
            genomeIndex %= genome[type].Count;
        }
        // повышение клейкости
        else if(genome[type][genomeIndex] == 7)
        {
            glue += 1f;
            genomeIndex++;
            genomeIndex %= genome[type].Count;
        }
    }

    // слипание клеток
    public void Link(Cell cell)
    {
        if(links.ContainsKey(cell)) return;
        if(links.Keys.Count > 5) return;
        if(cell.links.Keys.Count > 5) return;
        HingeJoint2D joint = gameObject.AddComponent<HingeJoint2D>();
        joint.connectedBody = cell.gameObject.GetComponent<Rigidbody2D>();
        joint.useLimits = true;
        JointAngleLimits2D limits = new JointAngleLimits2D { min = -15, max = 15 };
        joint.limits = limits;
        joint.breakForce = 20f;
        joint.breakTorque = 20f;
        links.Add(cell, joint);
        cell.links.Add(this, joint);
    }

    public void BreakLink(Cell c)
    {
        links.Remove(c);
        c.links.Remove(this);
    }

}
