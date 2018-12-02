# SimpleKeplerOrbits

![gif](https://i.imgur.com/0dhq9kL.gif)

## Description

Unity3d project for simplest orbits simulation.

## Importing

This repository contains whole Unity Project for basic demonstration setup.
There are multiple ways of importing project:
- Copy all files from Asset directory manually;
- Importing asset store package;
- Importing src branch as submodule, which contains only Assets folder content.

## Usage

This plugin is designed to be customizable via editor inspector and via scripting.
Scripting is fully optional. Some usage examples:

#### Initializing orbit from orbit elements (JPL database supported)
```cs
	var body = GetComponent<KeplerOrbitMover>();
	body.AttractorSettings.AttractorObject = attractorTransform;
	body.AttractorSettings.AttractorMass = attractorMass;
	body.AttractorSettings.GravityConstant = GConstant;
	body.OrbitData = new KeplerOrbitData(
		eccentricity: eValue,
		semiMajorAxis: aValue,
		meanAnomalyDeg: mValue,
		inclinationDeg: inValue,
		argOfPerifocus: wValue,
		ascendingNodeDeg: omValue,
		attractorMass: attractorMass,
		gConst: GConstant);
	body.ForceUpdateViewFromInternalState();
```

#### Initializing orbit from orbit vectors
```cs
	var body = GetComponent<KeplerOrbitMover>();
	body.AttractorSettings.AttractorObject = attractorTransform;
	body.AttractorSettings.AttractorMass = attractorMass;
	body.AttractorSettings.GravityConstant = GConstant;
	body.OrbitData = new KeplerOrbitData(
		position: bodyPosition, 
		velocity: bodyVelocity, 
		attractorMass: attractorMass, 
		gConst: GConstant);
	body.ForceUpdateViewFromInternalState();	
```

For more detailed scipting snippets see [manual](Assets/SimpleKeplerOrbits/Readme.md).

## Published version

Asset store [link]

[link]: https://www.assetstore.unity3d.com/en/#!/content/97048

## License

[MIT](LICENSE)