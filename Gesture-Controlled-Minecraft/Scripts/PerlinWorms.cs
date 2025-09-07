using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Algoritmo de geração procedimental. É usado para definir as grutas
public class PerlinWorms : MonoBehaviour
{
    [SerializeField] private Transform cubeStart;
    [SerializeField] private Transform cubeEnd;
    [SerializeField] [Range(0.01f, 0.4f)] private float smooth;
    [SerializeField] private float offset = 23456;
    [SerializeField] private float maxTurningAngle = 30f;
    private Vector3 start, end;
    [SerializeField] private List<Vector3> wormSequence;
    [SerializeField] [Range(0f, 1f)] private float weightTarget;
    private Vector3[] dirNeigh = { Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
    [SerializeField] private bool randomWalk = false;

    IEnumerator EnlargeWorm()
    {
        foreach (Vector3 v in wormSequence)
        {
            foreach (Vector3 dir in dirNeigh)
            {
                Vector3 neighPos = v + dir;
                GameObject newgo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newgo.transform.position = neighPos;
                newgo.transform.parent = this.transform;
                newgo.transform.name = neighPos.x + " " + neighPos.y + " " + neighPos.z;
                newgo.GetComponent<MeshRenderer>().material.color = Color.cyan;
                yield return null;
            }
        }
    }

    void InitAndBuildWorld()
    {
        cubeStart.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
        cubeEnd.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        start = cubeStart.position;
        end = cubeEnd.position;
        wormSequence = PerlinWorm(start, end, 75);
    }

    List<Vector3> PerlinWorm(Vector3 start, Vector3 end, int numMax = 100)
    {
        List<Vector3> sequence = new List<Vector3>();
        Vector3 pos = start; // Posição atual da minhoca
        Vector3 dirRef = (end - start).normalized; // A direção ideal (linha reta para o fim)
        Vector3 dir = dirRef; // A direção atual da minhoca

        for (int i = 0; i < numMax; i++)
        {
            // 1. Calcular as rotações com base no Ruído Perlin
            float yawRotation = ConvertToAngle(Mathf.PerlinNoise(pos.x * smooth + offset, pos.z * smooth + offset));
            float pitchRotation = ConvertToAngle(Mathf.PerlinNoise(pos.y * smooth + offset, pos.z * smooth + offset));

            // 2. Aplicar as rotações para desviar a direção
            if (randomWalk)
                dir = Quaternion.AngleAxis(yawRotation, Vector3.up) * dir;
            else
                dir = Quaternion.AngleAxis(yawRotation, Vector3.up) * dirRef;
            dir = Quaternion.AngleAxis(pitchRotation, Vector3.right) * dir;
            // 3. Puxar a direção de volta na direção do alvo
            dir = (1 - weightTarget) * dir + weightTarget * dirRef;

            // 4. Mover a minhoca e guardar a sua posição
            pos += dir;

            if (Vector3.Distance(pos, end) < 1) break;

            sequence.Add(RoundVector3(pos));
        }
        while (Vector3.Distance(pos, end) > 1.5f)
        {
            Vector3 dirToTarget = (end - pos).normalized;
            pos += dirToTarget;
            sequence.Add(RoundVector3(pos));
        }

        return sequence;
    }

    IEnumerator RenderWorm()
    {
        foreach (Vector3 v in wormSequence)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = v;
            go.transform.parent = this.transform;
            go.transform.name = v.x + " " + v.y + " " + v.z;
            go.GetComponent<MeshRenderer>().material.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
        }
        StartCoroutine(EnlargeWorm());
    }

    float ConvertToAngle(float p)
    {
        return (2 * p - 1) * maxTurningAngle;
    }

    Vector3 RoundVector3(Vector3 pos)
    {
        float posX = Mathf.Floor(pos.x + 0.5f);
        float posY = Mathf.Floor(pos.y + 0.5f);
        float posZ = Mathf.Floor(pos.z + 0.5f);

        return new Vector3(posX, posY, posZ);
    }

    // Start is called before the first frame update
    void Start()
    {
        InitAndBuildWorld();
        StartCoroutine(RenderWorm());
    }

    // Update is called once per frame
    void Update()
    {

    }
}