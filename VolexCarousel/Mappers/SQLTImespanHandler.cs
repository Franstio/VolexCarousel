using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolexCarousel.Mappers
{
    public class SQLTImespanHandler : SqlMapper.TypeHandler<TimeSpan>
    {
        public override TimeSpan Parse(object value)
        {
            if (value is null || value is DBNull) return TimeSpan.Zero;
            return TimeSpan.Parse(value.ToString()!);
        }

        public override void SetValue(IDbDataParameter parameter, TimeSpan value)
        {
            parameter.Value = value.ToString(@"hh\:mm\:ss") ;
        }
    }
}
