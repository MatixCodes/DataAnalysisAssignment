using System.Linq;
using System;
using Data.Models;
using MathNet.Numerics.Interpolation;


public class DataSetResult
{
    public DataSet DataSet1 { get; set; }
    public DataSet DataSet2 { get; set; }
}

public class DataSetProcessor
{
    public static DataSet ConvertData(string channelName,DataSet ds1, DataSet ds2,OperationType operationType, DataConversionType dataConversionType)
    {
        DataSet generatedDataset = new DataSet();

        int minRecordCount = Math.Min(ds1.TimeArray.Length, ds2.TimeArray.Length);
        var times1 = ds1.TimeArray.Take(minRecordCount).ToArray();
        var values1 = ds1.ValueArray.Take(minRecordCount).ToArray();
        var times2 = ds2.TimeArray.Take(minRecordCount).ToArray();
        var values2 = ds2.ValueArray.Take(minRecordCount).ToArray();

       


        double[] interpolatedValues2;
        
        
        // Perform interpolation based on the specified type
        switch (dataConversionType)
        {
            case DataConversionType.LinearInterpolation:

                interpolatedValues2 = LinearInterpolate(times2, values2, times1);
                
                generatedDataset = createNewDataSet(channelName, times1,values1, interpolatedValues2, operationType);
                break;
            case DataConversionType.CubicSplineInterpolation:
                interpolatedValues2 = CubicSplineInterpolate(times2, values2, times1);
                
                generatedDataset = createNewDataSet(channelName, times1, values1, interpolatedValues2, operationType);
                break;
            case DataConversionType.None:
                generatedDataset = createNewDataSet(channelName, times1, values1, values2, operationType);
                break;
            default:
                break;
        }
        return generatedDataset;        
    }
    

    public static DataSet createNewDataSet(string name, double[] timeArray, double[] valueArray1, double[] valueArrayy2, OperationType operationType)
    {
        DataSet result = new DataSet();

        result.ChannelName = name;
        result.TimeArray = timeArray;
        result.ValueArray = PerformOperation(valueArray1, valueArrayy2, operationType);
        result.Selected = true;

        return result;
    }
    
    private static double[] LinearInterpolate(double[] xValues, double[] yValues, double[] newXValues)
    {
        double[] yValuesNonNull = yValues.Select(y => y).ToArray(); // Replace null values with 0.0
        var interpolatedValues = new double[newXValues.Length];

        for (int i = 0; i < newXValues.Length; i++)
        {
            double x = newXValues[i];
            int index = Array.BinarySearch(xValues, x);

            if (index >= 0)
            {
                // Exact match found, take the corresponding y value
                interpolatedValues[i] = yValuesNonNull[index];
            }
            else
            {
                index = ~index; // Convert negative index to positive index

                if (index == 0)
                {
                    // Extrapolate left
                    interpolatedValues[i] = yValuesNonNull[0];
                }
                else if (index == xValues.Length)
                {
                    // Extrapolate right
                    interpolatedValues[i] = yValuesNonNull[xValues.Length - 1];
                }
                else
                {
                    // Perform linear interpolation
                    double x0 = xValues[index - 1];
                    double x1 = xValues[index];
                    double y0 = yValuesNonNull[index - 1];
                    double y1 = yValuesNonNull[index];

                    interpolatedValues[i] = y0 + (x - x0) * (y1 - y0) / (x1 - x0);
                }
            }
        }

        return interpolatedValues;
    }

    private static double[] CubicSplineInterpolate(double[] xValues, double[] yValues, double[] newXValues)
    {
        double[] yValuesNonNull = yValues.Select(y => y).ToArray(); // Replace null values with 0.0
        var interpolatedValues = new double[newXValues.Length];

        var spline = CubicSpline.InterpolateAkimaSorted(xValues, yValuesNonNull);

        for (int i = 0; i < newXValues.Length; i++)
        {
            double x = newXValues[i];
            interpolatedValues[i] = spline.Interpolate(x); // Use cubic spline interpolation
        }

        return interpolatedValues;
    }
    public static double[] PerformOperation(double[] dataSet1Values, double[] dataSet2Values, OperationType operationType)
    {
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
        switch (operationType)
        {
            case OperationType.Addition:
                return value1 + value2;
            case OperationType.Subtraction:
                return value1 - value2;
            case OperationType.Multiplication:
                return value1 * value2;
            case OperationType.Division:
                if (value2 == 0)
                {
                    throw new DivideByZeroException("Division by zero");
                }
                return value1 / value2;
            default:
                throw new ArgumentException("Invalid operation type");
        }
    }
}
