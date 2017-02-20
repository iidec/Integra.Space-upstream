using Orleans;
using Orleans.Runtime.Configuration;
using Orleans.TestingHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.Testing
{
	/// <summary>
	/// Permite crear un Cluster de pruebas. Los silos que pertenecen al cluster son creados de forma dinamica.
	/// </summary>
	public sealed class TestCluster : IDisposable
	{
		private Orleans.TestingHost.TestCluster cluster;
		private TestClusterOptions clusterOptions;
		private int siloCounter = 0;

		public ClusterConfiguration ClusterConfiguration
		{
			get
			{
				return this.Options.ClusterConfiguration;
			}
		}

		public ClientConfiguration ClientConfiguration
		{
			get
			{
				return this.Options.ClientConfiguration;
			}
		}

		private TestClusterOptions Options
		{
			get
			{
				if (this.clusterOptions == null)
				{

					// Se crea una configuración por defecto, donde hay 1 solo silo.
					this.clusterOptions = new TestClusterOptions(1);

					// Se carga la configuración por defecto desde el xml.
					this.clusterOptions.ClusterConfiguration.StandardLoad();

					// Agrega un nodo para que el cluster pueda levantar.
					// this.clusterOptions.ClusterConfiguration.CreateNodeConfigurationForSilo("Primary");

					// Se carga la configuración por defecto desde el xml.
					this.clusterOptions.ClientConfiguration = ClientConfiguration.StandardLoad();
				}

				return clusterOptions;
			}
		}

		private Orleans.TestingHost.TestCluster Cluster
		{
			get
			{
				if (cluster == null)
					cluster = new Orleans.TestingHost.TestCluster(this.Options);

				return cluster;
			}
		}

		public SiloHandle DeploySilo()
		{
			if (++siloCounter == 1)
			{
				this.Cluster.Deploy();
				return this.Cluster.Primary;
			}
			else
			{
				return this.Cluster.StartAdditionalSilo();
			}
		}

		public Task StabilizeClusterDeployAsync()
		{
			return Cluster.WaitForLivenessToStabilizeAsync();
		}

		public void KillSilo(SiloHandle silo)
		{
			this.Cluster.KillSilo(silo);
		}

		public IGrainFactory GrainFactory
		{
			get
			{
				return this.Cluster.GrainFactory;
			}
		}

		public IEnumerable<SiloHandle> GetActiveSilos()
		{
			return this.Cluster.GetActiveSilos();
		}

		public void Dispose()
		{
			if (this.Cluster != null)
			{
				this.Cluster.StopAllSilos();
			}
		}
	}
}
