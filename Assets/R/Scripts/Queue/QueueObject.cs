using UnityEngine;
using System.Collections;

namespace ProGamer
{
    public abstract class QueueObject : MonoBehaviour
    {
        protected QueueStatus status = QueueStatus.Waiting;

        public QueueStatus Status
        {
            get { return status; }
        }

        /// <summary>
        /// Only call by QueueSystem
        /// </summary>
        public abstract void QueueStart();

        /// <summary>
        /// Only call when mission completed and call by queue class
        /// </summary>
        public abstract void QueueCompleted();

        /* format
    public override void QueueStart (){
        status = QueueStatus.Executing;
        //TODO
    }
    
    public override void QueueCompleted(){
        //TODO
        status = QueueStatus.Completed;
    }
    */
    }
}