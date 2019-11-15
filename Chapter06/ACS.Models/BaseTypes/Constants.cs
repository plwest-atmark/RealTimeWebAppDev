using System;
using System.Collections.Generic;
using System.Text;

namespace ASC.Models.BaseTypes
{
    public static class Constants
    {
    }

    /// <summary>
    /// These will be the "types" of users that our website will have. We call these "roles" or "responsibilities".
    /// 
    /// These will be used to determine what a use is able to do, 
    /// </summary>
    public enum Roles
    {
        Admin, Engineer, User
    }
}
