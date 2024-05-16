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
    public static DataSetResult InterpolateDataSets(DataSet ds1, DataSet ds2, int interpolationType)
    {
        DataSetValidator dsValidator = new DataSetValidator();

        bool timesAlign = dsValidator.DoTimesAlign(ds1, ds2);

        int minRecordCount = Math.Min(ds1.TimeArray.Length, ds2.TimeArray.Length);
        var times1 = ds1.TimeArray.Take(minRecordCount).ToArray();
        var values1 = ds1.ValueArray.Take(minRecordCount).ToArray();
        var times2 = ds2.TimeArray.Take(minRecordCount).ToArray();
        var values2 = ds2.ValueArray.Take(minRecordCount).ToArray();

        double[] interpolatedValues2;
        if (timesAlign)
        {
            // Times align, return the original datasets truncated to the smaller size
            return new DataSetResult
            {
                DataSet1 = new DataSet { ChannelName = ds1.ChannelName, Outing = ds1.Outing, TimeArray = times1, ValueArray = values1, Selected = ds1.Selected },
                DataSet2 = new DataSet { ChannelName = ds2.ChannelName, Outing = ds2.Outing, TimeArray = times2, ValueArray = values2, Selected = ds2.Selected }
            };
        }
        else
        {
            // Perform interpolation based on the specified type
            switch (interpolationType)
            {
                case 1:
                    interpolatedValues2 = LinearInterpolate(times2, values2, times1);
                    break;
                default:
                    interpolatedValues2 = CubicSplineInterpolate(times2, values2, times1);
                    break;
            }

            // Return the interpolated dataset
            return new DataSetResult
            {
                DataSet1 = new DataSet { ChannelName = ds1.ChannelName, Outing = ds1.Outing, TimeArray = times1, ValueArray = values1, Selected = ds1.Selected },
                DataSet2 = new DataSet { ChannelName = ds2.ChannelName, Outing = ds2.Outing, TimeArray = times1, ValueArray = interpolatedValues2.Cast<double?>().ToArray(), Selected = ds2.Selected }
            };
        }
    }

    private static double[] LinearInterpolate(double[] xValues, double?[] yValues, double[] newXValues)
    {
        double[] yValuesNonNull = yValues.Select(y => y ?? 0.0).ToArray(); // Replace null values with 0.0
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

    private static double[] CubicSplineInterpolate(double[] xValues, double?[] yValues, double[] newXValues)
    {
        double[] yValuesNonNull = yValues.Select(y => y ?? 0.0).ToArray(); // Replace null values with 0.0
        var interpolatedValues = new double[newXValues.Length];

        var spline = CubicSpline.InterpolateAkimaSorted(xValues, yValuesNonNull);

        for (int i = 0; i < newXValues.Length; i++)
        {
            double x = newXValues[i];
            interpolatedValues[i] = spline.Interpolate(x); // Use cubic spline interpolation
        }

        return interpolatedValues;
    }


    public static DataSetResult ResampleDataSets(DataSet ds1, DataSet ds2)
    {
        double[] times1 = ds1.TimeArray;
        double[] values1 = ds1.ValueArray.Select(x => x ?? double.NaN).ToArray();

        double[] times2 = ds2.TimeArray;
        double[] values2 = ds2.ValueArray.Select(x => x ?? double.NaN).ToArray();

        if (times1.Length != times2.Length)
        {
            if (times1.Length < times2.Length)
            {
                (times2, values2) = Resample(times2, values2, times1);
            }
            else
            {
                (times1, values1) = Resample(times1, values1, times2);
            }
        }

        return new DataSetResult
        {
            DataSet1 = new DataSet
            {
                ChannelName = ds1.ChannelName,
                Outing = ds1.Outing,
                TimeArray = times1,
                ValueArray = values1.Select(x => double.IsNaN(x) ? (double?)null : x).ToArray(),
                Selected = ds1.Selected
            },
            DataSet2 = new DataSet
            {
                ChannelName = ds2.ChannelName,
                Outing = ds2.Outing,
                TimeArray = times2,
                ValueArray = values2.Select(x => double.IsNaN(x) ? (double?)null : x).ToArray(),
                Selected = ds2.Selected
            }
        };
    }

    private static (double[] times, double[] values) Resample(double[] srcTimes, double[] srcValues, double[] targetTimes)
    {
        double[] targetValues = new double[targetTimes.Length];

        int j = 0;
        for (int i = 0; i < targetTimes.Length; i++)
        {
            double targetTime = targetTimes[i];

            // Find the closest time points in the source data for linear interpolation
            while (j < srcTimes.Length - 1 && srcTimes[j + 1] < targetTime)
            {
                j++;
            }

            // Ensure the indices are within bounds
            int index0 = Math.Max(j, 0);
            int index1 = Math.Min(j + 1, srcTimes.Length - 1);

            double t0 = srcTimes[index0];
            double t1 = srcTimes[index1];

            double v0 = srcValues[index0];
            double v1 = srcValues[index1];

            targetValues[i] = LinearInterpolation(t0, v0, t1, v1, targetTime);
        }

        return (targetTimes, targetValues);
    }

    private static double LinearInterpolation(double x0, double y0, double x1, double y1, double x)
    {
        // Perform linear interpolation
        return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
    }
    public double? PerformOperation(double? value1, double? value2, OperationType operationType)
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
                return value1 / value2;
            default:
                throw new ArgumentException("Invalid operation type");
        }
    }
}
