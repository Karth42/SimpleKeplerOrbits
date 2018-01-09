# SimpleKeplerOrbits

![gif](https://i.imgur.com/0dhq9kL.gif)

## Description

Unity3d project for creating objects, moving on eliptic or hyperbolic curves around other objects by Kepler's laws of planetary motion. 
Curves settings set by attractor parameters (mass, GConst) and velocity vector of body, 
which makes this system useful in simulation of static celestial mechanics in games.

## Importing

This repository contains whole Unity Project for basic demonstration setup, 
but if you want to import package into separated project, you can safely copy all files 
from Assets/SimpleKeplerOrbits/ folder into your project folder.
Or you can just import Unity Package from AssetStore page ([link]).

## Usage

Main part of this package - **KeplerMover component**, which will move along orbit path any gameobject it attached to, if orbit settings are valid.
KeplerMover requiers references to two other gameobjects - one will be attractor, and other will be velocity vector handle.
Attractor reference is used to provide position vector in world space for orbit focus point.
**KeplerOrbitLineDisplay** component will help to visualize resulting orbit path, when you adjusting KeplerMover settings.

## Published version

Asset store [link]

[link]: https://www.assetstore.unity3d.com/en/#!/content/97048

## License

[MIT](LICENSE)