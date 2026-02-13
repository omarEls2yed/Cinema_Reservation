using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Exceptions
{
    public sealed class SeatNotFoundException(int id) : NotFoundException($"The seat with {id} is not found")
    {

    }
}
