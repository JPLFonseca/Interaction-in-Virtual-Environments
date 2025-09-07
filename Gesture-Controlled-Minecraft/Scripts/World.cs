using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System;

public class World : MonoBehaviour
{
    public GameObject player;
    public Material material;
    public static int chunkSize = 16;
    public static int radius = 3;
    public static ConcurrentDictionary<string, Chunk> chunkDict;
    public static List<string> toRemove = new List<string>();
    Vector3 lastBuildPosition;
    bool drawing;

    public static string CreateChunkName(Vector3 v)
    {
        return (int)v.x + " " + (int)v.y + " " + (int)v.z;
    }

    IEnumerator BuildRecursiveWorld(Vector3 chunkPos, int rad, List<Vector3> cave)
    {
        BuildChunkAt(chunkPos, cave);
        yield return null;

        int x = (int)chunkPos.x;
        int y = (int)chunkPos.y;
        int z = (int)chunkPos.z;

        if (--rad < 0) yield break;

        Building(new Vector3(x, y, z + chunkSize), rad, cave);
        yield return null;
        Building(new Vector3(x, y, z - chunkSize), rad, cave);
        yield return null;
        Building(new Vector3(x + chunkSize, y, z), rad, cave);
        yield return null;
        Building(new Vector3(x - chunkSize, y, z), rad, cave);
        yield return null;
        Building(new Vector3(x, y + chunkSize, z), rad, cave);
        yield return null;
        Building(new Vector3(x, y - chunkSize, z), rad, cave);
        yield return null;

    }

    void Building(Vector3 chunkPos, int rad, List<Vector3> cave)
    {
        StartCoroutine(BuildRecursiveWorld(chunkPos, rad, cave));
    }

    void BuildChunkAt(Vector3 chunkPos, List<Vector3> cave)
    {
        string name = CreateChunkName(chunkPos);
        Chunk c;
        if (!chunkDict.TryGetValue(name, out c))
        {
            c = new Chunk(chunkPos, material, cave);
            c.goChunk.transform.parent = this.transform;
            chunkDict.TryAdd(c.goChunk.name, c);
        }
    }

    IEnumerator RemoveChunks()
    {
        for (int i = 0; i < toRemove.Count; i++)
        {
            string name = toRemove[i];
            Chunk c;
            if (chunkDict.TryGetValue(name, out c))
            {
                Destroy(c.goChunk);
                chunkDict.TryRemove(name, out c);
                yield return null;
            }
        }
    }

    IEnumerator DrawChunks()
    {
        drawing = true;
        foreach (KeyValuePair<string, Chunk> c in chunkDict)
        {
            if (c.Value.status == Chunk.ChunkStatus.DRAW)
            {
                c.Value.DrawChunk();
                yield return null;
            }
            if (c.Value.goChunk && Vector3.Distance(player.transform.position, c.Value.goChunk.transform.position) > chunkSize * radius)
                toRemove.Add(c.Key);
        }
        StartCoroutine(RemoveChunks()); //BUG QUANDO SE VOLTA A CHUNKS JA VISITADOS E APAGADOS
        drawing = false;
    }

    void Drawing()
    {
        StartCoroutine(DrawChunks());
    }

    Vector3 WhichChunk(Vector3 position)
    {
        Vector3 chunkPos = new Vector3();
        chunkPos.x = Mathf.Floor(position.x / chunkSize) * chunkSize;
        chunkPos.y = Mathf.Floor(position.y / chunkSize) * chunkSize;
        chunkPos.z = Mathf.Floor(position.z / chunkSize) * chunkSize;
        return chunkPos;
    }

    Vector3 GetCaveEdgePoint(Vector3 centerPosition)
    {
        System.Random r = new System.Random();

        int posX = r.Next((int)centerPosition.x - chunkSize, (int)centerPosition.x + chunkSize);
        int posZ = r.Next((int)centerPosition.z - chunkSize, (int)centerPosition.z + chunkSize);
        int posHeight = Utils.GenerateHeight(posX, posZ);
        int posY = posHeight - r.Next(6, posHeight - 1);

        return new Vector3(posX, posY, posZ);
    }

