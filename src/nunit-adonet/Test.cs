using NUnit.Framework;
using System;
using System.Configuration;
using System.Data.Common;

namespace nunitadonet
{
	[TestFixture ()]
	public class Test
	{
		[Test]
		public void VerifyAppDomainHasConfigurationSettings()
		{
			string value = ConfigurationManager.AppSettings["MyTestConfig"];
			Assert.IsFalse(String.IsNullOrEmpty(value), "No App.Config found.");
		}

		[Test ()]
		public void Test_DbProviderFactories_GetFactory()
		{
			var cs = ConfigurationManager.ConnectionStrings["MyConnectionString"];
			var providerName = cs.ProviderName;
			var factory = DbProviderFactories.GetFactory(providerName); 			
			Assert.IsNotNull (factory);
		}
	}
}
