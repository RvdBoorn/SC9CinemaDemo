using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;

namespace SC9Demo.Rules.Conditions
{
    public static class ContactHelper
    {
        public static Contact GetXConnectContact(Sitecore.Analytics.Tracking.Contact currentContact)
        {
            /*
             * Stupid hack. I struggled with the 'new' rule structure for over 2 hours
             * with a very strange scenario where it kept saying it couldn't find my 
             * implementation, while it was clearly there. I checked it over ten times with
             * the new documentation website. Ended up giving it the .....
             */

            using (XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
            {
                try
                {
                    return GetXConnectContactSynchronous(client, currentContact);
                }
                catch (XdbExecutionException)
                {
                }
            }

            return null;
        }

        public static Contact GetXConnectContact(string ContactIdentifier)
        {
            /*
             * Stupid hack. I struggled with the 'new' rule structure for over 2 hours
             * with a very strange scenario where it kept saying it couldn't find my 
             * implementation, while it was clearly there. I checked it over ten times with
             * the new documentation website. Ended up giving it the .....
             */

            using (XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
            {
                try
                {
                    var contact = AsyncHelpers.RunSync(() => client.GetContactAsync(
                    new IdentifiedContactReference("sitecoreextranet", ContactIdentifier),
                    new ContactExpandOptions(SitecoreCinema.Model.Collection.CinemaVisitorInfo.DefaultFacetKey)));

                    return contact;
                }
                catch (XdbExecutionException)
                {
                }
            }

            return null;
        }

        private static Contact GetXConnectContactSynchronous(XConnectClient client, Sitecore.Analytics.Tracking.Contact currentContact)
        {
            var validIdentifier = currentContact.Identifiers.FirstOrDefault(
                i =>
                i.Source.Equals("sitecoreextranet", StringComparison.OrdinalIgnoreCase)
                &&
                i.Identifier.Contains("@"));

            if (validIdentifier != null)
            {
                var contact = AsyncHelpers.RunSync(() => client.GetContactAsync(
                    new IdentifiedContactReference(validIdentifier.Source, validIdentifier.Identifier),
                    new ContactExpandOptions(SitecoreCinema.Model.Collection.CinemaVisitorInfo.DefaultFacetKey)));

                return contact;
            }

            return null;
        }



    }

    public static class AsyncHelpers
    {
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
            synch.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (this.items)
                {
                    this.items.Enqueue(Tuple.Create(d, state));
                }
                this.workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                this.Post(_ => this.done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!this.done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (this.items)
                    {
                        if (this.items.Count > 0)
                        {
                            task = this.items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (this.InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", this.InnerException);
                        }
                    }
                    else
                    {
                        this.workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}