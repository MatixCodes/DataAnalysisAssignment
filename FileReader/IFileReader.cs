using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface IFileReader
{
    void Initialize(string filePath);
    bool InvalidValuesExist();
    void PredictMissingValues();
    void RemoveMissingValues();
    List<DataSet> GetResults();
    
}
