//using System.Runtime.CompilerServices;
//using UnityEngine;

//public class PlayerPickUpDrop : MonoBehaviour
//{
//    [Header("Keybinding")]
//    public KeyCode pickKey = KeyCode.E;

//    private float pickUpDistance = 2f;
//    public LayerMask pickUpLayerMask;
//    public Transform playerCameraTransform;
//    public Transform objectGrabPointTransform;

//    private Item currentItem;


//    void Update()
//    {
//        if (Input.GetKeyDown(pickKey))
//        {
//            if (currentItem == null)
//            {
//                //try to grab it
//                if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit raycastHit, pickUpDistance, pickUpLayerMask))
//                {
//                    if (raycastHit.transform.TryGetComponent(out currentItem))
//                    {
//                        currentItem.Grab(objectGrabPointTransform);
//                        currentItem.isHeld = true;
//                    }
//                }
//            }
//            else
//            {
//                currentItem.Drop();
//                currentItem = null;
//            }
//        }
//    }
//}