    // Torna o caminho traçado mais espesso
    List<Vector3> EnlargeCave(List<Vector3>  caveBlocks)
    {
        Vector3[] dirNeigh = {
        Vector3.up, Vector3.up + Vector3.right, Vector3.right + Vector3.down, Vector3.down, Vector3.left + Vector3.down, Vector3.left, Vector3.up + Vector3.left,
        Vector3.up + Vector3.forward, Vector3.up + Vector3.right + Vector3.forward, Vector3.right + Vector3.down + Vector3.forward, Vector3.down + Vector3.forward, Vector3.left + Vector3.down + Vector3.forward, Vector3.left + Vector3.forward, Vector3.up + Vector3.left + Vector3.forward,
        Vector3.up + Vector3.back, Vector3.up + Vector3.right + Vector3.back, Vector3.right + Vector3.down + Vector3.back, Vector3.down + Vector3.back, Vector3.left + Vector3.down + Vector3.back, Vector3.left + Vector3.back, Vector3.up + Vector3.left + Vector3.back,
        Vector3.up * 2, Vector3.up * 2 + Vector3.right, Vector3.up * 2 + Vector3.left,
        Vector3.right * 2, Vector3.right * 2 + Vector3.up, Vector3.right * 2 + Vector3.down,
        Vector3.down * 2, Vector3.down * 2 + Vector3.right, Vector3.down * 2 + Vector3.left,
        Vector3.left * 2, Vector3.left * 2 + Vector3.up, Vector3.left * 2 + Vector3.down
        };

        List<Vector3> sequence = new List<Vector3>(caveBlocks);

        foreach (Vector3 v in caveBlocks)
        {
            foreach (Vector3 dir in dirNeigh)
            {
                Vector3 neighPos = v + dir;
                sequence.Add(Utils.RoundVector3(neighPos));
            }
        }
        return sequence;
    }

    // Escolhe dois pontos aleatórios dentro do terreno para serem o início e o fim da gruta
    List<Vector3> GenerateCave(Vector3 centerPosition, int numMax = 100, float maxTurningAngle = 30f, float offset = 23456, float weightTarget = 0.22f, float smooth = 0.4f)
    {
        List<Vector3> caveBlocks = new List<Vector3>();

        Vector3 start = GetCaveEdgePoint(centerPosition);
        Vector3 end = GetCaveEdgePoint(centerPosition);

        Vector3 pos = start;
        Vector3 dirRef = (end - start).normalized;
        Vector3 dir = dirRef;

        for (int i = 0; i < numMax; i++)
        {
            float yawRotation = Utils.ConvertToAngle(Mathf.PerlinNoise(pos.x * smooth + offset, pos.z * smooth + offset), maxTurningAngle);
            float pitchRotation = Utils.ConvertToAngle(Mathf.PerlinNoise(pos.y * smooth + offset, pos.z * smooth + offset), maxTurningAngle);

            dir = Quaternion.AngleAxis(yawRotation, Vector3.up) * dirRef;
            dir = Quaternion.AngleAxis(pitchRotation, Vector3.right) * dir;
            dir = (1 - weightTarget) * dir + weightTarget * dirRef;

            pos += dir;

            if (Vector3.Distance(pos, end) < 1) break;

            caveBlocks.Add(Utils.RoundVector3(pos));
        }
        while (Vector3.Distance(pos, end) > 1.5f)
        {
            Vector3 dirToTarget = (end - pos).normalized;
            pos += dirToTarget;
            caveBlocks.Add(Utils.RoundVector3(pos));
        }
        return EnlargeCave(caveBlocks);
    }


    // Start is called before the first frame update
    void Start()
    {
        player.SetActive(false); // Desativa o jogador temporariamente
        chunkDict = new ConcurrentDictionary<string, Chunk>();
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;

        Vector3 ppos = player.transform.position;

        // Posiciona o jogador um pouco acima da superfície do terreno gerado
        player.transform.position = new Vector3(ppos.x, Utils.GenerateHeight(ppos.x, ppos.z) + 1, ppos.z);
        lastBuildPosition = player.transform.position;

        // Gera os dados para uma gruta
        List<Vector3> cave = GenerateCave(lastBuildPosition);
        Debug.Log(cave[0]);

        // Começa a construir o mundo à volta do jogador
        Building(WhichChunk(lastBuildPosition), radius, cave);
        Drawing(); // Manda desenhar os chunks recém-criados
        player.SetActive(true); // Reativa o jogador
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = player.transform.position - lastBuildPosition;
        if (movement.magnitude > chunkSize)
        {
            lastBuildPosition = player.transform.position;
            List<Vector3> cave = GenerateCave(lastBuildPosition);
            Building(WhichChunk(lastBuildPosition), radius, cave);
            Drawing();
        }
        if (!drawing) Drawing();
    }
}
