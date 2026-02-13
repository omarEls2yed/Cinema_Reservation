using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Exceptions
{
    public sealed class EventNotFoundException (int id) : NotFoundException($"The event with {id} is not found")
    {
    }
}
