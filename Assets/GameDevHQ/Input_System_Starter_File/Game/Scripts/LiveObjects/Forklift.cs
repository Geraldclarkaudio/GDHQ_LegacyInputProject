using System;
using UnityEngine;
using Cinemachine;

namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        [SerializeField]
        private GameObject _lift, _steeringWheel, _leftWheel, _rightWheel, _rearWheels;
        [SerializeField]
        private Vector3 _liftLowerLimit, _liftUpperLimit;
        [SerializeField]
        private float _speed = 5f, _liftSpeed = 1f;
        [SerializeField]
        private CinemachineVirtualCamera _forkliftCam;
        [SerializeField]
        private GameObject _driverModel;
        private bool _inDriveMode = false;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onDriveModeEntered;
        public static event Action onDriveModeExited;

        private GameInput _forliftInput;

        private void OnEnable()
        {
            _forliftInput = new GameInput();
            _forliftInput.Forklift.Enable();
            _forliftInput.Forklift.Exit.performed += Exit_performed;
            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
        }

        private void Exit_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ExitDriveMode();   
        }

        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode !=true && zone.GetZoneID() == 5) //Enter ForkLift
            {
                _inDriveMode = true;
                _forkliftCam.Priority = 11;
                onDriveModeEntered?.Invoke();
                _driverModel.SetActive(true);
                _interactableZone.CompleteTask(5);
            }
        }

        private void ExitDriveMode()
        {
            _inDriveMode = false;
            _forkliftCam.Priority = 9;            
            _driverModel.SetActive(false);
            onDriveModeExited?.Invoke();
            
        }

        private void Update()
        {
            if (_inDriveMode == true)
            {
                LiftControls();
                CalcutateMovement();
            }

        }

        private void CalcutateMovement()
        {
            var drive = _forliftInput.Forklift.Drive.ReadValue<float>();
            var rot = _forliftInput.Forklift.Rotate.ReadValue<float>();
            var direction = new Vector3(0, 0, drive);
            var velocity = direction * _speed;

            transform.Translate(velocity * Time.deltaTime);

            if (rot > 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y += drive * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
            else if(rot < 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y -= drive * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }

        }

        private void LiftControls()
        {
            var lift = _forliftInput.Forklift.Lift.ReadValue<float>();
            if(lift > 0)
            {
                LiftUpRoutine();
            }
            else if(lift < 0)
            {
                LiftDownRoutine();
            }
        }

        private void LiftUpRoutine()
        {
            if (_lift.transform.localPosition.y < _liftUpperLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y += Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y >= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftUpperLimit;
        }

        private void LiftDownRoutine()
        {
            if (_lift.transform.localPosition.y > _liftLowerLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y <= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftLowerLimit;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;
        }

    }
}