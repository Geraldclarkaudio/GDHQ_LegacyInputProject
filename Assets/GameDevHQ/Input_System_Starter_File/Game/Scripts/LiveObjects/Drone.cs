using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private GameInput _droneInput;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
            _droneInput = new GameInput();
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            _droneInput.Drone.Enable();
            _droneInput.Drone.EscapeFlight.performed += EscapeFlight_performed;

            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void EscapeFlight_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            _inFlightMode = false;
            onExitFlightmode?.Invoke();
            ExitFlightMode();
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);            
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateMovementUpdate();
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
            var turn = _droneInput.Drone.Turn.ReadValue<float>();
            var rotation = transform.localRotation.eulerAngles;

            if (turn < 0)
            {
                rotation.y -= _speed / 3;
                transform.localRotation = Quaternion.Euler(rotation);
            }
            else if (turn > 0)
            {
                rotation.y += _speed / 3;
                transform.localRotation = Quaternion.Euler(rotation);
            }
        }

        private void CalculateMovementFixedUpdate()
        {
            var height = _droneInput.Drone.Height.ReadValue<float>();
            
            if (height > 0)
            {
                _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
            }
            if (height < 0)
            {
                _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
            }
        }

        private void CalculateTilt()
        {
            var forwardBack =  _droneInput.Drone.ForwardBackTilt.ReadValue<float>();
            var leftRight = _droneInput.Drone.LeftRightTilt.ReadValue<float>();
            transform.rotation = Quaternion.Euler(forwardBack * 30, transform.localRotation.eulerAngles.y, leftRight * 30);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
        }
    }
}
