using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public enum DataConversionType
    {
        [Description("Linear Interpolation")]
        LinearInterpolation,

        [Description("Cubic Spline Interpolation")]
        CubicSplineInterpolation,

        [Description("None")]
        None
    }
}
