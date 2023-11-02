using Photon.Pun;
using UnityEngine;

namespace MRTK.Tutorials.MultiUserCapabilities
{
    public class GenericNetSyncObject : MonoBehaviourPun, IPunObservable
    {
        private Vector3 networkPosition;
        private Vector3 startingPosition;
        public Vector3 adjustPosFeature;

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(transform.position);
            }
            else
            {
                networkPosition = (Vector3) stream.ReceiveNext() + adjustPosFeature;
            }
        }

        private void Start()
        {
            var trans = transform;
            startingPosition = trans.position;
            networkPosition = startingPosition + adjustPosFeature;
        }

        // private void FixedUpdate()
        private void Update()
        {
            if (!photonView.IsMine)
            {
                var trans = transform;
                trans.position = networkPosition;
            }
        }
    }
}
