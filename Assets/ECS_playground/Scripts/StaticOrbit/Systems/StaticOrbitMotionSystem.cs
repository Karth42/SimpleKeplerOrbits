using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
using SimpleKeplerOrbits;
using Unity.Mathematics;
using ECS_SpaceShooterDemo;

namespace ECS_Sandbox
{
	public class StaticOrbitMotionSystem : JobComponentSystem
	{
		[BurstCompile]
		private struct EllipticAnomaliesTimeProgressionJob : IJobProcessComponentData<EccentricityData, OrbitPeriodData, AnomalyData>
		{
			public double dt;

			public void Execute([ReadOnly]ref EccentricityData eccData, [ReadOnly]ref OrbitPeriodData periodData, ref AnomalyData anomaliesData)
			{
				double eccentricity = eccData.Eccentricity;
				if (eccentricity < 1.0)
				{
					double period = periodData.Period;
					double meanAnomaly = anomaliesData.MeanAnomaly += KeplerOrbitUtils.PI_2 * dt / period;
					meanAnomaly %= KeplerOrbitUtils.PI_2;
					if (meanAnomaly < 0)
					{
						meanAnomaly = KeplerOrbitUtils.PI_2 - meanAnomaly;
					}
					double eccentricAnomaly = KeplerOrbitUtils.KeplerSolver(meanAnomaly, eccentricity);
					double cosE = math.cos(eccentricAnomaly);
					double trueAnomaly = math.acos((cosE - eccentricity) / (1.0 - eccentricity * cosE));
					if (meanAnomaly > math.PI)
					{
						trueAnomaly = KeplerOrbitUtils.PI_2 - trueAnomaly;
					}
					anomaliesData.MeanAnomaly = meanAnomaly;
					anomaliesData.TrueAnomaly = trueAnomaly;
					anomaliesData.EccentricAnomaly = eccentricAnomaly;
				}
			}
		}

		[BurstCompile]
		private struct HyperbolicAnomaliesTimeProgressionJob : IJobProcessComponentData<EccentricityData, AttractorData, SemiMinorMajorAxisData, AnomalyData>
		{
			public float dt;
			public double gravitationalConstant;

			public void Execute([ReadOnly]ref EccentricityData eccData, [ReadOnly]ref AttractorData attractorData, [ReadOnly]ref SemiMinorMajorAxisData axisData, ref AnomalyData anomaliesData)
			{
				double eccentricity = eccData.Eccentricity;
				if (eccentricity >= 1.0)
				{
					double n = math.sqrt(attractorData.AttractorMass * gravitationalConstant / axisData.SemiMajorAxisPow3);
					double meanAnomaly = anomaliesData.MeanAnomaly + n * dt;
					double eccentricAnomaly = KeplerOrbitUtils.KeplerSolverHyperbolicCase(meanAnomaly, eccentricity);
					double trueAnomaly = math.atan2(math.sqrt(eccData.EccentricitySqr - 1.0) * math.sinh(eccentricAnomaly), eccentricity - math.cosh(eccentricAnomaly));
					anomaliesData.MeanAnomaly = meanAnomaly;
					anomaliesData.TrueAnomaly = trueAnomaly;
					anomaliesData.EccentricAnomaly = eccentricAnomaly;
				}
			}
		}

		[BurstCompile]
		private struct UpdateFocalPositionsJob : IJobProcessComponentData<EccentricityData, SemiMinorMajorAxisData, AnomalyData, OrbitalPositionData>
		{
			public void Execute([ReadOnly]ref EccentricityData eccData, [ReadOnly]ref SemiMinorMajorAxisData axisData, [ReadOnly]ref AnomalyData anomData, [WriteOnly]ref OrbitalPositionData posData)
			{
				double dirX;
				double dirY;
				if (eccData.Eccentricity < 1.0)
				{
					dirX = math.sin(anomData.EccentricAnomaly) * axisData.SemiMinorAxis;
					dirY = math.cos(anomData.EccentricAnomaly) * axisData.SemiMajorAxis;
				}
				else
				{
					dirX = math.sinh(anomData.EccentricAnomaly) * axisData.SemiMinorAxis;
					dirY = math.cosh(anomData.EccentricAnomaly) * axisData.SemiMajorAxis;
				}
				double3 pos = axisData.SemiMinorAxisBasis * dirX + axisData.SemiMajorAxisBasis * dirY;
				pos += axisData.CenterPoint;
				posData.Position = pos;
			}
		}

		[BurstCompile]
		private struct MoveOrbitPositionToRenderTransform : IJobProcessComponentData<OrbitalPositionData, EntityInstanceRenderData>
		{
			public void Execute([ReadOnly]ref OrbitalPositionData orbitPosData, [WriteOnly]ref EntityInstanceRenderData rendPosData)
			{
				rendPosData.position = (float3)orbitPosData.Position;
			}
		}

		private struct EllipticOrbitGroup
		{
			public ComponentDataArray<EccentricityData> eccentricityData;
			public ComponentDataArray<OrbitPeriodData> periodData;
			public ComponentDataArray<AnomalyData> anomalyData;
		}

		private struct HyperbolicOrbitGroup
		{
			public ComponentDataArray<EccentricityData> eccentricitydata;
			public ComponentDataArray<AttractorData> attractorData;
			public ComponentDataArray<SemiMinorMajorAxisData> axisData;
			public ComponentDataArray<AnomalyData> anomData;
		}

		private struct FocalPositionGroup
		{
			public ComponentDataArray<EccentricityData> eccentrictyData;
			public ComponentDataArray<SemiMinorMajorAxisData> axisData;
			public ComponentDataArray<AnomalyData> anomData;
			public ComponentDataArray<OrbitalPositionData> positionData;
		}

		private struct RendPositionGroup
		{
			public ComponentDataArray<OrbitalPositionData> positionData;
			public ComponentDataArray<EntityInstanceRenderData> rendPosData;
		}

		[Inject]
		private EllipticOrbitGroup ellipticGroup;

		[Inject]
		private HyperbolicOrbitGroup hyperbolicGroup;

		[Inject]
		private FocalPositionGroup focalGroup;

		[Inject]
		private RendPositionGroup rendPosGroup;

		protected override void OnCreateManager()
		{
			//Debug.Log("System create");
			base.OnCreateManager();
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			float timeScale = ECSSceneManager.Instance.Config.TimeScale;
			var ellipticJob = new EllipticAnomaliesTimeProgressionJob { dt = Time.deltaTime * timeScale };
			inputDeps = ellipticJob.Schedule(this, inputDeps);
			var hyperbolicJob = new HyperbolicAnomaliesTimeProgressionJob { dt = Time.deltaTime * timeScale, gravitationalConstant = ECSSceneManager.Instance.Config.GravConstant };
			inputDeps = hyperbolicJob.Schedule(this, inputDeps);
			//TODO: Figure out how to make elliptic and hyperbolic jobs execute in parallel, not in sequence.
			var moveJob = new UpdateFocalPositionsJob();
			inputDeps = moveJob.Schedule(this, inputDeps);
			//TODO: Add transform hierarchy support.
			var rendJob = new MoveOrbitPositionToRenderTransform();
			inputDeps = rendJob.Schedule(this, inputDeps);
			return inputDeps;
		}
	}
}