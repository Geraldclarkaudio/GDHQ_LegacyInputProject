using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.UI;
using Game.Scripts.Player;

namespace Game.Scripts.LiveObjects
{
    public class InteractableZone : MonoBehaviour
    {
        private enum ZoneType
        {
            Collectable,
            Action,
            HoldAction
        }

        private enum KeyState
        {
            Press,
            PressHold
        }
        [SerializeField]
        private ZoneType _zoneType;
        [SerializeField]
        private int _zoneID;
        [SerializeField]
        private int _requiredID;
        [SerializeField]
        [Tooltip("Press the (---) Key to .....")]
        private string _displayMessage;
        [SerializeField]
        private GameObject[] _zoneItems;
        private bool _inZone = false;
        private bool _itemsCollected = false;
        private bool _actionPerformed = false;
        [SerializeField]
        private Sprite _inventoryIcon;
        [SerializeField]
        private KeyCode _zoneKeyInput;
        [SerializeField]
        private KeyState _keyState;
        [SerializeField]
        private GameObject _marker;

        private Game.Scripts.Player.Player player;
        
        private bool _inHoldState = false;

        private static int _currentZoneID = 0;
        public static int CurrentZoneID
        { 
            get 
            { 
               return _currentZoneID; 
            }
            set
            {
                _currentZoneID = value; 
                         
            }
        }


        public static event Action<InteractableZone> onZoneInteractionComplete;
        public static event Action<int> onHoldStarted;
        public static event Action<int> onHoldEnded;

        private void Start()
        {
            player = GameObject.FindObjectOfType<Player.Player>();
        }
        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += SetMarker;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _currentZoneID > _requiredID)
            {
                switch (_zoneType)
                {
                    case ZoneType.Collectable:
                        if (_itemsCollected == false)
                        {
                            _inZone = true;
                            player.interactableZone = this;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the {_zoneKeyInput.ToString()} key to collect");
                        }
                        break;

                    case ZoneType.Action:
                        if (_actionPerformed == false)
                        {
                            _inZone = true;
                            player.interactableZone = this;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the {_zoneKeyInput.ToString()} key to perform action");
                        }
                        break;

                    case ZoneType.HoldAction:
                        _inZone = true;
                        player.interactableZone = this;
                        if (_displayMessage != null)
                        {
                            string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                            UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                        }
                        else
                            UIManager.Instance.DisplayInteractableZoneMessage(true, $"Hold the {_zoneKeyInput.ToString()} key to perform action");
                        break;
                }
            }
        }
     
        private void Update()
        {
            if (_inZone == true)
            {
                #region Old Inputs
                //if (Input.GetKeyDown(_zoneKeyInput) && _keyState != KeyState.PressHold)
                //{
                //    //press
                //    switch (_zoneType)
                //    {
                //        case ZoneType.Collectable:
                //            if (_itemsCollected == false)
                //            {
                //                CollectItems();
                //                _itemsCollected = true;
                //                UIManager.Instance.DisplayInteractableZoneMessage(false);
                //            }
                //            break;

                //        case ZoneType.Action:
                //            if (_actionPerformed == false)
                //            {
                //                PerformAction();
                //                _actionPerformed = true;
                //                UIManager.Instance.DisplayInteractableZoneMessage(false);
                //            }
                //            break;
                //    }
                //}
                //else if (Input.GetKey(_zoneKeyInput) && _keyState == KeyState.PressHold && _inHoldState == false)
                //{
                //    _inHoldState = true;



                //    switch (_zoneType)
                //    {                      
                //        case ZoneType.HoldAction:
                //            PerformHoldAction();
                //            break;           
                //    }
                //}

                //if (Input.GetKeyUp(_zoneKeyInput) && _keyState == KeyState.PressHold)
                //{
                //    _inHoldState = false;
                //    onHoldEnded?.Invoke(_zoneID);
                //}
                #endregion
                if (player._interactPress == true)
                {
                    switch (_zoneType)
                    {
                        case ZoneType.Collectable:
                            if (_itemsCollected == false)
                            {
                                CollectItems();
                                _itemsCollected = true;
                                player._interactPress = false;
                                player._interactHold = false;
                                UIManager.Instance.DisplayInteractableZoneMessage(false);
                            }
                            break;
                        case ZoneType.Action:
                            if (_actionPerformed == false)
                            {
                                PerformAction();
                                _actionPerformed = true;
                                player._interactPress = false;
                                player._interactHold = false;
                                UIManager.Instance.DisplayInteractableZoneMessage(false);
                            }
                            break;
                        case ZoneType.HoldAction:
                            player._interactPress = false;
                            player._interactHold = false;
                            break;
                    }
                }
                if(player._interactHold == true)
                {
                    switch(_zoneType)
                    {
                        case ZoneType.HoldAction:
                            player._interactHold = false;
                            PerformHoldAction();
                            break;
                        case ZoneType.Collectable:
                            if (_itemsCollected == false)
                            {
                                player._interactPress = false;
                                player._interactHold = false;
                            }
                            break;
                        case ZoneType.Action:
                            if (_actionPerformed == false)
                            {
                                player._interactPress = false;
                                player._interactHold = false;
                                PerformHoldAction();
                            }
                            break;
                    }
                }
                if(player._holdStop == true)
                {
                    switch(_zoneType)
                    {
                        case ZoneType.Action:
                            PerformHoldCancelAction();
                            break;
                    }

                }
            }
        }
        private void CollectItems()
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(false);
            }

            UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            CompleteTask(_zoneID);

            onZoneInteractionComplete?.Invoke(this);

        }

        private void PerformAction()
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(true);
            }

            if (_inventoryIcon != null)
                UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            onZoneInteractionComplete?.Invoke(this);
        }

        private void PerformHoldAction()
        {
            UIManager.Instance.DisplayInteractableZoneMessage(false);
            onHoldStarted?.Invoke(_zoneID);
            player._holdStop = false;
        }
        private void PerformHoldCancelAction()
        {
            onHoldEnded?.Invoke(_zoneID);
            player._holdStop = false;
        }

        public GameObject[] GetItems()
        {
            return _zoneItems;
        }

        public int GetZoneID()
        {
            return _zoneID;
        }

        public void CompleteTask(int zoneID)
        {
            if (zoneID == _zoneID)
            {
                _currentZoneID++;
                onZoneInteractionComplete?.Invoke(this);
            }
        }

        public void ResetAction(int zoneID)
        {
            if (zoneID == _zoneID)
                _actionPerformed = false;
        }

        public void SetMarker(InteractableZone zone)
        {
            if (_zoneID == _currentZoneID)
                _marker.SetActive(true);
            else
                _marker.SetActive(false);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _inZone = false;
                player.interactableZone = null;
                UIManager.Instance.DisplayInteractableZoneMessage(false);
            }
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= SetMarker;
        }       
        
    }
}


