﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NHibernate.Linq;

namespace NHibernate.Test.Linq.ReadWrite
{
    using System.Threading.Tasks;
    /// <summary>
	///     Self-contained setup
	/// </summary>
    [TestFixture]
	public class LinqQueriesReadWriteTestsAsync : TestCase
	{
		private static readonly ProductDefinition productDefinition1 = new ProductDefinition() { Id = 1000, MaterialDefinition = new MaterialDefinition { Id = 1 } };
		private static readonly ProductDefinition productDefinition2 = new ProductDefinition() { Id = 1001, MaterialDefinition = new MaterialDefinition { Id = 2 } };
		private static readonly Material material1 = new Material {Id = 1, MaterialDefinition = productDefinition1.MaterialDefinition, ProductDefinition = productDefinition1};
		private static readonly Material material2 = new Material {Id = 2, MaterialDefinition = productDefinition2.MaterialDefinition, ProductDefinition = productDefinition2};

		protected override string[] Mappings
		{
			get { return new[] {"Linq.ReadWrite.ProductMaterial.hbm.xml"}; }
		}

		/// <inheritdoc />
		protected override string MappingsAssembly => MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Name;

		protected override void OnSetUp()
		{
			using (var session = OpenSession(true))
			{
				session.Save(productDefinition1);
				session.Save(productDefinition2);
				session.Save(material1);
				session.Save(material2);

				session.Transaction.Commit();
			}
			base.OnSetUp();
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession(true))
			{
				session.Delete(material1);
				session.Delete(material2);
				session.Delete(productDefinition1);
				session.Delete(productDefinition2);
				session.Delete(productDefinition1.MaterialDefinition);
				session.Delete(productDefinition2.MaterialDefinition);

				session.Transaction.Commit();
			}

			base.OnTearDown();
		}

		[Test(Description = "#2244")]
		public async Task ExpressionOnConstantEvaluatedAsync()
		{
			using (var session = OpenSession())
			{
				var selectedProducts = new[] { productDefinition1 };

				var query = session.Query<Material>()
					.Where(x => selectedProducts.Contains(x.ProductDefinition) && selectedProducts.Select(y => y.MaterialDefinition).Contains(x.MaterialDefinition));

				var result = await (query.ToListAsync());

                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(material1, result.Single());
			}
		}
	}
}
