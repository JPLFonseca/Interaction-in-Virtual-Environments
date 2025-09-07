using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Chunk
{
    // array tridimensional que armazena Block para cada posição dentro do chunk
    public Block[,,] chunkdata;
    public GameObject goChunk;
    public enum ChunkStatus { DRAW, DONE };
    public ChunkStatus status;
    Material material;


    public Chunk(Vector3 pos, Material material, List<Vector3> cave)
    {
        goChunk = new GameObject(World.CreateChunkName(pos));
        goChunk.transform.position = pos;
        this.material = material;
        BuildChunk(cave);
    }

    void BuildChunk(List<Vector3> cave)
    {
        chunkdata = new Block[World.chunkSize, World.chunkSize, World.chunkSize];

       

        for (int x = 0; x < World.chunkSize; x++)
        {
            for (int y = 0; y < World.chunkSize; y++)
            {
                for (int z = 0; z < World.chunkSize; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    int worldX = (int)goChunk.transform.position.x + x;
                    int worldY = (int)goChunk.transform.position.y + y;
                    int worldZ = (int)goChunk.transform.position.z + z;
                    int h = Utils.GenerateHeight(worldX, worldZ);
                    int hs = Utils.GenerateStoneHeight(worldX, worldZ);

                    if (cave.Contains(new Vector3(worldX, worldY, worldZ)))
                    {
                        //Debug.Log(pos); //CAVERNA
                        chunkdata[x, y, z] = new Block(Block.BlockType.AIR, pos, this, material);
                    }
                    else if (worldY < hs)
                        chunkdata[x, y, z] = new Block(Block.BlockType.STONE, pos, this, material);
                    else if(worldY == hs)
                    {
                        chunkdata[x, y, z] = new Block(Block.BlockType.GRAVEL, pos, this, material);
                    }
                    else if (worldY == h)
                        chunkdata[x, y, z] = new Block(Block.BlockType.GRASS, pos, this, material);
                    else if (worldY <= h) {
                        chunkdata[x, y, z] = new Block(Block.BlockType.DIRT, pos, this, material);
                    }


                    else if (worldY == h + 1)
                        if (Random.Range(0f, 1f) < 0.002f)
                            chunkdata[x, y, z] = new Block(Block.BlockType.PUMPKIN, pos, this, material);
                        else
                            chunkdata[x, y, z] = new Block(Block.BlockType.AIR, pos, this, material);
                    else
                        chunkdata[x, y, z] = new Block(Block.BlockType.AIR, pos, this, material);


                    
                }
            }
        }
        



                    status = ChunkStatus.DRAW;
    }

    // percorre todos os blocos no chunkdata. Para cada um destes chama o método block.draw
    public void DrawChunk()
    {
        for (int z = 0; z < World.chunkSize; z++)
            for (int y = 0; y < World.chunkSize; y++)
                for (int x = 0; x < World.chunkSize; x++)
                    chunkdata[x, y, z].Draw();

        // depois de todos os blocos terem criado as suas faces visíveis, pega nessas malhas e cria uma única malha
        // e apaga as faces individuais
        CombineQuads();
        MeshCollider collider = goChunk.AddComponent<MeshCollider>();
        collider.sharedMesh = goChunk.GetComponent<MeshFilter>().mesh;
        status = ChunkStatus.DONE;
    }

    public void Redraw()
    {
        GameObject.DestroyImmediate(goChunk.GetComponent<MeshFilter>());
        GameObject.DestroyImmediate(goChunk.GetComponent<MeshRenderer>());
        GameObject.DestroyImmediate(goChunk.GetComponent<Collider>());
        DrawChunk();
    }

    void CombineQuads()
    {
        MeshFilter[] meshFilters = goChunk.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int idx = 0; idx < meshFilters.Length; idx++)
        {
            combine[idx].mesh = meshFilters[idx].sharedMesh;
            combine[idx].transform = meshFilters[idx].transform.localToWorldMatrix;
        }

        MeshFilter mf = goChunk.AddComponent<MeshFilter>();
        mf.mesh = new Mesh();

        mf.mesh.CombineMeshes(combine);

        MeshRenderer renderer = goChunk.AddComponent<MeshRenderer>();
        renderer.material = material;

        foreach (Transform quad in goChunk.transform)
        {
            GameObject.Destroy(quad.gameObject);
        }
    }
}
