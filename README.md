# SimpleKeplerOrbits

![gif](https://i.imgur.com/0dhq9kL.gif)

## Description

Unity3d project for simulating simple orbits using 2-body solution model.

## Importing

This repository contains the entire Unity Project for basic demo setup.
There are several ways to import the project:
- Manually copy all files from Asset directory to your unity project directory;
- Import asset store package;
- Import of the 'src' branch as [embedded package](https://docs.unity3d.com/Manual/CustomPackages.html#EmbedMe).

## Usage

This plugin has the ability to customize orbits using the editor inspector and scripts.
Some scripting usage examples:

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

For more detailed scripting snippets go to the [included manual](Assets/SimpleKeplerOrbits/Readme.md).

## Contributing

This repository is closed for contributions.

## Published version

[Unity Asset Store]

[Unity Asset Store]: https://assetstore.unity.com/packages/tools/physics/simple-kepler-orbits-97048
## License

[MIT](LICENSE)