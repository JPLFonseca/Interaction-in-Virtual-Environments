using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Gerar o terreno. Usa Fractal Brownian Motion (fBM), versão mais aprimorada do ruído de Perlin
public class Utils : MonoBehaviour
{
    static float smooth = 0.002f;
    static float smooth3D = 0.03f;
    static int maxHeight = 150;

    // nº de camadas de ruído a serem sobrepostas
    static int octaves = 6;
    static float persistence = 0.7f;
    static float offset = 32000f;

    // função usada para gerar a altura do terreno
    public static int GenerateHeight(float x, float z)
    {
        return (int)Map(0, maxHeight, 0, 1, fBM(x * smooth, z * smooth, octaves, persistence));
    }


    // função usada para gerar a altura da camada de pedra. Usa fBm para criar um padrão distinto
    // função para converter o valor entre 0 e 1 que sai do fBM em valor para a escala do mundo do jogo
    public static int GenerateStoneHeight(float x, float z)
    {
        return (int)Map(0, maxHeight - 6, 0, 1, fBM(x * smooth, z * smooth, octaves - 2, 1.8f * persistence)); //REMOVER 1.8
    }

    
    
    static float Map(float newmin, float newmax, float orimin, float orimax, float val)
    {
        return Mathf.Lerp(newmin, newmax, Mathf.InverseLerp(orimin, orimax, val));
    }

    public static float fBM3D(float x, float y, float z, int octaves, float persistence)
    {
        float xy = fBM(x * smooth3D, y * smooth3D, octaves, persistence);
        float yx = fBM(y * smooth3D, x * smooth3D, octaves, persistence);
        float xz = fBM(x * smooth3D, z * smooth3D, octaves, persistence);
        float zx = fBM(z * smooth3D, x * smooth3D, octaves, persistence);
        float yz = fBM(y * smooth3D, z * smooth3D, octaves, persistence);
        float zy = fBM(z * smooth3D, y * smooth3D, octaves, persistence);

        return (xy + yx + xz + zx + yz + zy) / 6;
    }

    static float fBM(float x, float z, int octaves, float persistence)
    {
        float total = 0;
        float amplitude = 1;
        float frequency = 1;
        float maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise((x + offset) * frequency, (z + offset) * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }

    public static float ConvertToAngle(float p, float maxTurningAngle)
    {
        return (2 * p - 1) * maxTurningAngle;
    }

    // arredonda uma função simples para um componente Vector3 para o número inteiro mais próximo
    public static Vector3 RoundVector3(Vector3 pos)
    {
        float posX = Mathf.Floor(pos.x + 0.5f);
        float posY = Mathf.Floor(pos.y + 0.5f);
        float posZ = Mathf.Floor(pos.z + 0.5f);

        return new Vector3(posX, posY, posZ);
    }



    // Update is called once per frame
    void Update()
    {

    }
}
