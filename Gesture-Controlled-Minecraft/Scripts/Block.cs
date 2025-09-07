using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    enum Cubeside { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK };
    public enum BlockType { GRASS, DIRT, STONE, PUMPKIN, GRAVEL, AIR , CRACK1, CRACK2, CRACK3, CRACK4, CRACK5,FLOWER};

    BlockType bType;
    Material material;
    Chunk owner;
    Vector3 pos;
    bool isSolid;

    public BlockType health;
    int currentHealth;
    int[] BlockHealthMax = { 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0 };

    static Vector2 GrassSide_LBC = new Vector2(3f, 15f) / 16;
    static Vector2 GrassTop_LBC = new Vector2(2f, 6f) / 16;
    static Vector2 Dirt_LBC = new Vector2(2f, 15f) / 16;
    static Vector2 Stone_LBC = new Vector2(1f, 15f) / 16;
    static Vector2 PumpkinSide_LBC = new Vector2(6f, 8f) / 16;
    static Vector2 PumpkinTop_LBC = new Vector2(6f, 9f) / 16;
    static Vector2 PumpkinFront_LBC = new Vector2(7f, 8f) / 16;
    static Vector2 Gravel_LBC = new Vector2(0f, 14f) / 16;
    static Vector2 Crack1_LBC = new Vector2(1f, 0f) / 16;
    static Vector2 Crack2_LBC = new Vector2(4f, 0f) / 16;
    static Vector2 Crack3_LBC = new Vector2(6f, 0f) / 16;
    static Vector2 Crack4_LBC = new Vector2(6f, 0f) / 16;
    static Vector2 Crack5_LBC = new Vector2(7f, 0f) / 16;
    

    Vector2[,] blockUVs =
    {
        {GrassSide_LBC, GrassSide_LBC + new Vector2(1f, 0f)/16, GrassSide_LBC + new Vector2(0f, 1f)/16, GrassSide_LBC + new Vector2(1f, 1f)/16},
        {Dirt_LBC, Dirt_LBC + new Vector2(1f, 0f)/16, Dirt_LBC + new Vector2(0f, 1f)/16, Dirt_LBC + new Vector2(1f, 1f)/16},
        {Stone_LBC, Stone_LBC + new Vector2(1f, 0f)/16, Stone_LBC + new Vector2(0f, 1f)/16, Stone_LBC + new Vector2(1f, 1f)/16},
        {PumpkinSide_LBC, PumpkinSide_LBC + new Vector2(1f, 0f)/16, PumpkinSide_LBC + new Vector2(0f, 1f)/16, PumpkinSide_LBC + new Vector2(1f, 1f)/16},
        {Gravel_LBC, Gravel_LBC + new Vector2(1f, 0f)/16,Gravel_LBC + new Vector2(0f, 1f)/16,Gravel_LBC + new Vector2(1f, 1f)/16},
        {GrassTop_LBC, GrassTop_LBC + new Vector2(1f, 0f)/16, GrassTop_LBC + new Vector2(0f, 1f)/16, GrassTop_LBC + new Vector2(1f, 1f)/16},
        {PumpkinTop_LBC, PumpkinTop_LBC + new Vector2(1f, 0f)/16, PumpkinTop_LBC + new Vector2(0f, 1f)/16, PumpkinTop_LBC + new Vector2(1f, 1f)/16},
        {PumpkinFront_LBC, PumpkinFront_LBC + new Vector2(1f, 0f)/16, PumpkinFront_LBC + new Vector2(0f, 1f)/16, PumpkinFront_LBC + new Vector2(1f, 1f)/16},
        {Crack1_LBC, Crack1_LBC + new Vector2(1f, 0f)/16, Crack1_LBC + new Vector2(0f, 1f)/16, Crack1_LBC + new Vector2(1f, 1f)/16},
        {Crack2_LBC, Crack2_LBC + new Vector2(1f, 0f)/16, Crack2_LBC + new Vector2(0f, 1f)/16, Crack2_LBC + new Vector2(1f, 1f)/16},
        {Crack3_LBC, Crack3_LBC + new Vector2(1f, 0f)/16, Crack3_LBC + new Vector2(0f, 1f)/16, Crack3_LBC + new Vector2(1f, 1f)/16},
        {Crack4_LBC, Crack4_LBC + new Vector2(1f, 0f)/16, Crack4_LBC + new Vector2(0f, 1f)/16, Crack4_LBC + new Vector2(1f, 1f)/16},
        {Crack5_LBC, Crack5_LBC + new Vector2(1f, 0f)/16, Crack5_LBC + new Vector2(0f, 1f)/16, Crack5_LBC + new Vector2(1f, 1f)/16}

    };

    public Block(BlockType bType, Vector3 pos, Chunk owner, Material material)
    {
        this.pos = pos;
        this.owner = owner;
        this.material = material;
        SetType(bType);
    }

    public void SetType(BlockType bType)
    {
        this.bType = bType;
        if (bType == BlockType.AIR)
            this.isSolid = false;
        else
            this.isSolid = true;

        health = BlockType.CRACK1;
        currentHealth = BlockHealthMax[(int)bType];
    }

    public BlockType getType()
    {
        return bType;
    }

    public bool hitBlock()
    {
        if (currentHealth == -1) return false;
        currentHealth--;
        health++;
        if (currentHealth <= 0)
        {
            bType = BlockType.AIR;
            isSolid = false;
            health = BlockType.CRACK1;
            owner.Redraw();
            return true;
        }
        owner.Redraw();
        return false;
    }

    void CreateQuad(Cubeside side)
    {
        Mesh mesh = new Mesh();
        List<Vector2> suvs = new List<Vector2>();


        Vector3 v0 = new Vector3(-0.5f, -0.5f, 0.5f);
        Vector3 v1 = new Vector3(0.5f, -0.5f, 0.5f);
        Vector3 v2 = new Vector3(0.5f, -0.5f, -0.5f);
        Vector3 v3 = new Vector3(-0.5f, -0.5f, -0.5f);
        Vector3 v4 = new Vector3(-0.5f, 0.5f, 0.5f);
        Vector3 v5 = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 v6 = new Vector3(0.5f, 0.5f, -0.5f);
        Vector3 v7 = new Vector3(-0.5f, 0.5f, -0.5f);

        Vector2 uv00 = new Vector2(0, 0);
        Vector2 uv01 = new Vector2(0, 1);
        Vector2 uv10 = new Vector2(1, 0);
        Vector2 uv11 = new Vector2(1, 1);

        if (bType == BlockType.GRASS && side == Cubeside.TOP)
        {
            uv00 = blockUVs[5, 0];
            uv10 = blockUVs[5, 1];
            uv01 = blockUVs[5, 2];
            uv11 = blockUVs[5, 3];
        }
        else if (bType == BlockType.GRASS && side == Cubeside.BOTTOM)
        {
            uv00 = blockUVs[1, 0];
            uv10 = blockUVs[1, 1];
            uv01 = blockUVs[1, 2];
            uv11 = blockUVs[1, 3];
        }
        else if (bType == BlockType.PUMPKIN && side == Cubeside.TOP)
        {
            uv00 = blockUVs[6, 0];
            uv10 = blockUVs[6, 1];
            uv01 = blockUVs[6, 2];
            uv11 = blockUVs[6, 3];
        }
        else if (bType == BlockType.PUMPKIN && side == Cubeside.FRONT)
        {
            uv00 = blockUVs[7, 0];
            uv10 = blockUVs[7, 1];
            uv01 = blockUVs[7, 2];
            uv11 = blockUVs[7, 3];
        } else
        {
            uv00 = blockUVs[(int)bType, 0];
            uv10 = blockUVs[(int)bType, 1];
            uv01 = blockUVs[(int)bType, 2];
            uv11 = blockUVs[(int)bType, 3];
        }
        
        if(currentHealth != 3) { 
        suvs.Add(blockUVs[(int)(health + 1), 3]);
        suvs.Add(blockUVs[(int)(health + 1), 2]);
        suvs.Add(blockUVs[(int)(health + 1), 0]);
        suvs.Add(blockUVs[(int)(health + 1), 1]);
        }


        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        int[] triangles = new int[] { 3, 1, 0, 3, 2, 1 };
        Vector2[] uvs = new Vector2[] { uv11, uv01, uv00, uv10 };

        switch (side)
        {
            case Cubeside.FRONT:
                vertices = new Vector3[] { v4, v5, v1, v0 };
                normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                break;

            case Cubeside.BOTTOM:
                vertices = new Vector3[] { v0, v1, v2, v3 };
                normals = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
                break;

            case Cubeside.TOP:
                vertices = new Vector3[] { v7, v6, v5, v4 };
                normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                break;

            case Cubeside.LEFT:
                vertices = new Vector3[] { v7, v4, v0, v3 };
                normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
                break;

            case Cubeside.RIGHT:
                vertices = new Vector3[] { v5, v6, v2, v1 };
                normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                break;

            case Cubeside.BACK:
                vertices = new Vector3[] { v6, v7, v3, v2 };
                normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                break;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.SetUVs(1, suvs);

        mesh.RecalculateBounds();

        GameObject quad = new GameObject("Quad");
        quad.transform.position = this.pos;
        quad.transform.parent = owner.goChunk.transform;

        MeshFilter mf = quad.AddComponent<MeshFilter>();
        mf.mesh = mesh;
    }

    int ConvertToLocalIndex(int i)
    {
        if (i == -1)
            return World.chunkSize - 1;
        if (i == World.chunkSize)
            return 0;
        return i;
    }

    bool HasSolidNeighbour(int x, int y, int z)
    {
        Block[,,] chunkdata;

        if (x < 0 || x >= World.chunkSize || y < 0 || y >= World.chunkSize || z < 0 || z >= World.chunkSize)
        {
            Vector3 neighChunkPos = owner.goChunk.transform.position + new Vector3(
                (x - (int)pos.x) * World.chunkSize,
                (y - (int)pos.y) * World.chunkSize,
                (z - (int)pos.z) * World.chunkSize);
            string chunkName = World.CreateChunkName(neighChunkPos);

            x = ConvertToLocalIndex(x);
            y = ConvertToLocalIndex(y);
            z = ConvertToLocalIndex(z);

            Chunk neighChunk;
            if (World.chunkDict.TryGetValue(chunkName, out neighChunk))
                chunkdata = neighChunk.chunkdata;
            else
                return false;

        }
        else
            chunkdata = owner.chunkdata;

        try
        {
            return chunkdata[x, y, z].isSolid;
        }
        catch (System.IndexOutOfRangeException ex)
        {
            return false;
        }

    }

    public void Draw()
    {
        if (bType == BlockType.AIR) return;

        if (!HasSolidNeighbour((int)pos.x - 1, (int)pos.y, (int)pos.z))
            CreateQuad(Cubeside.LEFT);

        if (!HasSolidNeighbour((int)pos.x + 1, (int)pos.y, (int)pos.z))
            CreateQuad(Cubeside.RIGHT);

        if (!HasSolidNeighbour((int)pos.x, (int)pos.y - 1, (int)pos.z))
            CreateQuad(Cubeside.BOTTOM);

        if (!HasSolidNeighbour((int)pos.x, (int)pos.y + 1, (int)pos.z))
            CreateQuad(Cubeside.TOP);

        if (!HasSolidNeighbour((int)pos.x, (int)pos.y, (int)pos.z - 1))
            CreateQuad(Cubeside.BACK);

        if (!HasSolidNeighbour((int)pos.x, (int)pos.y, (int)pos.z + 1))
            CreateQuad(Cubeside.FRONT);

    }
}
