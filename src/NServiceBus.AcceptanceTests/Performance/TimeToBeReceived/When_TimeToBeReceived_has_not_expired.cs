﻿namespace NServiceBus.AcceptanceTests.Performance.TimeToBeReceived
{
    using System;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_TimeToBeReceived_has_not_expired : NServiceBusAcceptanceTest
    {
        [Test]
        public void Message_should_be_received()
        {
            var context = new Context();

            Scenario.Define(context)
                    .WithEndpoint<Endpoint>(b => b.Given((bus, c) => bus.SendLocal(new MyMessage())))
                    .Done(c => c.WasCalled)
                    .Run();

            Assert.IsTrue(context.WasCalled);
            Assert.AreEqual(TimeSpan.FromSeconds(10),context.TTBROnIncomingMessage, "TTBR should be available as a header so receiving endpoints can know what value was used when the message was originally sent");
        }

        public class Context : ScenarioContext
        {
            public bool WasCalled { get; set; }
            public TimeSpan TTBROnIncomingMessage { get; set; }
        }

        public class Endpoint : EndpointConfigurationBuilder
        {
            public Endpoint()
            {
                EndpointSetup<DefaultServer>();
            }
            public class MyMessageHandler : IHandleMessages<MyMessage>
            {
                public Context Context { get; set; }

                public IBus Bus { get; set; }

                public void Handle(MyMessage message)
                {
                    Context.TTBROnIncomingMessage = TimeSpan.Parse(Bus.CurrentMessageContext.Headers[Headers.TimeToBeReceived]);
                    Context.WasCalled = true;
                }
            }
        }

        [Serializable]
        [TimeToBeReceived("00:00:10")]
        public class MyMessage : IMessage
        {
        }
    }
}