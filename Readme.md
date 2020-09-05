# Simple Kepler Orbits

## First usage guide

	* Create new object which will be attractor.
	* Create other object which will be orbiting body.
	* Create third object which will be velocity handle helper object.
	* Attach KeplerOrbitMover component to the orbiting body object.
	* Assign attractor object reference to the AttractorObject field inside the KeplerOrbitMover component.
	* Assign the velocity helper object to the VelocityHandler field inside the KeplerOrbitMover component.
	* (optional) Attach KeplerOrbitLineDisplay component to the orbiting body object to be able to see orbit path curve.
	* Move orbiting body and velocity helper in scene window and notice how orbit curve changes.
	* If you hit play, body will start moving along orbit curve around the attractor body.

## Repository

https://github.com/Karth42/SimpleKeplerOrbits

## Components

	* KeplerOrbitMover - component that simulates orbit motion for game object.
	It contains main settings and orbit state.

	* KeplerOrbitLineDisplay - component for displaying orbit path
	via customizable LineRenderer object or via editor Gizmos.

	* EllipticInterceptionSolver - component for calculation transfer
	orbit between two orbiting bodies.

## Scripting

Main MonoBehaviour component, KeplerOrbitMover, is just an lightweight adapter between the unity scene and the KeplerOrbitData container. 
The KeplerOrbitData container struct contains full orbit state and all orbit handling logic and can be used in scripts even without KeplerOrbitMover in different situations.
This document, however, will give examples only for KeplerOrbitData in pair with KeplerOrbitMover situations.

List of some scripting snippets, that can be usefull:

#### Orbit initialization

```cs
	// Initializing orbit via KeplerOrbitMover public interface.
	
	var body = GetComponent<KeplerOrbitMover>();
  
	// Setup attractor for orbit.
	body.AttractorSettings.AttractorObject = attractorReference;
	body.AttractorSettings.AttractorMass = attractorMass;
  
	// Lock inspector editing.
	body.LockOrbitEditing = false;

	// Create orbit from custom position and velocity
	body.CreateNewOrbitFromPositionAndVelocity(newPosition, newVelocity);
```

```cs
	// Initializing orbit from orbit elements (JPL database supported)
	
	var body = GetComponent<KeplerOrbitMover>();
	
	// Setup attractor settings for update process;
	// Note: AttractorSettings is not required for OrbitData setup, but it will be used in Update later, so it is initialized with same parameters.
	body.AttractorSettings.AttractorObject = attractorTransform;
	body.AttractorSettings.AttractorMass = attractorMass;
	body.AttractorSettings.GravityConstant = GConstant;
	
	// Setup orbit state.
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

```cs
	// Initializing orbit from orbit vectors.
	
	var body = GetComponent<KeplerOrbitMover>();
	
	// Setup attractor settings for update process;
	body.AttractorSettings.AttractorObject = attractorTransform;
	body.AttractorSettings.AttractorMass = attractorMass;
	body.AttractorSettings.GravityConstant = GConstant;
	
	// Setup orbit state.
	body.OrbitData = new KeplerOrbitData(
		position: bodyPosition, 
		velocity: bodyVelocity, 
		attractorMass: attractorMass, 
		gConst: GConstant);
	body.ForceUpdateViewFromInternalState();	
```

#### Orbit changing

```cs
	// Make orbit circular.
	body.SetAutoCircleOrbit();
```
```cs
	// Update attractor settings.
	body.AttractorSettings.AttractorMass = 100;

	// Refresh orbit state to apply changes.
	body.ForceUpdateOrbitData();
```
```cs
	// Set different eccentricity for orbit, leaving perifocus and mean anomaly unchanged.
	body.OrbitData.SetEccentricity(newEccentricity);

	// Update transform from orbit state.
	body.ForceUpdateViewFromInternalState();
```
```cs
	// Set anomaly for orbit.
	body.OrbitData.SetMeanAnomaly(anomalyValueInRadians);
	// Or other anomalies:
	// body.OrbitData.SetTrueAnomaly(anomalyValueInRadians);
	// body.OrbitData.SetEccentricAnomaly(anomalyValueInRadians);

	// Note: changing one anomaly will automatically update other two.

	// After changing orbit state, transform should be updated:
	body.ForceUpdateViewFromInternalState();
```
```cs
	// Progress mean anomaly by mean motion, scaled by delta time.
	body.OrbitData.UpdateOrbitAnomaliesByTime(deltaTime);
	body.ForceUpdateViewFromInternalState();

	//Note: the mean motion is dependent on orbit period. It will behave differently for different orbits.
	//To strictly set some certain orbit time or progress ratio (independent from orbit state), set anomaly value explicitly instead.
```
```cs
	// Set orbit position at specific Time.
	// The mean anomaly is changing with time as meanAnomaly = meanMotion * elapsedTime;
	// So the orbit state at specific time can be calculated with this snipped:

	float time = Time.time;
	float T0 = 0; // input parameter, can be anything.
	float meanAnomalyAtT0 = 0; // input parameter.
	float diff = Time - T0;
	var meanAnomaly = meanAnomalyAtT0 + body.OrbitData.MeanMotion * diff;
	body.OrbitData.SetMeanAnomaly(meanAnomaly);

	// Update transform from orbit state.
	body.ForceUpdateViewFromInternalState();
```
```cs
	//Get orbit points for orbit line in array.
	var array = body.OrbitData.GetOrbitPoints();
	//Non-allocating version of same method, which is more efficient in Update methods.
	body.OrbitData.GetOrbitPointsNoAlloc(ref array, pointsCount: 100, origin: body.AttractorSettings.AttractorObject.transform);
```
```cs
	//Get orbit point at certain anomaly angle, without changing current orbit state.
	//Useful for manual sampling of orbit points.
	var position = body.OrbitData.GetFocalPositionAtEccentricAnomaly(anomalyValue);
```

## EllipticInterceptionSolver usage

This component can calculate transfer trajectory in first approach. Functionality is limited to just display curve, which can be used as helper guidline for initial transfer planning process (which is not implemented here).

	* Open or create scene with at least two orbiting bodies;
	* Orbiting bodies must have mutual attractor directly or indirectly (attractor of attractor of attractor etc..);
	* Attach EllipticInterceptionSolver component to one of orbiting bodies;
	* Assign reference to another body inside component inspector;
	* Transfer orbit green curve will appear;

Note 1: If you need to calculate transfer trajectory from circular orbit around planet to another circular orbit around another planet (i.e. 'parking orbit'), 
then you can create dummy celestial body with prefered orbit parameters, and then use it for tranfer calculations.

Note 2: Transfer calculator will take into account orbital motion of orbits and calculate proper
ending position vector after transfer duration period ending. But if resulting duration will not match target duration, 
then ending point will not match real ending point.
In other words, the bigger is difference between target and real duration the bigger is ending point error.
This will be a problem in case, when target duration is 0, because algorithm will then calculate minimal possible duration, which will always be bigger than 0.
To solve this, you can use 'set real target duration' button, which will assign real duration to target duration
(it may requre multiple uses, for better convergence of resulting duration and reducing value difference).

## Contacts

If you have any questions about this plugin, feel free to write your feedback on **itanksp@gmail.com**
