using Orleans;
using Orleans.TestingHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integra.Space.Testing
{
	public abstract class TestCluster : IDisposable
	{
		private TestingSiloHost siloHost;
		private TestingSiloOptions defaultSiloOptions;

		protected TestingSiloOptions DefaultSiloOptions
		{
			get
			{
				if (this.defaultSiloOptions == null)
				{
					this.defaultSiloOptions = new TestingSiloOptions
					{
						// Valores por default usados cuando se crea el primer silo del cluster
						StartFreshOrleans = true,
						StartPrimary = true,
						ParallelStart = false,
						StartSecondary = false//,
											  //LivenessType = Orleans.Runtime.Configuration.GlobalConfiguration.LivenessProviderType.MembershipTableGrain
											  //ReminderServiceType = Orleans.Runtime.Configuration.GlobalConfiguration.ReminderServiceProviderType.SqlServer,
											  //LivenessType = Orleans.Runtime.Configuration.GlobalConfiguration.LivenessProviderType.SqlServer
					};
				}

				return this.defaultSiloOptions;
			}
			set
			{
				this.defaultSiloOptions = value;
			}
		}

		protected SiloHandle StartPrimarySilo()
		{
			return this.StartPrimarySilo(this.DefaultSiloOptions);
		}

		protected SiloHandle StartPrimarySilo(TestingSiloOptions options)
		{
			if (this.siloHost == null || siloHost.Primary == null)
			{
				try
				{
					this.siloHost = new TestingSiloHost(options);
				}
				catch (Exception e)
				{
					var ex = e;
					throw e;
				}
			}

			return this.siloHost.Primary;
		}

		protected SiloHandle StartSecondarySilo()
		{
			// Se setean los valores para crear el segundo silo del cluster
			this.DefaultSiloOptions.StartFreshOrleans = false;
			this.DefaultSiloOptions.StartPrimary = false;
			this.DefaultSiloOptions.ParallelStart = false;
			this.DefaultSiloOptions.StartSecondary = true;
			this.DefaultSiloOptions.BasePort++;
			return this.StartSecondarySilo(this.DefaultSiloOptions);
		}

		protected SiloHandle StartSecondarySilo(TestingSiloOptions options)
		{
			if (this.siloHost.Secondary == null)
			{
				this.siloHost.StartSecondarySilo(options, 0);
			}

			return this.siloHost.Secondary;
		}

		protected SiloHandle StartAdditionalSilo()
		{
			return this.siloHost.StartAdditionalSilo();
		}

		protected void KillSilo(SiloHandle silo)
		{
			this.siloHost.KillSilo(silo);
		}

		protected IGrainFactory GrainFactory
		{
			get
			{
				return this.siloHost.GrainFactory;
			}
		}

		protected IEnumerable<SiloHandle> GetActiveSilos()
		{
			return this.siloHost.GetActiveSilos();
		}

		public virtual void Dispose()
		{
			if (this.siloHost != null)
			{
				this.siloHost.StopAllSilos();
			}
		}
	}
}
