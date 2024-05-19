using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


public class DataSetValidator
{
    public bool DoTimesAlign(DataSet dataSet1, DataSet dataSet2)
    {
        bool timesAlign = dataSet1.TimeArray.SequenceEqual(dataSet2.TimeArray);
        return timesAlign;
    }
}

