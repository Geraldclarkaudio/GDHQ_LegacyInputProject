using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.LiveObjects;
using Cinemachine;
using UnityEngine.InputSystem;

namespace Game.Scripts.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        private CharacterController _controller;
        private Animator _anim;
        [SerializeField]
        private float _speed = 5.0f;
        private bool _playerGrounded;
        [SerializeField]
        private Detonator _detonator;
        private bool _canMove = true;
        [SerializeField]
        private CinemachineVirtualCamera _followCam;
        [SerializeField]
        private GameObject _model;

        private InteractableZone _interactableZone;

        private GameInput _input;

        public delegate void PressInteract();
        public static event PressInteract onPressInteract;

        public delegate void HoldInteract();
        public static event HoldInteract onHoldInteract;

        public bool _holdStop;
        public bool _interactHold;
        public bool _interactPress;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
            Laptop.onHackComplete += ReleasePlayerControl;
            Laptop.onHackEnded += ReturnPlayerControl;
            Forklift.onDriveModeEntered += ReleasePlayerControl;
            Forklift.onDriveModeExited += ReturnPlayerControl;
            Forklift.onDriveModeEntered += HidePlayer;
            Drone.OnEnterFlightMode += ReleasePlayerControl;
            Drone.onExitFlightmode += ReturnPlayerControl;
        } 

        private void Start()
        {
            _controller = GetComponent<CharacterController>();

            if (_controller == null)
                Debug.LogError("No Character Controller Present");

            _anim = GetComponentInChildren<Animator>();

            if (_anim == null)
                Debug.Log("Failed to connect the Animator");
            _interactableZone = GameObject.FindObjectOfType<InteractableZone>();

            _input = new GameInput();
            _input.Player.Enable();
            _input.Player.Interact.performed += Interacted;
        }

        public void Interacted(InputAction.CallbackContext context)
        {
            if(context.interaction is UnityEngine.InputSystem.Interactions.PressInteraction)
            {
                if (onPressInteract != null)
                {
                    onPressInteract();
                }
            }
            else if(context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
            {
                if(onHoldInteract != null)
                {
                    onHoldInteract();
                }
            }
        }

        private void Update()
        {
            if (_canMove == true)
                CalcutateMovement();
        }

        private void CalcutateMovement()
        {
            var move = _input.Player.Movement.ReadValue<float>();
            var rot = _input.Player.Rotate.ReadValue<float>();
            transform.Rotate(transform.up, rot);
            var direction = transform.forward * move;
            var velocity = direction * _speed;
            _anim.SetFloat("Speed", Mathf.Abs(velocity.magnitude));

            _controller.Move(velocity * Time.deltaTime);                 
        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            switch(zone.GetZoneID())
            {
                case 1: //place c4
                    _detonator.Show();
                    break;
                case 2: //Trigger Explosion
                    TriggerExplosive();
                    break;
            }
        }

        private void ReleasePlayerControl()
        {
            _canMove = false;
            _followCam.Priority = 9;
        }

        private void ReturnPlayerControl()
        {
            _model.SetActive(true);
            _canMove = true;
            _followCam.Priority = 10;
        }

        private void HidePlayer()
        {
            _model.SetActive(false);
        }
               
        private void TriggerExplosive()
        {
            _detonator.TriggerExplosion();
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
            Laptop.onHackComplete -= ReleasePlayerControl;
            Laptop.onHackEnded -= ReturnPlayerControl;
            Forklift.onDriveModeEntered -= ReleasePlayerControl;
            Forklift.onDriveModeExited -= ReturnPlayerControl;
            Forklift.onDriveModeEntered -= HidePlayer;
            Drone.OnEnterFlightMode -= ReleasePlayerControl;
            Drone.onExitFlightmode -= ReturnPlayerControl;
        }

    }
}

