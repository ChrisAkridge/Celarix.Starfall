using System.Reflection;
using Celarix.Starfall.Atria;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Tests.Atria;

public sealed class SlideMetadataTests
{
    [Fact]
    public void AddSlide_SetsSlideName()
    {
        var engine = CreateEngine();
        var slide = new CountingSlide(engine.Runtime!, steps: 0);

        engine.AddSlide(slide, "Counting");

        Assert.Equal("Counting", slide.Name);
    }

    [Fact]
    public void AdvanceCurrentSlide_CountsBeatsUntilTheSlideCanAdvance()
    {
        var engine = CreateEngine();
        var slide = AddCurrent(engine, new CountingSlide(engine.Runtime!, steps: 2));

        Assert.Equal(SlideAdvanceResult.InternalStateChanged, engine.AdvanceCurrentSlide());
        Assert.Equal(SlideAdvanceResult.InternalStateChanged, engine.AdvanceCurrentSlide());
        Assert.Equal(SlideAdvanceResult.CanAdvance, engine.AdvanceCurrentSlide());

        Assert.Equal(2, slide.BeatIndex);
        Assert.Equal("Step 2", slide.CurrentBeat);
    }

    [Fact]
    public void RewindCurrentSlide_StepsBeatBackButNotBelowZero()
    {
        var engine = CreateEngine();
        var slide = AddCurrent(engine, new CountingSlide(engine.Runtime!, steps: 1));

        engine.AdvanceCurrentSlide();
        engine.RewindCurrentSlide();
        engine.RewindCurrentSlide();

        Assert.Equal(0, slide.BeatIndex);
    }

    [Fact]
    public void CurrentBeat_UsesStateMachineStateName()
    {
        var engine = CreateEngine();
        var slide = AddCurrent(engine, new StateMachineSlide(engine.Runtime!));

        Assert.Equal(nameof(StateMachineSlide.State.Initial), slide.CurrentBeat);

        engine.AdvanceCurrentSlide();

        Assert.Equal(nameof(StateMachineSlide.State.ShowProblem), slide.CurrentBeat);
        Assert.Equal(1, slide.BeatIndex);
    }

    [Fact]
    public void Runtime_DefaultsToConsoleInput()
    {
        var engine = CreateEngine();

        Assert.IsType<ConsolePresenterInput>(engine.Runtime!.Input);
    }

    private static AtriaLayoutEngine CreateEngine()
    {
        var engine = new AtriaLayoutEngine(1280, 720);
        engine.Attach(NullRenderTarget.Create());
        return engine;
    }

    private static TSlide AddCurrent<TSlide>(AtriaLayoutEngine engine, TSlide slide) where TSlide : AtriaSlide
    {
        engine.AddSlide(slide, "slide");
        engine.SetCurrentSlide("slide");
        return slide;
    }

    private sealed class CountingSlide : AtriaSlide
    {
        private readonly int _steps;
        private int _position;

        public CountingSlide(AtriaRuntime runtime, int steps) : base(runtime, runtime.ViewportSize)
        {
            _steps = steps;
        }

        public override void Initialize() { }

        public override SlideAdvanceResult Advance()
        {
            if (_position >= _steps)
            {
                return SlideAdvanceResult.CanAdvance;
            }

            _position += 1;
            return SlideAdvanceResult.InternalStateChanged;
        }

        public override SlideAdvanceResult Rewind()
        {
            if (_position == 0)
            {
                return SlideAdvanceResult.CanRewind;
            }

            _position -= 1;
            return SlideAdvanceResult.InternalStateChanged;
        }
    }

    private sealed class StateMachineSlide : AtriaSlide
    {
        public enum State
        {
            Initial,
            ShowProblem
        }

        private StateMachine<State>? _stateMachine;

        public StateMachineSlide(AtriaRuntime runtime) : base(runtime, runtime.ViewportSize) { }

        public override void Initialize()
        {
            _stateMachine = new StateMachine<State>(this, State.Initial);
        }

        public override SlideAdvanceResult Advance()
        {
            if (_stateMachine!.CurrentState == State.ShowProblem)
            {
                return SlideAdvanceResult.CanAdvance;
            }

            _stateMachine.GoToState(State.ShowProblem);
            return SlideAdvanceResult.InternalStateChanged;
        }

        [StateTransition<State>(State.Initial, State.ShowProblem)]
        private void ShowProblem() { }
    }

    /// <summary>
    /// An <see cref="IRenderTarget"/> whose members all do nothing and return defaults.
    /// </summary>
    private class NullRenderTarget : DispatchProxy
    {
        public static IRenderTarget Create() => Create<IRenderTarget, NullRenderTarget>();

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var returnType = targetMethod?.ReturnType;
            return returnType == null || returnType == typeof(void) || !returnType.IsValueType
                ? null
                : Activator.CreateInstance(returnType);
        }
    }
}
