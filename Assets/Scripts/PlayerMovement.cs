using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float acceleration = 5f;
    [SerializeField] float deacceleration = 5f;
    [SerializeField] float rollAmount = 30f;
    [SerializeField] float rollSpeed = 5f;
    [SerializeField] float mouseSensitivity = 100f;
    [SerializeField] float normalSpeed = 10f;
    [SerializeField] float boostSpeed = 25f;
    [SerializeField] float boostAcceleration = 3f;
    
    [SerializeField] float normalFOV = 60f;
    [SerializeField] float boostFOV = 75f;
    [SerializeField] float fovSpeed = 5f;
    [SerializeField] CinemachineCamera virtualCamera;

    [SerializeField] ParticleSystem leftBoostVFX;
    [SerializeField] ParticleSystem rightBoostVFX;
    [SerializeField] Renderer shipRenderer;
    [SerializeField] Color idleEmissionColor = new Color(0.2f, 0.5f, 1f);
    [SerializeField] Color boostEmissionColor = new Color(0.5f, 2f, 5f);
    [SerializeField] float emissionSpeed = 5f;


    bool isBoosting;
    float currentSpeed;
    float yaw;
    float pitch;    
    Vector2 moveInput;
    Vector2 lookInput;
    float verticalInput;
    Vector3 currentVelocity;
    float currentRoll;
    bool isMouseActive = false;

    Material shipMat;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        
        lookInput = Vector2.zero;

        currentSpeed = normalSpeed;

        if(shipRenderer != null)
        {
            shipMat = shipRenderer.material;
        }
    }

    void Update()
    {
        Vector3 move = (transform.forward*moveInput.y + transform.up*verticalInput).normalized;
        yaw += lookInput.x*mouseSensitivity*Time.deltaTime;
        pitch -= lookInput.y*mouseSensitivity*Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -60f, 60f);

        float targetSpeed = isBoosting?boostSpeed:normalSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, boostAcceleration*Time.deltaTime);
        Vector3 targetVelocity = move * currentSpeed;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, (targetVelocity.magnitude > 0 ? acceleration : deacceleration)*Time.deltaTime);
        transform.position += currentVelocity*Time.deltaTime;

        //Smooth roll
        float targetRoll = -moveInput.x*rollAmount;
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, rollSpeed*Time.deltaTime);
        transform.rotation = Quaternion.Euler(pitch, yaw, currentRoll);

        if(virtualCamera != null)
        {
            float targetFOV = isBoosting?boostFOV:normalFOV;
            virtualCamera.Lens.FieldOfView = Mathf.Lerp(virtualCamera.Lens.FieldOfView, targetFOV, fovSpeed*Time.deltaTime);
        }

        HandleboostVFX();
    }
    public void OnMove(InputAction.CallbackContext context){
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnLift(InputAction.CallbackContext context)
    {
        verticalInput = context.ReadValue<float>();
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (!isMouseActive)
        {
            if(input.magnitude > 0.01f)
            {
                isMouseActive=true;
            }
            else
            {
                lookInput = Vector2.zero;
                return;
            }
        }
        lookInput = input;
    }
    public void OnBoost(InputAction.CallbackContext context)
    {
        isBoosting = context.ReadValue<float>() > 0.5f;
    }
    void HandleboostVFX()
        {
        float emissionRate = isBoosting?80f:10f;
        Color boostColor = isBoosting?Color.cyan:new Color(0.3f,0.5f,1f);
        float size = isBoosting?0.7f:0.4f;

        if(leftBoostVFX != null)
        {
            var emission = leftBoostVFX.emission;
            emission.rateOverTime = emissionRate;
            var main = leftBoostVFX.main;
            main.startColor = boostColor;
            var mainL = leftBoostVFX.main;
            mainL.startSize = size;
        }
        if(rightBoostVFX != null)
        {
            var emission = rightBoostVFX.emission;
            emission.rateOverTime = emissionRate;
            var main = rightBoostVFX.main;
            main.startColor = boostColor;
            var mainL = rightBoostVFX.main;
            mainL.startSize = size;
        }

        Color targetColor = isBoosting?boostEmissionColor:idleEmissionColor;
        if(shipMat != null)
        {
            Color current = shipMat.GetColor("_EmissionColor");
            Color newColor = Color.Lerp(current, targetColor, emissionSpeed * Time.deltaTime);
            shipMat.SetColor("_EmissionColor", newColor);
        }

        }
}
