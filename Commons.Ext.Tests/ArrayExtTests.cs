using NUnit.Framework;
using System;

namespace Commons.Ext
{
    [TestFixture]
    [TestOf(typeof(ArrayExt))]
    class ArrayExtTests
    {
        #region TESTS
        [Test]
        public void Slice_NullArray_YieldsNullResult_Test()
        {
            //Arrange
            int[] array = null;

            //Act
            var result = array.Slice(0, 0);

            //Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Slice_EmptyArrayCannotBeSliced_Test()
        {
            //Arrange
            int[] array = new int[0];

            //Act
            var result = array.Slice(0, 0);

            //Assert
            Assert.That(result, Is.Not.SameAs(array).And.EqualTo(array));
        }

        [Test]
        public void Slice_StartIndexOutOfBounds_ThrowsException_Test()
        {
            //Arrange
            var array = new int[] { 1 };

            //Assert
            Assert.That(() => array.Slice(1, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Slice_EndIndexOutOfBounds_ThrowsException_Test()
        {
            //Arrange
            var array = new int[] { 1 };

            //Assert
            Assert.That(() => array.Slice(0, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Slice_StartIndexGreaterThenEndIndex_ThrowsException_Test()
        {
            //Arrange
            var array = new int[] { 1, 2, 3 };

            //Assert
            Assert.That(() => array.Slice(2, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Slice_StartIndexSameAsEndIndex_YieldsArrayOfSingleElement_Test()
        {
            //Arrange
            var array = new int[] { 1, 2, 3 };
            var expectedResult = new int[] { 2 };

            //Act
            var actualResult = array.Slice(1, 1);

            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        /// <summary>
        /// Tests that <see cref="ArrayExt.Slice{T}(T[], int, int)" /> can yield a subsection of array
        /// that does not include the boundary elements.
        /// </summary>
        [Test]
        public void Slice_StartIndexAndEndIndexAreNotBounds_Test()
        {
            //Arrange
            var array = new int[] { 1, 2, 3, 4 };
            var expectedResult = new int[] { 2, 3 };

            //Act
            var actualResult = array.Slice(1, 2);

            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        /// <summary>
        /// Tests that <see cref="ArrayExt.Slice{T}(T[], int, int)" /> can yield a subsection of array
        /// that includes the lower boundary but not the upper boundary elements.
        /// </summary>
        [Test]
        public void Slice_StartIndexIsBoundAndEndIndexIsNotBound_Test()
        {
            //Arrange
            var array = new int[] { 1, 2, 3, 4 };
            var expectedResult = new int[] { 1, 2, 3 };

            //Act
            var actualResult = array.Slice(0, 2);

            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        /// <summary>
        /// Tests that <see cref="ArrayExt.Slice{T}(T[], int, int)" /> can yield a subsection of array
        /// that includes the upper boundary but not the lower boundary elements.
        /// </summary>
        [Test]
        public void Slice_StartIndexIsNotBoundAndEndIndexIsBound_Test()
        {
            //Arrange
            var array = new int[] { 1, 2, 3, 4 };
            var expectedResult = new int[] { 2, 3, 4 };

            //Act
            var actualResult = array.Slice(1, 3);

            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void Slice_WholeArraySlice_YieldsCopyOfOriginalArray_Test()
        {
            //Arrange
            var array = new int[] { 1, 2, 3, 4 };

            //Act
            var actualResult = array.Slice(0, 3);

            //Assert
            Assert.That(actualResult, Is.Not.SameAs(array).And.EqualTo(array));
        }
        #endregion
    }
}
