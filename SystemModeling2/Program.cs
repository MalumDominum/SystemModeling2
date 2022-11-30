using SystemModeling2;
using SystemModeling2.Devices;
using SystemModeling2.Infrastructure;
using RE = SystemModeling2.Infrastructure.RandomExtended;

var createT = new Transition("Create T", () => 1);
var goToServiceT = new Transition("Go to service T", () => 1);
var busyT = new Transition("Busy a resource T", () => 1);
var freeT = new Transition("Free a resourse T", () => 1);
var finalT1 = new Transition("Final T1", () => 1);
var finalT2 = new Transition("Final T2", () => 1);
var finalT3 = new Transition("Final T3", () => 1);
var finalT4 = new Transition("Final T4", () => 1);
var transitions = new List<Transition> { createT, goToServiceT, busyT, freeT, finalT1, finalT2, finalT3, finalT4 };


var createP1 = new Place("Create P1");
var createP2 = new Place("Create P2", 1);
var passP = new Place("Pass P");
var busyP = new Place("Busy P");
var freeP = new Place("Free P", 1);
var passFinalP = new Place("Pass final P");
var finalP1 = new Place("Final P1");
var finalP2 = new Place("Final P2");
var finalP3 = new Place("Final P3");
var finalP4 = new Place("Final P4");
var places = new List<Place> { createP1, createP2, passP, busyP, freeP, passFinalP, finalP1, finalP2, finalP3, finalP4 };


var createA1 = ArcBuilder.Build(createP1, createT);
var createA2 = ArcBuilder.Build(createT, createP2);
var createA3 = ArcBuilder.Build(createP2, goToServiceT);
var createA4 = ArcBuilder.Build(goToServiceT, createP1);
var serviceA1 = ArcBuilder.Build(goToServiceT, passP);
var serviceA2 = ArcBuilder.Build(passP, busyT);
var busyA1 = ArcBuilder.Build(busyT, busyP);
var busyA2 = ArcBuilder.Build(busyP, freeT);
var freeA1 = ArcBuilder.Build(freeT, freeP);
var freeA2 = ArcBuilder.Build(freeP, busyT);
var finalPassA = ArcBuilder.Build(freeT, passFinalP);
var finalA1ToTransition = ArcBuilder.Build(passFinalP, finalT1);
var finalA2ToTransition = ArcBuilder.Build(passFinalP, finalT2);
var finalA3ToTransition = ArcBuilder.Build(passFinalP, finalT3);
var finalA4ToTransition = ArcBuilder.Build(passFinalP, finalT4);
var finalA1ToPlace = ArcBuilder.Build(finalT1, finalP1);
var finalA2ToPlace = ArcBuilder.Build(finalT2, finalP2);
var finalA3ToPlace = ArcBuilder.Build(finalT3, finalP3);
var finalA4ToPlace = ArcBuilder.Build(finalT4, finalP4);


var model = new Model
{
	Transitions = transitions,
	Places = places,
};

model.Simulate(5000);

