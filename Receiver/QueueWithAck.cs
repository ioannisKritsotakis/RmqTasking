using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Receiver.Models;

namespace Receiver
{
    public class QueueWithAck<T> where T : class
    {
        private readonly Queue<QueueItem<T>> _queue;
        public QueueWithAck()
        {
            _queue = new Queue<QueueItem<T>>();
        }

        public Task<QueueItem<T>> ReadItem(CancellationToken cancellationToken)
        {
            return Task.Run(
                () =>
                {
                    QueueItem<T> queueItem;
                    while (!_queue.TryPeek(out queueItem))
                    {
                        Task.Delay(500, cancellationToken);
                    }

                    return queueItem;
                }, cancellationToken);
        }

        public void WriteItem(T item)
        {
            _queue.Enqueue(new QueueItem<T>(item));
        }

        public bool AckItem(QueueItem<T> item)
        {
            if (!_queue.TryPeek(out var kvPair)) return true;
            if (kvPair.Id != item.Id)
            {
                return false;
            }

            _queue.Dequeue();
            return true;
        }
    }

    public class QueueItem<T> where T : class
    {
        public T Value { get; }
        public Guid Id = Guid.NewGuid();

        public QueueItem(T value)
        {
            Value = value;
        }
    }
}