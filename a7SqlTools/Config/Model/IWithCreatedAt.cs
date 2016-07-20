using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.Config.Model
{
    public interface IWithCreatedAt
    {
        DateTime CreatedAt { get; }
    }
}
