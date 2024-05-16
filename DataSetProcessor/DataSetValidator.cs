using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


public class DataSetValidator
{
    

    public bool CheckForNaNValues(DataSet dataSet)
    {
        if (dataSet.ValueArray == null)
        {
            return false;
        }

        foreach (var value in dataSet.ValueArray)
        {
            if (!value.HasValue || double.IsNaN(value.Value) || double.IsInfinity(value.Value))
            {
                return false;
            }
        }
        return true;
    }

    public bool AreDataSetsSameLength(DataSet dataSet1, DataSet dataSet2)
    {
        // Check if the arrays have the same length
        return dataSet1.TimeArray.Length == dataSet2.TimeArray.Length;
    }

    public bool DoTimesAlign(DataSet dataSet1, DataSet dataSet2)
    {
        bool timesAlign = dataSet1.TimeArray.SequenceEqual(dataSet2.TimeArray);
        return timesAlign;
    }
}

