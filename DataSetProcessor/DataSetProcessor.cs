using System;
using System.Linq;
using Data.Models;
using MathNet.Numerics.Interpolation;

public class DataSetResult
{
    public DataSet DataSet1 { get; set; }
    public DataSet DataSet2 { get; set; }
}

public class DataSetProcessor
{
    public static DataSet ConvertData(string channelName, DataSet ds1, DataSet ds2, OperationType operationType, DataConversionType dataConversionType)
    {
        if (ds1 == null || ds2 == null)
            throw new ArgumentNullException("Input DataSets cannot be null");

        DataSet generatedDataset = new DataSet();

        int minRecordCount = Math.Min(ds1.TimeArray.Length, ds2.TimeArray.Length);
        var times1 = ds1.TimeArray.Take(minRecordCount).ToArray();
        var values1 = ds1.ValueArray.Take(minRecordCount).ToArray();
        var times2 = ds2.TimeArray.Take(minRecordCount).ToArray();
        var values2 = ds2.ValueArray.Take(minRecordCount).ToArray();

        double[] interpolatedValues2;

        switch (dataConversionType)
        {
            case DataConversionType.LinearInterpolation:
                interpolatedValues2 = LinearInterpolate(times2, values2, times1);
                break;
            case DataConversionType.CubicSplineInterpolation:
                interpolatedValues2 = CubicSplineInterpolate(times2, values2, times1);
                break;
            case DataConversionType.None:
                interpolatedValues2 = values2;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataConversionType), "Invalid data conversion type");
        }

        generatedDataset = CreateNewDataSet(channelName, times1, values1, interpolatedValues2, operationType);
        return generatedDataset;
    }

    private static DataSet CreateNewDataSet(string name, double[] timeArray, double[] valueArray1, double[] valueArray2, OperationType operationType)
    {
        DataSet result = new DataSet
        {
            ChannelName = name,
            TimeArray = timeArray,
            ValueArray = PerformOperation(valueArray1, valueArray2, operationType),
            Selected = true
        };

        return result;
    }

    private static double[] LinearInterpolate(double[] xValues, double[] yValues, double[] newXValues)
    {
        if (xValues.Length != yValues.Length)
            throw new ArgumentException("xValues and yValues arrays must have the same length");

        double[] interpolatedValues = new double[newXValues.Length];
        for (int i = 0; i < newXValues.Length; i++)
        {
            double x = newXValues[i];
            int index = Array.BinarySearch(xValues, x);

            if (index >= 0)
            {
                interpolatedValues[i] = yValues[index];
            }
            else
            {
                index = ~index;

                if (index == 0)
                {
                    interpolatedValues[i] = yValues[0];
                }
                else if (index == xValues.Length)
                {
                    interpolatedValues[i] = yValues[xValues.Length - 1];
                }
                else
                {
                    double x0 = xValues[index - 1];
                    double x1 = xValues[index];
                    double y0 = yValues[index - 1];
                    double y1 = yValues[index];
                    interpolatedValues[i] = y0 + (x - x0) * (y1 - y0) / (x1 - x0);
                }
            }
        }
        return interpolatedValues;
    }

    private static double[] CubicSplineInterpolate(double[] xValues, double[] yValues, double[] newXValues)
    {
        if (xValues.Length != yValues.Length)
            throw new ArgumentException("xValues and yValues arrays must have the same length");

        var spline = CubicSpline.InterpolateAkimaSorted(xValues, yValues);
        return newXValues.Select(spline.Interpolate).ToArray();
    }

    public static double[] PerformOperation(double[] dataSet1Values, double[] dataSet2Values, OperationType operationType)
    {
        if (dataSet1Values.Length != dataSet2Values.Length)
            throw new ArgumentException("DataSet value arrays must have the same length");

        int length = dataSet1Values.Length;
        double[] result = new double[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = PerformSingleOperation(dataSet1Values[i], dataSet2Values[i], operationType);
        }

        return result;
    }

    private static double PerformSingleOperation(double value1, double value2, OperationType operationType)
    {
        return operationType switch
        {
            OperationType.Addition => value1 + value2,
            OperationType.Subtraction => value1 - value2,
            OperationType.Multiplication => value1 * value2,
            OperationType.Division => value2 == 0 ? throw new DivideByZeroException("Division by zero") : value1 / value2,
            _ => throw new ArgumentException("Invalid operation type"),
        };
    }
}
