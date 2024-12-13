using DynamicData;
using System.Reactive.Linq;

namespace TestSwitchOperator
{
    public class SwitchTest
    {

        [Fact]
        public void PropagatesErrors()
        {
            bool exceptionCaught = false;

            List<List<int>> listOfLists = [[0, 1, 2]];

            using IDisposable subscription = listOfLists.ToObservable()
                .ToObservableChangeSet(int (value) => value)
                .Select(set =>
                {
                    throw new Exception();              // Something bad happened here
                    return Observable.Return(set);
                })
                .Do(_ => { }, e => { })                 // here the error handler is called
                .Switch()                               // here the exception is swallowed
                .Catch((Exception e) =>                 // this code will never be executed
                {
                    exceptionCaught = true;
                    return Observable.Empty<IChangeSet<int, int>>();
                })
                .Subscribe();

            Assert.True(exceptionCaught);
        }



        [Fact]
        public void FixedPropagatesErrors()
        {
            bool exceptionCaught = false;

            List<List<int>> listOfLists = [[0, 1, 2]];

            using IDisposable subscription = listOfLists.ToObservable()
                .ToObservableChangeSet(int (value) => value)
                .Select(set =>
                {
                    throw new Exception();
                    return Observable.Return(set);
                })
                .Do(_ => { }, e => { })
                .SwitchChangeSet()
                .Catch((Exception e) =>
                {
                    exceptionCaught = true;
                    return Observable.Empty<IChangeSet<int, int>>();
                })
                .Subscribe();

            Assert.True(exceptionCaught);
        }

    }
}
