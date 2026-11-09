using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProGamer
{
    public enum QueueStatus
    {
        Waiting = 0,
        Executing = 1,
        Completed = 2
    };

    public class QueueSystem : Singleton<QueueSystem>
    {
        private Queue<QueueObject> queues = new Queue<QueueObject>();
        private QueueObject currentQueue;
        private QueueStatus queueSystemStatus = QueueStatus.Waiting;

        #region QueueSystem API

        /// Starts the execute queues.
        public static void ExecuteQueues()
        {
            Instance.StartExecuteQueues();
        }

        public static void AddWaitingQueue(QueueObject queue)
        {
            Instance.AddQueue(queue);
        }

        public static void AddQueueWithExecution(QueueObject queue)
        {
            Instance.AddQueueAndExecute(queue);
        }

        public static bool ExistenceInQueue(System.Type t)
        {
            return Instance.CheckExistenceInQueue(t);
        }

        #endregion

        private void StartExecuteQueues()
        {
            if (queueSystemStatus == QueueStatus.Executing)
                return;
            queueSystemStatus = QueueStatus.Executing;
            NextQueue();
        }

        private void AddQueue(QueueObject queue)
        {
            if (queueSystemStatus == QueueStatus.Waiting)
            {
                queues.Enqueue(queue);
            }
            else
            {
                Debug.LogWarning("ATTENTION: AddQueue - Only valid for queue system waiting");
            }
        }

        private void AddQueueAndExecute(QueueObject queue)
        {
            if (queueSystemStatus == QueueStatus.Executing)
                queues.Enqueue(queue);
            else
            {
                queues.Enqueue(queue);
                StartExecuteQueues();
            }
        }

        private void ClearQueue()
        {
            queues.Clear();
        }

        private void NextQueue()
        {
            if (queues.Count > 0)
            {
                currentQueue = queues.Dequeue();

                try
                {
                    currentQueue.QueueStart();
                    StartListen();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                    currentQueue.QueueCompleted();
                }
            }
        }

        private const float timeOut = 1f;
        private float waitingTime;

        private bool isListen = false;

        private void StartListen()
        {
            waitingTime = 0;

            isListen = true;
        }

        void Update()
        {
            if (isListen)
            {
                if (currentQueue.Status != QueueStatus.Completed)
                {
                    waitingTime += Time.deltaTime;
                    if (waitingTime >= timeOut)
                    {
                        if (currentQueue == null || !currentQueue.gameObject.activeSelf)
                        {
                            OnQueueCompleted();
                        }
                        else
                        {
                            //Retry
                            waitingTime = 0;
                        }
                    }
                }
                else
                {
                    OnQueueCompleted();
                }
            }
        }

        // Completed single queue
        private void OnQueueCompleted()
        {
            Debug.Log("Single queue completed");

            isListen = false;

            if (queues.Count > 0)
            {
                NextQueue();
            }
            else
            {
                //Change to waiting new queue
                queueSystemStatus = QueueStatus.Waiting;

                currentQueue = null;

                Debug.Log("Queues all completed");
            }
        }

        protected bool CheckExistenceInQueue(System.Type t)
        {
            if (currentQueue != null && currentQueue.GetType() == t) return true;

            foreach (var q in queues)
            {
                if (q.GetType() == t)
                {
                    return true;
                }
            }

            return false;
        }
    }
}