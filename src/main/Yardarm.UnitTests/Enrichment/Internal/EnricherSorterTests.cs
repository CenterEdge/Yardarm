using System;
using System.Linq;
using Xunit;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Internal;

namespace Yardarm.UnitTests.Enrichment.Internal
{
    public class EnricherSorterTests
    {
        #region Basic

        [Fact]
        public void Basic_Null_NullArgumentException()
        {
            // Act/Assert

            var ex = Assert.Throws<ArgumentNullException>(
                () => EnricherSorter.Default.Sort(((IEnricher[]) null)!).ToList());

            Assert.Equal("enrichers", ex.ParamName);
        }

        [Fact]
        public void Basic_EmptyList_EmptyList()
        {
            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] { }).ToList();

            // Assert

            Assert.Empty(result);
        }

        [Fact]
        public void Basic_SingleItem_Returns()
        {
            // Arrange

            var enricher1 = new Enricher1();

            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] { enricher1 }).ToList();

            // Assert

            Assert.Single(result);
            Assert.Equal(result[0], enricher1);
        }

        [Fact]
        public void Basic_EmptyTypes_AnyOrder()
        {
            // Arrange

            var enricher1 = new Enricher1();
            var enricher2 = new Enricher2();
            var enricher3 = new Enricher3();

            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList();

            // Assert

            Assert.Equal(3, result.Count);
        }

        #endregion

        #region ExecuteAfter

        [Fact]
        public void ExecuteAfter_OneDependsOnThree_ThreeBeforeOne()
        {
            // Arrange

            var enricher1 = new Enricher1 {ExecuteAfter = new[] {typeof(Enricher3)}};
            var enricher2 = new Enricher2();
            var enricher3 = new Enricher3();

            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList();

            // Assert

            Assert.Equal(3, result.Count);

            var index1 = result.IndexOf(enricher1);
            var index3 = result.IndexOf(enricher3);

            Assert.True(index3 < index1);
        }

        [Fact]
        public void ExecuteAfter_ThreeDependsOnOne_OneBeforeThree()
        {
            // Arrange

            var enricher1 = new Enricher1();
            var enricher2 = new Enricher2();
            var enricher3 = new Enricher3 {ExecuteAfter = new[] {typeof(Enricher1)}};

            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList();

            // Assert

            Assert.Equal(3, result.Count);

            var index1 = result.IndexOf(enricher1);
            var index3 = result.IndexOf(enricher3);

            Assert.True(index1 < index3);
        }

        [Fact]
        public void ExecuteAfter_TwoDependsOnBoth_TwoIsLast()
        {
            // Arrange

            var enricher1 = new Enricher1();
            var enricher2 = new Enricher2 {ExecuteAfter = new[] {typeof(Enricher1), typeof(Enricher3)}};
            var enricher3 = new Enricher3();

            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList();

            // Assert

            Assert.Equal(3, result.Count);

            var index2 = result.IndexOf(enricher2);

            Assert.Equal(2, index2);
        }

        [Fact]
        public void ExecuteAfter_Chained_Sorted()
        {
            // Arrange

            var enricher1 = new Enricher1 {ExecuteAfter = new[] {typeof(Enricher2)}};
            var enricher2 = new Enricher2 {ExecuteAfter = new[] {typeof(Enricher3)}};
            var enricher3 = new Enricher3();

            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList();

            // Assert

            Assert.Equal(3, result.Count);

            Assert.Equal(enricher1, result[2]);
            Assert.Equal(enricher2, result[1]);
            Assert.Equal(enricher3, result[0]);
        }

        [Fact]
        public void ExecuteAfter_CircularReference_InvalidOperationException()
        {
            // Arrange

            var enricher1 = new Enricher1 {ExecuteAfter = new[] {typeof(Enricher2)}};
            var enricher2 = new Enricher2 {ExecuteAfter = new[] {typeof(Enricher1)}};
            var enricher3 = new Enricher3();

            // Act/Assert


            Assert.Throws<InvalidOperationException>(
                () => EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList());
        }

        #endregion

        #region ExecuteAfter

        [Fact]
        public void ExecuteBefore_OneDependsOnThree_ThreeBeforeOne()
        {
            // Arrange

            var enricher1 = new Enricher1();
            var enricher2 = new Enricher2();
            var enricher3 = new Enricher3 {ExecuteBefore = new[] {typeof(Enricher1)}};

            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList();

            // Assert

            Assert.Equal(3, result.Count);

            var index1 = result.IndexOf(enricher1);
            var index3 = result.IndexOf(enricher3);

            Assert.True(index3 < index1);
        }

        [Fact]
        public void ExecuteBefore_ThreeDependsOnOne_OneBeforeThree()
        {
            // Arrange

            var enricher1 = new Enricher1 {ExecuteBefore = new[] {typeof(Enricher3)}};
            var enricher2 = new Enricher2();
            var enricher3 = new Enricher3();

            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList();

            // Assert

            Assert.Equal(3, result.Count);

            var index1 = result.IndexOf(enricher1);
            var index3 = result.IndexOf(enricher3);

            Assert.True(index1 < index3);
        }

        [Fact]
        public void ExecuteBefore_TwoDependsOnBoth_TwoIsLast()
        {
            // Arrange

            var enricher1 = new Enricher1 {ExecuteBefore = new[] {typeof(Enricher2)}};
            var enricher2 = new Enricher2();
            var enricher3 = new Enricher3 {ExecuteBefore = new[] {typeof(Enricher2)}};

            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList();

            // Assert

            Assert.Equal(3, result.Count);

            var index2 = result.IndexOf(enricher2);

            Assert.Equal(2, index2);
        }

        [Fact]
        public void ExecuteBefore_Chained_Sorted()
        {
            // Arrange

            var enricher1 = new Enricher1();
            var enricher2 = new Enricher2 {ExecuteBefore = new[] {typeof(Enricher1)}};
            var enricher3 = new Enricher3 {ExecuteBefore = new[] {typeof(Enricher1)}};

            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList();

            // Assert

            Assert.Equal(3, result.Count);

            Assert.Equal(enricher1, result[2]);
            Assert.Equal(enricher2, result[1]);
            Assert.Equal(enricher3, result[0]);
        }

        [Fact]
        public void ExecuteBefore_CircularReference_InvalidOperationException()
        {
            // Arrange

            var enricher1 = new Enricher1 {ExecuteBefore = new[] {typeof(Enricher2)}};
            var enricher2 = new Enricher2 {ExecuteBefore = new[] {typeof(Enricher1)}};
            var enricher3 = new Enricher3();

            // Act/Assert


            Assert.Throws<InvalidOperationException>(
                () => EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList());
        }

        #endregion

        #region Mixed

        [Fact]
        public void Mixed_TwoDependsOnBoth_TwoIsLast()
        {
            // Arrange

            var enricher1 = new Enricher1 {ExecuteBefore = new[] {typeof(Enricher2)}};
            var enricher2 = new Enricher2 {ExecuteAfter = new[] {typeof(Enricher3)}};
            var enricher3 = new Enricher3();

            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList();

            // Assert

            Assert.Equal(3, result.Count);

            var index2 = result.IndexOf(enricher2);

            Assert.Equal(2, index2);
        }

        [Fact]
        public void Mixed_Chained_Sorted()
        {
            // Arrange

            var enricher1 = new Enricher1 {ExecuteAfter = new[] {typeof(Enricher2)}};
            var enricher2 = new Enricher2();
            var enricher3 = new Enricher3 {ExecuteBefore = new[] {typeof(Enricher2)}};

            // Act

            var result = EnricherSorter.Default.Sort(new IEnricher[] {enricher1, enricher2, enricher3}).ToList();

            // Assert

            Assert.Equal(3, result.Count);

            Assert.Equal(enricher1, result[2]);
            Assert.Equal(enricher2, result[1]);
            Assert.Equal(enricher3, result[0]);
        }

        #endregion

        #region Helpers

        private class Enricher1 : IEnricher
        {
            public Type[] ExecuteAfter { get; init; } = Type.EmptyTypes;
            public Type[] ExecuteBefore { get; init; } = Type.EmptyTypes;
        }

        private class Enricher2 : IEnricher
        {
            public Type[] ExecuteAfter { get; init; } = Type.EmptyTypes;
            public Type[] ExecuteBefore { get; init; } = Type.EmptyTypes;
        }

        private class Enricher3 : IEnricher
        {
            public Type[] ExecuteAfter { get; init; } = Type.EmptyTypes;
            public Type[] ExecuteBefore { get; init; } = Type.EmptyTypes;
        }

        #endregion
    }
}
