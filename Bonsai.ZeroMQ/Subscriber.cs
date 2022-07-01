﻿using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Osc;
using NetMQ;
using NetMQ.Sockets;

namespace Bonsai.ZeroMQ
{
    public class Subscriber : Source<byte[]>
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Topic { get; set; }

        public override IObservable<byte[]> Generate()
        {
            return Observable.Create<byte[]>((observer, cancellationToken) =>
            {
                var sub = new SubscriberSocket($"tcp://{Host}:{Port}");
                sub.Subscribe(Topic);

                return Task.Factory.StartNew(() =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        string messageTopic = sub.ReceiveFrameString();
                        byte[] messagePayload = sub.ReceiveFrameBytes();
                        observer.OnNext(messagePayload);
                    }
                }).ContinueWith(task => {
                    sub.Dispose();
                    task.Dispose();
                });
            });
        }
    }
}
