using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace NHibernate.Test.Linq.ReadWrite
{
    /// <summary>
	///     Self-contained setup
	/// </summary>
    [TestFixture]
	public class LinqQueriesReadWriteTests : TestCase
    {
	    private ProductDefinition _productDefinition1;
	    private ProductDefinition _productDefinition2;
	    private Material _material1;
	    private Material _material2;

		protected override string[] Mappings
		{
			get { return new[] {"Linq.ReadWrite.ProductMaterial.hbm.xml"}; }
		}

		/// <inheritdoc />
		protected override string MappingsAssembly => MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Name;

		protected override void OnSetUp()
		{
			_productDefinition1 = new ProductDefinition() { Id = 1000, MaterialDefinition = new MaterialDefinition { Id = 1 } };
			_productDefinition2 = new ProductDefinition() { Id = 1001, MaterialDefinition = new MaterialDefinition { Id = 2 } };
			_material1 = new Material { Id = 1, MaterialDefinition = _productDefinition1.MaterialDefinition, ProductDefinition = _productDefinition1 };
			_material2 = new Material { Id = 2, MaterialDefinition = _productDefinition2.MaterialDefinition, ProductDefinition = _productDefinition2 };

			using (var session = OpenSession(true))
			{
				session.Save(_productDefinition1);
				session.Save(_productDefinition2);
				session.Save(_material1);
				session.Save(_material2);

				session.Transaction.Commit();
			}
			base.OnSetUp();
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession(true))
			{
				session.Delete(_material1);
				session.Delete(_material2);
				session.Delete(_productDefinition1);
				session.Delete(_productDefinition2);
				session.Delete(_productDefinition1.MaterialDefinition);
				session.Delete(_productDefinition2.MaterialDefinition);

				session.Transaction.Commit();
			}

			_productDefinition1 = _productDefinition2 = null;
			_material1 = _material2 = null;

			base.OnTearDown();
		}

		[Test(Description = "#2276")]
		public void SelectRelatedOnConstantEvaluated()
		{
			using (var session = OpenSession())
			{
				var selectedProducts = new[] { _productDefinition1 };

				var query = session.Query<Material>()
					.Where(x => selectedProducts.Contains(x.ProductDefinition) && selectedProducts.Select(y => y.MaterialDefinition).Contains(x.MaterialDefinition));

				var result = query.ToList();

                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(_material1, result.Single());
			}
		}

		[Test(Description = "#2276")]
		public void FilterOnConstantEvaluated()
		{
			var allMaterials = new[] {_material1, _material2};
			var allMaterialDefinitions = allMaterials.Select(x => x.MaterialDefinition).Distinct().ToArray();

			Assume.That(allMaterialDefinitions.Length, Is.GreaterThan(1));

			using (var session = OpenSession())
			{
				var excludeDefinitionId = allMaterialDefinitions.Max(z => z.Id);

				var query = session.Query<Material>()
					.Where(x => allMaterialDefinitions.Where(y => y.Id < allMaterialDefinitions.Max(z => z.Id)).Contains(x.MaterialDefinition));

				var result = query.ToList();

				Assert.IsNotEmpty(result);
				Assert.That(result.Where(x => x.MaterialDefinition.Id == excludeDefinitionId), Is.Empty);

				var expectedResult = new HashSet<Material>(allMaterials.Where(
					x => allMaterialDefinitions.Where(y => y.Id < allMaterialDefinitions.Max(z => z.Id)).Contains(x.MaterialDefinition)));

				Assert.AreEqual(expectedResult.Count, result.Count);

				foreach (var material in result)
					Assert.IsTrue(expectedResult.Contains(material));
			}
		}
	}
}
