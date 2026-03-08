// First
var slideCount = 0;
var random = new Random();
var firstSlide = new HeliumScene();
var singleElementContainer = new SingleElementContainer()
{
    Alignment = Alignment.Center,
    Child = new RectangleElement(0.3d, 0.3d, new SColor(0, 0, 255, 255), "blue-rect")
};
firstSlide.Root = singleElementContainer;
presentationEngine.AddScene($"slide{slideCount}", firstSlide);
presentationEngine.SetCurrentScene($"slide{slideCount}");

var timer = new System.Timers.Timer(3000);
timer.Start();
timer.Elapsed += (sender, e) =>
{
    slideCount += 1;
    var nextSlide = firstSlide.Clone();
    var nextSEC = nextSlide.Root as SingleElementContainer;
    nextSEC!.Alignment = (Alignment)random.Next(0, 9);
    presentationEngine.AddScene($"slide{slideCount}", nextSlide);

    var transition = new FirstTransition(firstSlide, nextSlide, 0.5d, new SSizeF(1280, 720));
    presentationEngine.AddTransition($"slide{slideCount - 1}", $"slide{slideCount}", transition);

    presentationEngine.SetCurrentScene($"slide{slideCount - 1}");
    presentationEngine.RemoveScene($"slide{slideCount - 2}");
    firstSlide = nextSlide;
};

// Second

var slide = new HeliumScene();
var binaryElementContainer = new BinaryElementContainer
{
    Alignment = Alignment.Center
};
binaryElementContainer.SplitVertical(1, 1);

var text = new TextElement()
{
    Text = "Hello, world!",
    Font = new SFontFamily("Calibri", 1f),
    Color = SColor.White,
    Rotation = SAngle.Zero
};
text.SetDesiredWidthFraction(0.5d);
text.SetDesiredHeightFraction(0.5d);
var greenRect = new RectangleElement(0.5d, 0.5d, new SColor(0, 255, 0, 255), "green-rect");
var redRect = new RectangleElement(0.5d, 0.5d, new SColor(255, 0, 0, 255), "red-rect");
binaryElementContainer.FirstSplit!.SplitHorizontal(1, 1);
binaryElementContainer.SecondSplit!.SetSingleChild(greenRect);
binaryElementContainer.FirstSplit.FirstSplit!.SetSingleChild(text);
binaryElementContainer.FirstSplit.SecondSplit!.SetSingleChild(redRect);
slide.Root = binaryElementContainer;

presentationEngine.AddScene(nameof(slide), slide);
presentationEngine.SetCurrentScene(nameof(slide));