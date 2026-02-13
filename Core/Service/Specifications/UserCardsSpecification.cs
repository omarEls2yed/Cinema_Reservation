using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Specifications
{
    public class UserCardsSpecification : BaseSpecifications<UserPaymentMethod>
    {
        public UserCardsSpecification(int userId) : base(card => card.UserId == userId)
        {
            AddOrderBy(x => x.Id);
        }
    }
}
