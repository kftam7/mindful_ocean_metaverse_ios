using UnityEngine;
using System.Collections;

namespace ProGamer
{
    /// <summary>
    /// Use for exexuting sigle function
    /// </summary>
    public class SimpleQueue : QueueObject
    {
        private event SimplePopupCallback executeActions;

        public SimpleQueue(SimplePopupCallback action)
        {
            executeActions += action;
        }

        public override void QueueStart()
        {
            status = QueueStatus.Executing;
            if (executeActions != null) executeActions();
            QueueCompleted();
        }

        public override void QueueCompleted()
        {
            status = QueueStatus.Completed;
        }
    }
}