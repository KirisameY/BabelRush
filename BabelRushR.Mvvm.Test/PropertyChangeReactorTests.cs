using System.Collections.Frozen;
using System.Collections.Immutable;

using BabelRushR.Core.Common;
using BabelRushR.Mvvm.Infrastructure;

namespace BabelRushR.Mvvm.Test;

/// <summary>
///     <see cref="PropertyChangeReactor{TObj}"/> 的转发行为。
/// </summary>
/// <remarks>
///     具体 VM 的反应表不需要被逐条钉死；这里用一个可控的派生 VM 测试「更新先于通知」「空属性名跑全部」
///     「未知属性忽略」「重复更新去重」这些基类契约。
/// </remarks>
public class PropertyChangeReactorTests
{
    private sealed class TestModel : ObservableObject
    {
        public void Raise(string? propertyName) => OnPropertyChanged(propertyName);
    }

    private sealed class TestViewModel : ViewModelBase<TestViewModel>
    {
        private static readonly PropertyChangeReactor<TestViewModel> Reactions = new(
            new Dictionary<string, (ImmutableArray<Action<TestViewModel>> Updates, ImmutableArray<string> Notifications)>
            {
                ["A"] = ([RecordA], ["A", "ADerived"]),
                ["B"] = ([RecordB, RecordShared], ["B"]),
                ["C"] = ([RecordShared], ["C"]),
            }.ToFrozenDictionary()
        );

        public TestViewModel(TestModel model) : base(model, Reactions)
        {
            PropertyChanged += (_, args) => Log.Add($"notify:{args.PropertyName}");
        }

        public List<string> Log { get; } = [];

        private static void RecordA(TestViewModel instance) => instance.Log.Add("update:A");
        private static void RecordB(TestViewModel instance) => instance.Log.Add("update:B");
        private static void RecordShared(TestViewModel instance) => instance.Log.Add("update:shared");
    }

    [Fact]
    public void Named_Property_Runs_Updates_Before_Its_Notifications()
    {
        var model = new TestModel();
        var viewModel = new TestViewModel(model);

        model.Raise("A");

        Assert.Equal(new[] { "update:A", "notify:A", "notify:ADerived" }, viewModel.Log);
    }

    [Fact]
    public void Named_Property_Does_Not_Run_Reactions_Of_Other_Properties()
    {
        var model = new TestModel();
        var viewModel = new TestViewModel(model);

        model.Raise("B");

        Assert.Contains("update:B", viewModel.Log);
        Assert.Contains("update:shared", viewModel.Log);
        Assert.Contains("notify:B", viewModel.Log);
        Assert.DoesNotContain("update:A", viewModel.Log);
        Assert.DoesNotContain("notify:C", viewModel.Log);
    }

    [Fact]
    public void Empty_Property_Name_Runs_All_And_Deduplicates_Updates()
    {
        var model = new TestModel();
        var viewModel = new TestViewModel(model);

        model.Raise(null);

        Assert.Contains("update:A", viewModel.Log);
        Assert.Contains("update:B", viewModel.Log);
        // RecordShared 同时挂在 B 和 C 上，"全部失效"时只应跑一次。
        Assert.Single(viewModel.Log, entry => entry == "update:shared");
        Assert.Contains("notify:A", viewModel.Log);
        Assert.Contains("notify:ADerived", viewModel.Log);
        Assert.Contains("notify:B", viewModel.Log);
        Assert.Contains("notify:C", viewModel.Log);
    }

    [Fact]
    public void Unknown_Property_Is_Ignored()
    {
        var model = new TestModel();
        var viewModel = new TestViewModel(model);

        model.Raise("NotAReaction");

        Assert.Empty(viewModel.Log);
    }

    [Fact]
    public void Disposed_ViewModel_Stops_Forwarding()
    {
        var model = new TestModel();
        var viewModel = new TestViewModel(model);

        viewModel.Dispose();
        model.Raise("A");

        Assert.Empty(viewModel.Log);
    }
}
