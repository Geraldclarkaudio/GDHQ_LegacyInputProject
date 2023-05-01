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

        private GameInput _input;

        public InteractableZone interactableZone;
        public bool interacted;
        public bool _interactPress;
        public bool _interactHold;
        public bool _holdStop;

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
            _input = new GameInput();
            _input.Player.Enable(); // enable Player Input Map on start
            _input.Player.Interact.canceled += Interact_canceled;
            _input.Player.Interact.performed +=
                ctx =>
                {
                    if (ctx.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
                    {
                        _holdStop = false;
                        Debug.Log("HOLDING");
                        if(interactableZone != null)
                        {
                            _interactHold = true;
                            InteractableZone_onZoneInteractionComplete(interactableZone);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        Debug.Log("PRESS");
                        if (interactableZone != null)
                        {
                            _interactPress = true;
                            InteractableZone_onZoneInteractionComplete(interactableZone);
                        }
                        else
                        {
                            return;
                        }
                    }
                };

            _controller = GetComponent<CharacterController>();

            if (_controller == null)
                Debug.LogError("No Character Controller Present");

            _anim = GetComponentInChildren<Animator>();

            if (_anim == null)
                Debug.Log("Failed to connect the Animator");
        }

        private void Interact_canceled(InputAction.CallbackContext context)
        {
            _holdStop = true;
        }

        private void Update()
        {
            if (_canMove == true)
                CalcutateMovement();
        }

        private void CalcutateMovement()
        {
            _playerGrounded = _controller.isGrounded;
            var move = _input.Player.Movement.ReadValue<Vector2>();
            var direction = transform.forward * move.y;
            var velocity = direction * _speed;
            transform.Rotate(transform.up, move.x);
         
            _anim.SetFloat("Speed", Mathf.Abs(velocity.magnitude));


            if (_playerGrounded)
                velocity.y = 0f;
            if (!_playerGrounded)
            {
                velocity.y += -20f * Time.deltaTime;
            }
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
            _input.Player.Enable();
        }

        private void ReturnPlayerControl()
        {
            _model.SetActive(true);
            _canMove = true;
            _followCam.Priority = 10;
            _input.Player.Enable();
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

