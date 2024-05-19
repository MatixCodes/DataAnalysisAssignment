using System;
using Xunit;
using Data.Models;

namespace DataSetProcessingTests
{
    public class DataSetProcessorTests
    {
        [Fact]
        public void ConvertData_ValidInputs_LinearInterpolation_Addition()
        {
            var ds1 = new DataSet
            {
                TimeArray = new double[] { 1, 2, 3 },
                ValueArray = new double[] { 10, 20, 30 }
            };
            var ds2 = new DataSet
            {
                TimeArray = new double[] { 1, 2, 3 },
                ValueArray = new double[] { 1, 2, 3 }
            };

            var result = DataSetProcessor.ConvertData("TestChannel", ds1, ds2, OperationType.Addition, DataConversionType.LinearInterpolation);

            Assert.Equal(new double[] { 11, 22, 33 }, result.ValueArray);
        }

        [Fact]
        public void ConvertData_NullDataSet_ThrowsArgumentNullException()
        {
            var ds1 = new DataSet
            {
                TimeArray = new double[] { 1, 2, 3 },
                ValueArray = new double[] { 10, 20, 30 }
            };

            Assert.Throws<ArgumentNullException>(() => DataSetProcessor.ConvertData("TestChannel", ds1, null, OperationType.Addition, DataConversionType.LinearInterpolation));
        }

        [Fact]
        public void ConvertData_ValidInputs_DifferentLengthes_LinearInterpolation_Addition()
        {
            var ds1 = new DataSet
            {
                TimeArray = new double[] { 1, 2, 3, 4 },
                ValueArray = new double[] { 10, 20, 30, 40 }
            };
            var ds2 = new DataSet
            {
                TimeArray = new double[] { 1, 2, 3 },
                ValueArray = new double[] { 1, 2, 3 }
            };

            var result = DataSetProcessor.ConvertData("TestChannel", ds1, ds2, OperationType.Addition, DataConversionType.LinearInterpolation);

            Assert.Equal(new double[] { 11, 22, 33 }, result.ValueArray);
        }

        [Fact]
        public void ConvertData_DivisionByZero_ThrowsDivideByZeroException()
        {
            var ds1 = new DataSet
            {
                TimeArray = new double[] { 1, 2, 3 },
                ValueArray = new double[] { 10, 20, 30 }
            };
            var ds2 = new DataSet
            {
                TimeArray = new double[] { 1, 2, 3 },
                ValueArray = new double[] { 1, 0, 1 }
            };

            Assert.Throws<DivideByZeroException>(() => DataSetProcessor.ConvertData("TestChannel", ds1, ds2, OperationType.Division, DataConversionType.None));
        }

        [Fact]
        public void ConvertData_InvalidDataConversionType_ThrowsArgumentOutOfRangeException()
        {
            var ds1 = new DataSet
            {
                TimeArray = new double[] { 1, 2, 3 },
                ValueArray = new double[] { 10, 20, 30 }
            };
            var ds2 = new DataSet
            {
                TimeArray = new double[] { 1, 2, 3 },
                ValueArray = new double[] { 1, 2, 3 }
            };

            var invalidConversionType = (DataConversionType)999;

            Assert.Throws<ArgumentOutOfRangeException>(() => DataSetProcessor.ConvertData("TestChannel", ds1, ds2, OperationType.Addition, invalidConversionType));
        }
    }
}