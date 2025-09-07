using System.IO;
using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;
    private string path = ".\\Assets\\Mini First Person Controller\\Scripts\\values.txt";
    private float timeSinceLastCommand = 0f;
    private float commandInterval = 1f; // 1 second interval
    private string[] commands;
    private int currentCommandIndex = 0;
    public Vector2 gestureLookInput = Vector2.zero;

    Vector2 velocity;
    Vector2 frameVelocity;



    void Reset()
    {
        // Get the character from the FirstPersonMovement in parents.
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        // Lock the mouse cursor to the game screen.
        Cursor.lockState = CursorLockMode.Locked;

        
    }

    void Update()
    {

        if (File.Exists(path))
        {
            
            

            if (commands != null && commands.Length > 0)
            {
                timeSinceLastCommand += Time.fixedDeltaTime;

                if (timeSinceLastCommand >= commandInterval)
                {
                    if (currentCommandIndex < commands.Length)
                    {
                        ExecuteCommand(commands[currentCommandIndex]);
                        currentCommandIndex++;
                        timeSinceLastCommand = 0f;
                        
                    }
                }
            }
        }
        else {

            Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            // Get smooth velocity.
            // Combine mouse input with gesture input.
            Vector2 totalInput = mouseDelta + gestureLookInput;

            Vector2 rawFrameVelocity = Vector2.Scale(totalInput, Vector2.one * sensitivity);
            frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
            velocity += frameVelocity;
            velocity.y = Mathf.Clamp(velocity.y, -90, 90);

            // Rotate camera up-down and controller left-right from velocity.
            transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
            character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);

            // Reset gesture input for the next frame
            gestureLookInput = Vector2.zero;
        }
    }

    void ExecuteCommand(string command)
    {
        if(command == "Cima")
        {

            velocity.y += 30;
            velocity.y = Mathf.Clamp(velocity.y, -90, 90);

            Debug.Log("Olhou para cima");
        } else if(command == "Baixo")
        {
            velocity.y -= 30;
            velocity.y = Mathf.Clamp(velocity.y, -90, 90);
            
            Debug.Log("Olhou para baixo");
        }
        else if (command == "LadoDireito")
        {
            velocity.x += 50;
            velocity.y = Mathf.Clamp(velocity.y, -90, 90);

            Debug.Log("Olhou para a direita");
        }
        else if (command == "LadoEsquerdo")
        {
            velocity.x -= 50;
            velocity.y = Mathf.Clamp(velocity.y, -90, 90);


            Debug.Log("Olhou para a esquerda");
        }

        

        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);

        

    }

}
