using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.APIModels;
using TeamServer.Services;

[InjectableService]
public interface IChangeTrackingService
{
    List<Change> ConsumeChanges(string session);
    void CleanSession(string session);
    void RecordSession(string session);
    bool ContainsSession(string session);
    void TrackChange(ChangingElement element, string id);
}

[InjectableServiceImplementation(typeof(IChangeTrackingService))]
public class ChangeTrackingService : IChangeTrackingService
{
    private readonly ConcurrentDictionary<string, List<Change>> TrackedChanges = new();
    private readonly object _trackedChangesLock = new object();

    public void TrackChange(ChangingElement element, string id)
    {
        var change = new Change(element, id);
        lock (_trackedChangesLock)
        {
            foreach (var session in this.TrackedChanges.Keys.ToList())
            {
                if (!TrackedChanges[session].Any(c => c.Element == change.Element && c.Id == id))
                    this.TrackedChanges[session].Add(change);
            }
        }
    }

    public List<Change> ConsumeChanges(string session)
    {
        lock (_trackedChangesLock)
        {
            if (!this.TrackedChanges.ContainsKey(session))
            {
                this.TrackedChanges.TryAdd(session, new List<Change>());
                return new List<Change>();
            }

            var lst = this.TrackedChanges[session];
            this.TrackedChanges[session] = new List<Change>();
            return lst;
        }
    }

    public void CleanSession(string session)
    {
        this.TrackedChanges.TryRemove(session, out _);
    }

    public bool ContainsSession(string session)
    {
        return this.TrackedChanges.ContainsKey(session);
    }

    public void RecordSession(string session)
    {
        this.TrackedChanges.TryAdd(session, new List<Change>());
    }
}

