using System;
using System.Collections.Generic;
using System.Text;

namespace ASC.Models.BaseTypes
{
    /// <summary>
    /// This is just a "black" interface, but we can use such things to test to see if we should "audit" the repository actions.
    /// 
    /// As there is no need to have any methods, we just use an empty interfaced for type checking.
    /// </summary>
    public interface IAuditTracker
    {
    }
}
