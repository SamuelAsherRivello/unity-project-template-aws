using RMC.Backend.Baas;
using RMC.Backend.Baas.Aws;
using RMC.MyProject.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RMC.MyProject.Scenes
{
    //  Enums -----------------------------------------
    public enum AccountState
    {
        NULL,
        SigningUp,
        SignedUp,
        SigningIn,
        SignedIn,
        SigningOut,
        SignedOut,
    }
    
    /// <summary>
    /// Main entry point for the Scene.
    /// </summary>
    public class Scene01_Intro : MonoBehaviour
    {
        
        //  Properties ------------------------------------
        public HudUI HudUI { get { return _hudUI; } }
        private AccountState CurrentAccountState
        {
            get
            {
                return _currentAccountState;
            }

            set
            {
                // Set value
                _currentAccountState = value;
            
                // Show value in UI
                string extraInfo = "";
                if (_currentAccountState == AccountState.SignedIn)
                {
                    extraInfo = " (Token Is Known)";
                }
            
                //HACK: We put the new AWS info into the existing SCORE text
                HudUI.SetScore($"{_currentAccountState}{extraInfo} :AWS");
            }
        
        }


        //  Fields ----------------------------------------
        [Header("UI")]
        [SerializeField]
        private HudUI _hudUI;

        [Header("Player")]
        [SerializeField]
        private Rigidbody _playerRigidBody;
        
        [SerializeField]
        private float _playerMoveSpeed = 4;
        
        [SerializeField]
        private float _playerJumpSpeed = 5;

        // Input
        private InputAction _moveInputAction;
        private InputAction _jumpInputAction;
        private InputAction _resetInputAction;
        
        // AWS
        private AccountState _currentAccountState = AccountState.NULL;
        
        //  Unity Methods ---------------------------------
        protected void Start()
        {
            Debug.Log($"{GetType().Name}.Start()");
            
            // Input
            _moveInputAction = InputSystem.actions.FindAction("Move");
            _jumpInputAction = InputSystem.actions.FindAction("Jump");
            _resetInputAction = InputSystem.actions.FindAction("Reset");
            
            // UI
            HudUI.SetLives("Lives: 003");
            HudUI.SetInstructions("Instructions: WASD/Arrows, Spacebar, R");
            HudUI.SetTitle(SceneManager.GetActiveScene().name);
            
            // AWS
            CurrentAccountState = AccountState.NULL;
            CheckAWSAccount();

        }


        protected void Update()
        {
            HandleUserInput();
            CheckPlayerFalling();
        }


        //  Methods ---------------------------------------
        private async void CheckAWSAccount()
        {
            //TODO: Optional, Add Unity UI to allow user to type in this info. 
            string email = "test.email@test.email.com";
            string password = "test.password!AND@#%123";
            string nickname = "test.nickname";
            
            
            //////////////////////////////////////
            // Accounts - System
            //////////////////////////////////////
            
            // UI
            CurrentAccountState = AccountState.SigningUp;
            
            // Observe
            Jaws.Instance.OnInitialized.AddListener((jaws) => { Debug.Log("@@ Jaws.OnInitialized()"); });
            Jaws.Instance.Accounts.OnUserCreated.AddListener((jaws) => { Debug.Log("@@ Jaws.OnUserCreated()"); });
            Jaws.Instance.Accounts.OnUserDeleted.AddListener((jaws) => { Debug.Log("@@ Jaws.OnUserDeleted()"); });
            Jaws.Instance.Accounts.OnUserSignedIn.AddListener((jaws) => { Debug.Log("@@ Jaws.OnUserSignedIn()"); });
            Jaws.Instance.Accounts.OnUserSignedOut.AddListener((jaws) => { Debug.Log("@@ Jaws.OnUserSignedOut()"); });

            // Init
            await Jaws.Instance.InitializeAsync();

            if (!Jaws.Instance.Accounts.HasUser())
            {
                // UI
                CurrentAccountState = AccountState.SigningUp;
                
                //
                var userCreateResponse = await Jaws.Instance.Accounts.UserCreateAsync(email, password, nickname);
                
                LogResponse($"Jaws #1: userCreateResponse", userCreateResponse);
                
                if (userCreateResponse.IsSuccess)
                {
                    CurrentAccountState = AccountState.SignedUp;
                }

                // UI
                CurrentAccountState = AccountState.SigningIn;
                
                //
                var userSignInResponse = await Jaws.Instance.Accounts.UserSignInAsync(email, password);
       
                LogResponse($"Jaws #2: userSignInResponse", userSignInResponse);
                
                if (userSignInResponse.IsSuccess)
                {
                    // UI
                    CurrentAccountState = AccountState.SignedIn;
                }
            }
            
            //
            var user = Jaws.Instance.Accounts.GetUser();
            Debug.LogWarning($"Email: {user.Email}");
            Debug.LogWarning($"TokenId: {user.TokenId}");
            
        }

        
        private void LogResponse(string message, Response response)
        {
            Debug.Log($"{message}. IsSuccess = {response.IsSuccess}");
            
            if (!response.IsSuccess)
            {
                Debug.LogError($"\t{response.ErrorMessage}");
            }
        }


        private void HandleUserInput()
        {
            Vector2 moveInputVector2 = _moveInputAction.ReadValue<Vector2>();
            
            if (moveInputVector2.magnitude > 0.1f)
            {
                Vector3 moveInputVector3 = new Vector3
                (
                    moveInputVector2.x,
                    0,
                    moveInputVector2.y
                );
                
                // Move with arrow keys / WASD / gamepad
                _playerRigidBody.AddForce(moveInputVector3 * _playerMoveSpeed, ForceMode.Acceleration);
            }

            if (_jumpInputAction.WasPerformedThisFrame())
            {
                // Jump with spacebar / gamepad
                _playerRigidBody.AddForce(Vector3.up * _playerJumpSpeed, ForceMode.Impulse);
            }
            
            if (_resetInputAction.IsPressed())
            {
                // Reload the current scene with R key 
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        
        private void CheckPlayerFalling()
        {
            if (_playerRigidBody.transform.position.y < -5)
            {
                // Reload the current scene if character falls off Floor
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        //  Event Handlers --------------------------------
    }
}