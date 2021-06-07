
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

namespace HelloWorld
{

    // Inherits from NetworkBehaviour (not MonoBehaviour)
    public class HelloWorldPlayer : NetworkBehaviour
    {

        // We now define a NetworkVariable to represent this player's networked position
        public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.ServerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        /*
         * Any MonoBehaviour implementing NetworkBehaviour can override the MLAPI method NetworkStart().
         * This method is fired when message handlers are ready to be registered and the networking is setup.
         * We override NetworkStart since a client and a server will run different logic here.
         */

        // То есть, когда установлено сеодинение, запускается данный метод. А Move() здесь
        // для первоначальной установки позиции player`а.
        public override void NetworkStart()
        {
            Debug.Log("NetworkStart is fired");
            Move();
        }

        public void Move()
        {
            // If the player is the server instance we assing a random position to our player
            // In other woords, if this player is a server-owned player, at NetworkStart() we can immediately move this player
            if (NetworkManager.Singleton.IsServer)
            {
                var randomPosition = GetRandomPositionOnPlane();
                transform.position = randomPosition;
                Position.Value = randomPosition;
            }
            // If the player is a client we request a position
            // If we are a client, we call a server RPC. (RPC = Remote Procedure Calls; Remote Actions)
            else
            {
                SubmitPositionRequestServerRpc();
            }
        }


        /*
         * [ServerRpc] attribute and matching ...ServerRpc suffix in the method name are there
         * to make it crystal clear for RPC call sites to know when they are executing an RPC.
         * It will be replicated and executed on the server-side, without necessarily jumping
         * into original RPC method declaration to find out if it was an RPC, if so whether
         * it is a ServerRpc or ClientRpc
         * 
         * То есть у нас Player может быть как сервером, так и клиентом.
         * Если Player это клиент, то он делает RPC (Удалённый запрос на какое-то действие),
         * то есть действие совершеается сервером по запросу клиента
         */
        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            Position.Value = GetRandomPositionOnPlane();
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        void Update()
        {
            transform.position = Position.Value;
        }
    }
}