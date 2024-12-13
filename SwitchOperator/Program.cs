// See https://aka.ms/new-console-template for more information

using DynamicData;
using DynamicData.Cache.Internal;
using System.Reactive.Disposables;
using System.Reactive.Linq;

Console.WriteLine("Hello, World!");



public static class DynamicDataExt
{
    public static IObservable<IChangeSet<TObject, TKey>> SwitchChangeSet<TObject, TKey>(
        this IObservable<IObservable<IChangeSet<TObject, TKey>>> sources
    )
        where TObject : notnull
        where TKey : notnull
    {
        return Observable.Create<IChangeSet<TObject, TKey>>(
            observer =>
            {
                object locker = new();

                LockFreeObservableCache<TObject, TKey> destination = new();

                IObservable<IChangeSet<TObject, TKey>> populator = Observable.Switch(
                        sources.Do(
                            _ =>
                            {
                                lock (locker)
                                {
                                    destination.Clear();
                                }
                            })
                    )
                    .Synchronize(locker)
                    .Do(changes => destination.Edit(updater => updater.Clone(changes)))
                    .Where(_ => false);

                return new CompositeDisposable(
                    destination,
                    populator.Merge(destination.Connect()).SubscribeSafe(observer)
                );
            });
    }
}